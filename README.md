# w3.CRC - a simple to use CRC library
`dotnet add package w3.CRC --version 1.0.0`

## Supported CRC algorithms (atm):
- CRC8 ATM-2
- CRC16 CCITT
- CRC32c (Catagnolli)


## CRC8, CRC16 and CRC32
Recommended for data with less than 256B

The CRC calculation is as follows:
```C#
byte[] data = BitConverter.GetBytes("Hello, world!");  // Input data - byte array
uint crc = CRC8.ComputeChecksum(data);
```
Along with the data validation:
```C#
using var stream = File.OpenRead("testFile.bin");  // Input data - also a byte array
bool isValid = CRC8.Validate(dataWithCRC);
```

## CRC32Stream
The CRC32Stream is different as it uses streams as inputs instead of byte arrays to save memory space when calculating the CRC.

The calculation is as follows:
```C#
var crcStream = new CRC32Stream(inputStream);
uint crc = crcStream.ComputeChecksum(8192, 0, (int)crcStream.Length);
```

And for the validation:
```C#
var crcStream = new CRC32Stream(inputStream);
bool isValid = crcStream.Validate(8192, 0, (int)crcStream.Length);
```

## Planned CRCs:
- [x] None
