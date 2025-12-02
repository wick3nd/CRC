using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace w3.CRC
{
    internal class Program
    {
        public static void Main()
        {
            using var stream = File.OpenRead("test.txt");
            var crcStream = new CRC32Stream(stream);

            Console.Write(crcStream.ComputeChecksum(50, 0, (int)stream.Length).ToString("X4"));
        }
    }
}
