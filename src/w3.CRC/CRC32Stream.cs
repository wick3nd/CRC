using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace w3.CRC
{
    public class CRC32Stream : Stream
    {
        private readonly Stream _inner;

        private byte[]? _buffer;

        public override long Length => _inner.Length;
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        
        public override bool CanRead => false;
        public override bool CanWrite => false;
        public override bool CanSeek => false;

        public CRC32Stream(Stream inner)
        {
            if (!inner.CanSeek) throw new NotSupportedException("The stream is not seekable. ");
            _inner = inner;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ComputeChecksum(int blockSize, int offset, int count)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(offset, count);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(blockSize);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            ArgumentOutOfRangeException.ThrowIfNegative(offset);

            uint crc = uint.MaxValue;

            _buffer = new byte[blockSize];
            _inner.Seek(offset, SeekOrigin.Begin);

            while (count > 0)
            {
                int toRead = Math.Min(blockSize, count);
                int read = _inner.Read(_buffer, 0, toRead);

                if (read <= 0) break; int i = 0;

                ref byte start = ref MemoryMarshal.GetReference(_buffer.AsSpan(0, read));

                while (i + 4 <= read)
                {
                    crc = PrecomputedTables.CatagnolliTable[(byte)(crc ^ Unsafe.Add(ref start, i))] ^(crc >> 8);
                    crc = PrecomputedTables.CatagnolliTable[(byte)(crc ^ Unsafe.Add(ref start, i + 1))] ^ (crc >> 8);
                    crc = PrecomputedTables.CatagnolliTable[(byte)(crc ^ Unsafe.Add(ref start, i + 2))] ^ (crc >> 8);
                    crc = PrecomputedTables.CatagnolliTable[(byte)(crc ^ Unsafe.Add(ref start, i + 3))] ^ (crc >> 8);

                    i += 4;
                }

                for (; i < read; i++) crc = (crc >> 8) ^ PrecomputedTables.CatagnolliTable[(byte)(crc ^ Unsafe.Add(ref start, i))];

                count -= read;
                offset += read;
            }

            return ~crc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Validate()
        {
            return false;
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Flush() => _inner.Flush();
    }
}