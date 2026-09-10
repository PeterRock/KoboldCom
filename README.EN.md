[中文](README.md)

# KoboldCom

KoboldCom is a C# serial port library. It reads bytes from a serial port, splits frames with one or more protocol analyzers, and maps a complete frame to a data model.

The library includes `HexProtocolAnalyzer` for binary frames and `TextProtocolAnalyzer` for text frames that use start and end markers.

In this documentation, “async” means receive and parse work driven by the `SerialPort.DataReceived` event. It does not mean C# `async` / `await`.

## Requirements

The current version requires the [.NET 8 SDK](https://dotnet.microsoft.com/download). Version 2.0.0 changes the target framework.

| Version | Target | How to get it |
| --- | --- | --- |
| **1.x** | .NET Framework 3.5 | Tag [`v1.1.0`](https://github.com/PeterRock/KoboldCom/releases/tag/v1.1.0). For the later 2-byte hex checksum and RTS/DTR behavior when `Handshake` is `None`, use branch [`netfx-1.x`](https://github.com/PeterRock/KoboldCom/tree/netfx-1.x). Do not upgrade a Framework project to 2.x. |
| **2.x** (current) | `net8.0` + NuGet `System.IO.Ports` | Current `master`. Assembly version **2.0.0**. Includes the 1.x protocol and serial behavior listed above. A .NET Framework 3.5 project cannot reference 2.x. |

The public C# API and Chinese XML docs in 2.0.0 match 1.x. Protocol logic was not rewritten. See [CHANGELOG.md](CHANGELOG.md) for the full list of changes.

A port name is `COM` plus the port number (`SerialPortSetting.Port`). The Demo targets `net8.0-windows`. The Demo runs on Windows only.

## Build

```bash
dotnet build KoboldCom.sln
```

These checks do not need hardware:

```bash
dotnet run --project verify/TextProtocolAnalyzerCheckData
dotnet run --project verify/HexProtocolAnalyzerCheckLength
dotnet run --project verify/SerialPortHandshakeNone
```

The projects check text checksums, hex checksum length, and RTS/DTR defaults when `Handshake` is `None`.

Run the Demo on Windows:

```bash
dotnet run --project Demo
```

Other systems can cross-compile the Demo (`EnableWindowsTargeting` is set) but cannot run the UI. Sample code is in [`Demo/`](Demo/).

## How it works

`Communicator` holds an `ICommunication` (usually `KoboldCom.SerialPort`) and an `IAnalyzerCollection`.

Processing order:

1. `SerialPort.DataReceived` fires.
2. `Communicator` reads the bytes.
3. `Communicator` raises `OnRawDataReceived`.
4. Each `IAnalyzer` in the collection calls `SearchBuffer`.
5. After a complete frame is found, `Analyze()` runs.

A port can register more than one analyzer.

## Usage

### Events

| Event | Type | When it runs |
| --- | --- | --- |
| `OnRawDataReceived` | `Communicator` | A chunk of raw bytes was read. |
| `OnDataAnalyzed` | `ProtocolAnalyzer<T>` | The subclass sets `Valid` to `true`. After a timeout, `Valid` is set back to `false` and the event runs again. |

`ICommunication.OnDataReceived` means the port has bytes to read. `Communicator` already subscribes to that event.

```csharp
var protocols = new MyProtocols();
var communicator = new Communicator(new KoboldCom.SerialPort(), protocols);

communicator.OnRawDataReceived += bytes => { /* raw bytes */ };
protocols.ProtocolText.OnDataAnalyzed += m => { /* text model */ };
protocols.ProtocolBinary.OnDataAnalyzed += m => { /* hex model */ };

communicator.Com.Open(new SerialPortSetting { Port = 2, Baudrate = 9600 });
```

Set the frame format in the subclass constructor. In `Analyze()`, assign `Raw` to `Data`, then set `Valid = true`.

### Serial settings

`SerialPortSetting` defaults:

- `Handshake = None`
- `RtsEnable = true`
- `DtrEnable = true`

RTS/DTR are written when `Setting` is applied. They are written again after `Open`. If `Handshake` is not `None`, the library does not write those lines. Reading the settings then reports both as `false`. To keep a line low, set the matching property to `false`.

### `TextProtocolAnalyzer`

Frame format: `[BeginOfLine][payload][EndOfLine]`. If `CheckData` is set, a checksum follows: `[EndOfLine][check]`.

- If `CheckData` is `null`, there is no checksum check. The Demo text protocol `^&100$$` does not set `CheckData`.
- `EndOfLine` is searched after `BeginOfLine`.
- A complete frame with a bad checksum is not a valid packet. That frame is removed from the buffer. A later valid frame can still match.
- `CheckLength` defaults to 2, meaning two hex ASCII digits after `EndOfLine`. `CheckLength = 1` compares the next raw byte.

See `verify/TextProtocolAnalyzerCheckData`.

### `HexProtocolAnalyzer`

Frame format: `[Mask][length][data][checksum]`.

- `CheckLength` defaults to 1.
- `CheckData` defaults to `XorCheck`.
- For `CheckLength = 2`, set `CheckData16` (`Crc16Modbus` or `SumCheck16`). The compare is little-endian (low byte first) by default. For big-endian, set `CheckBigEndian = true`.
- If `CheckData16` is not set, the 1-byte `CheckData` result is compared as a 16-bit value.
- `CheckLength <= 0` means the frame has no checksum field.
- For a fixed-length frame, set `StaticLength`.

See `verify/HexProtocolAnalyzerCheckLength` and `Demo/DemoHexProtocol.cs`.

### Custom protocols

If the built-in analyzers do not apply, subclass `ProtocolAnalyzer<T>`.

- Override `SearchBuffer`: copy one frame from the `List<byte>` into `Raw`, and remove the processed bytes from the buffer.
- Override `Analyze()`: map `Raw` to the model.

`HexProtocolAnalyzer` and `TextProtocolAnalyzer` already implement `SearchBuffer`. In that case, configure the subclass in the constructor and override `Analyze()`.

Put more than one analyzer in an `IAnalyzerCollection`. Example: `Demo/MyProtocols.cs`.

## Examples

Binary frame:

```
header + length + payload + checksum
AA 44 05 01 02 03 04 05 EA
```

Text frame:

```
$GPGGA,121252.000,3937.3032,N,11611.6046,E,1,05,2.0,45.9,M,-5.7,M,,0000*75
```

`$` is the start marker. `*` is the end marker. `75` is the XOR of the characters between `$` and `*`.

NMEA (`$...*HH`):

```csharp
public class NmeaProtocol : TextProtocolAnalyzer<MyModel>
{
    public NmeaProtocol()
    {
        BeginOfLine = "$";
        EndOfLine = "*";
        CheckData = XorCheck; // CheckLength defaults to 2
    }

    public override void Analyze() { /* Raw → Data; Valid = true; */ }
}
```

Hex with a 1-byte sum:

```csharp
public class DemoHex : HexProtocolAnalyzer<DemoDataModel>
{
    public DemoHex()
    {
        Mask = new byte[] { 0xAA, 0xBB, 0xCC };
        CheckData = SumCheck; // CheckLength defaults to 1
    }

    public override void Analyze() { /* Raw → Data; Valid = true; */ }
}
```

Hex with a 2-byte checksum (set this in the subclass constructor):

```csharp
CheckLength = 2;
CheckData16 = Crc16Modbus; // or SumCheck16
// Little-endian by default. Big-endian: CheckBigEndian = true;
```

## Demo

![Demo screenshot](docs/Screen01.png)

The Demo is a WinForms app. It parses the text protocol `^&…$$` and the hex protocol `AA BB CC …` and lists `OnDataAnalyzed` results. Source: [`Demo/`](Demo/).

## Additional resources

- [CHANGELOG.md](CHANGELOG.md)
- [Windows virtual serial ports and debugging](https://www.petershi.net/archives/2885)
- [Original reference article](http://blog.csdn.net/wuyazhe/article/details/5598945)
- [MIT license](LICENSE)
