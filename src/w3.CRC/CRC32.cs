using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace w3.CRC
{
    public class CRC32
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ComputeChecksum(ReadOnlySpan<byte> data)
        {
            uint crc = 0xFFFFFFFF;
            ref byte start = ref MemoryMarshal.GetReference(data);
            int len = data.Length;

            for (int i = 0; i < len; i++) crc = (crc >> 8) ^ PrecomputedTables.CatagnolliTable[((int)crc ^ Unsafe.Add(ref start, i)) & 0xFF];

            return crc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Validate(ReadOnlySpan<byte> dataWithCRC)
        {
            uint expected = (uint)(dataWithCRC[^1] << 24 | dataWithCRC[^2] << 16 | dataWithCRC[^3] << 8 | dataWithCRC[^4]);
            uint actual = ComputeChecksum(dataWithCRC[..^4]);

            return expected == actual;
        }
    }
}