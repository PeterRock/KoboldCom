[中文](/README.md)
### KoboldCom
KoboldCom is serial port communication lib.

Support Custom protocol, async data receive handler and analyzer.

“Async” here means the `SerialPort.DataReceived` event-driven receive/parse pipeline, not C# `async`/`await`.

Support basic HexProtocolAnalyzer and TextProtocolAnalyzer. So you can implement a protocol quickly.

TextProtocolAnalyzer can optionally validate checksums via `CheckData` (same delegate as hex protocols). In a subclass constructor, for NMEA-style `$...*HH`, set `BeginOfLine = "$"`, `EndOfLine = "*"`, and `CheckData = XorCheck`. `CheckLength` defaults to 2 hex ASCII digits after `EndOfLine`. Leave `CheckData` unset to keep existing no-checksum text protocols unchanged. `EndOfLine` is searched after `BeginOfLine`. Complete frames with a bad checksum are discarded from the buffer and are not treated as valid packets.

HexProtocolAnalyzer still defaults to a **1-byte** trailing checksum (`XorCheck`). For a 2-byte binary checksum, set `CheckLength = 2` and `CheckData16` (`Crc16Modbus` or `SumCheck16`). The 16-bit value is compared to the last two bytes in **little-endian** order by default (Modbus CRC-16). Set `CheckBigEndian = true` for high-byte-first frames.

When `Handshake` is `None`, `SerialPortSetting` defaults `RtsEnable` and `DtrEnable` to `true` and applies them on `Open` / `Setting`. Hardware that uses a real handshake mode is unchanged.

### Versioning

This is an intentional major-version break:

- **1.x** — .NET Framework 3.5. Tag **`v1.1.0`** is the Framework snapshot taken when 2.0 was planned (PR #4 CheckData). PR #5 (hex 2-byte checksum / Handshake.None RTS/DTR) later landed on Framework master; use branch **`netfx-1.x`** if you cannot leave Framework.
- **2.x** (current mainline) — `net8.0` + NuGet `System.IO.Ports`, assembly version **2.0.0**, including PR #4 / #5 behavior. Framework 3.5 projects cannot reference 2.x.

Public C# API and Chinese XML docs are preserved as much as practical. This modernization does not rewrite protocol logic.

### Build

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).

```
dotnet build KoboldCom.sln
dotnet run --project verify/TextProtocolAnalyzerCheckData
dotnet run --project verify/HexProtocolAnalyzerCheckLength
dotnet run --project verify/SerialPortHandshakeNone
```

The Demo project is `net8.0-windows` WinForms and should be run on Windows.

### Code Demo
See `/Demo` does

#### Usage 
```
var communicator = new KoboldCom.Communicator(new KoboldCom.SerialPort(), new MyProtocols());
```

### TODO:
- i18n

### Thanks
[Article](http://blog.csdn.net/wuyazhe/article/details/5598945)


### License
MIT
