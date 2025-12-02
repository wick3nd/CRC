using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace w3.CRC
{
    public static class CRC16
    {
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
                crc = UpdateCrc(crc, Unsafe.Add(ref start, i));
                crc = UpdateCrc(crc, Unsafe.Add(ref start, i + 1));
                crc = UpdateCrc(crc, Unsafe.Add(ref start, i + 2));
                crc = UpdateCrc(crc, Unsafe.Add(ref start, i + 3));
            }

            // handle remaining bytes
            for (; i < len; i++) crc = UpdateCrc(crc, Unsafe.Add(ref start, i));

            return crc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort UpdateCrc(ushort crc, byte b)
        {
            int index = (crc >> 8 ^ b) & 0xFF;

            return (ushort)(crc << 8 ^ PrecomputedTables.CCITTTable[index]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Validate(ReadOnlySpan<byte> dataWithCRC)
        {
            ushort expected = (ushort)(dataWithCRC[^1] << 8 | dataWithCRC[^2]);
            ushort actual = ComputeChecksum(dataWithCRC[..^2]);

            return expected == actual;
        }
    }
}