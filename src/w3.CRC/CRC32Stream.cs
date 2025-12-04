using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace w3.CRC
{
    public class CRC32Stream
    {
        private readonly Stream _inner;

        private byte[]? _buffer;

        public CRC32Stream(Stream inStream)
        {
            if (!inStream.CanSeek || !inStream.CanRead) throw new NotSupportedException("The stream is not seekable or readable.");
            _inner = inStream;
        }

        /// <summary>
        /// Computes a CRC32c
        /// </summary>
        /// <param name="blockSize"> Size of the internal buffer that holds the data</param>
        /// <param name="offset"> Offset from the beginning of the file</param>
        /// <param name="count"> Number of Bytes to process</param>
        /// <returns>CRC32c number</returns>
        public uint ComputeChecksum(int blockSize, int offset, int count)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(blockSize);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            ArgumentOutOfRangeException.ThrowIfNegative(offset);

            return ComputeChecksumInternal(blockSize, offset, count);
        }

        private uint ComputeChecksumInternal(int blockSize, int offset, int count)
        {
            _buffer = new byte[blockSize];
            _inner.Seek(offset, SeekOrigin.Begin);

            if (Sse42.IsSupported) return ComputeIntelCRCStream(count);
            else return ComputeAmdCRCStream(count);
        }

        private uint ComputeIntelCRCStream(int count)
        {
            uint crc = 0xFFFFFFFF;

            while (count > 0)
            {
                int toRead = Math.Min(_buffer!.Length, count);
                int read = _inner.Read(_buffer, 0, toRead);

                if (read <= 0) break;

                int i = 0;
                while (i + 4 <= read)
                {
                    crc = Sse42.Crc32(crc, BitConverter.ToUInt32(_buffer, i));
                    i += 4;
                }

                while (i < read)
                {
                    crc = Sse42.Crc32(crc, _buffer[i]);
                    i++;
                }

                count -= read;
            }

            return ~crc;
        }

        private uint ComputeAmdCRCStream(int count)
        {
            uint crc = 0xFFFFFFFF;

            while (count > 0)
            {
                int toRead = Math.Min(_buffer!.Length, count);
                int read = _inner.Read(_buffer, 0, toRead);

                if (read <= 0) break;

                int i = 0;
                while (i + 4 <= read)
                {
                    crc = Crc32.ComputeCrc32C(crc, BitConverter.ToUInt32(_buffer, i));
                    i += 4;
                }

                while (i < read)
                {
                    crc = Crc32.ComputeCrc32C(crc, _buffer[i]);
                    i++;
                }

                count -= read;
            }

            return ~crc;
        }

        // Inferior
        /*
         private uint ComputeCustomCRCStream(int count)
         {
             uint crc = 0xFFFFFFFF;
             int remaining = count;

             while (remaining > 0)
             {
                 int toRead = Math.Min(_buffer!.Length, remaining);
                 int read = _inner.Read(_buffer, 0, toRead);

                 if (read <= 0) break;

                 int i = 0;
                 ref byte start = ref MemoryMarshal.GetReference(_buffer.AsSpan(0, read));

                 while (i + 4 <= read)
                 {
                     crc = PrecomputedTables.CatagnolliTable[(byte)(crc ^ Unsafe.Add(ref start, i))] ^ (crc >> 8);
                     crc = PrecomputedTables.CatagnolliTable[(byte)(crc ^ Unsafe.Add(ref start, i + 1))] ^ (crc >> 8);
                     crc = PrecomputedTables.CatagnolliTable[(byte)(crc ^ Unsafe.Add(ref start, i + 2))] ^ (crc >> 8);
                     crc = PrecomputedTables.CatagnolliTable[(byte)(crc ^ Unsafe.Add(ref start, i + 3))] ^ (crc >> 8);
                     i += 4;
                 }

                 for (; i < read; i++) crc = (crc >> 8) ^ PrecomputedTables.CatagnolliTable[(byte)(crc ^ Unsafe.Add(ref start, i))];

                 remaining -= read;
             }

             return ~crc;
         }
        */

        /// <summary>
        /// Checks if the data is valid
        /// </summary>
        /// <param name="blockSize"> Size of the internal buffer that holds the data</param>
        /// <param name="offset"> Offset from the beginning of the file</param>
        /// <param name="count"> Number of Bytes to process</param>
        /// <returns>A bool signalizing if the CRC is valid</returns>
        public bool Validate(int blockSize, int offset, int count)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(blockSize);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(count-4, 0);
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            ArgumentOutOfRangeException.ThrowIfNegative(offset);

            return ValidateInternal(blockSize, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ValidateInternal(int blockSize, int offset, int count)
        {
            uint recalcCRC;
            uint fileCRC;
            byte[] temp = new byte[4];

            recalcCRC = ComputeChecksum(blockSize, offset, count-4);

            _inner.Read(temp, 0, 4);
            fileCRC = BitConverter.ToUInt32(temp);

            return recalcCRC == fileCRC;
        }
    }
}