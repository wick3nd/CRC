using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace w3.CRC
{
    public static class CRC16
    {
        /// <summary>
        /// Calculates a CRC16 CCITT
        /// </summary>
        /// <param name="data"> Input data</param>
        /// <returns>A CRC16 number</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ComputeChecksum(ReadOnlySpan<byte> data)
        {
            ushort crc = 0xFFFF;

            ref byte start = ref MemoryMarshal.GetReference(data);
            int len = data.Length;

            // unroll loop 4x to reduce per-iteration overhead
            int i = 0;
            for (; i <= len - 4; i += 4)
            {
                crc = (ushort)(crc << 8 ^ PrecomputedTables.CCITTTable[(crc >> 8 ^ Unsafe.Add(ref start, i)) & 0xFF]);
                crc = (ushort)(crc << 8 ^ PrecomputedTables.CCITTTable[(crc >> 8 ^ Unsafe.Add(ref start, i + 1)) & 0xFF]);
                crc = (ushort)(crc << 8 ^ PrecomputedTables.CCITTTable[(crc >> 8 ^ Unsafe.Add(ref start, i + 2)) & 0xFF]);
                crc = (ushort)(crc << 8 ^ PrecomputedTables.CCITTTable[(crc >> 8 ^ Unsafe.Add(ref start, i + 3)) & 0xFF]);
            }

            // handle remaining bytes
            for (; i < len; i++) crc = (ushort)(crc << 8 ^ PrecomputedTables.CCITTTable[(crc >> 8 ^ Unsafe.Add(ref start, i)) & 0xFF]);

            return crc;
        }

        /// <summary>
        /// Checks if the data is valid
        /// </summary>
        /// <param name="dataWithCRC"> Data with CRC</param>
        /// <returns>A bool signalizing if the CRC is valid</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Validate(ReadOnlySpan<byte> dataWithCRC)
        {
            ushort expected = (ushort)(dataWithCRC[^1] << 8 | dataWithCRC[^2]);
            ushort actual = ComputeChecksum(dataWithCRC[..^2]);

            return expected == actual;
        }
    }
}