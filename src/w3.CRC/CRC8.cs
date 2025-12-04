using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace w3.CRC
{
    public class CRC8
    {
        /// <summary>
        /// Calculates a CRC16 CCITT
        /// </summary>
        /// <param name="data"> Input data</param>
        /// <returns>A CRC16 number</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ComputeChecksum(ReadOnlySpan<byte> data)
        {
            byte crc = 0x00;
            ref byte start = ref MemoryMarshal.GetReference(data);

            for (byte i = 0; i < data.Length; i++) crc = PrecomputedTables.ATM2Table[crc ^ Unsafe.Add(ref start, i)];

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
            ReadOnlySpan<byte> originalString = dataWithCRC[..^1];
            byte CRC = dataWithCRC[^1];

            return ComputeChecksum(originalString) == CRC;
        }
    }
}