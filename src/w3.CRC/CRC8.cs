using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace w3.CRC
{
    public class CRC8
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ComputeChecksum(ReadOnlySpan<byte> data)
        {
            byte crc = 0x00;
            ref byte start = ref MemoryMarshal.GetReference(data);

            for (byte i = 0; i < data.Length; i++) crc = PrecomputedTables.ATM2Table[crc ^ Unsafe.Add(ref start, i)];

            return crc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Validate(ReadOnlySpan<byte> dataWithCRC)
        {
            ReadOnlySpan<byte> originalString = dataWithCRC[..^1];
            byte CRC = dataWithCRC[^1];

            return ComputeChecksum(originalString) == CRC;
        }
    }
}