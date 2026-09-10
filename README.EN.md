[中文](README.md)

# KoboldCom

KoboldCom is a C# serial port library. It opens a port, reads and writes bytes, splits frames with one or more protocols, and maps a complete frame to a data model. One port can run more than one protocol.

In this documentation, “async” means receive and parse work driven by the `SerialPort.DataReceived` event. It does not mean C# `async` / `await`.

## Requirements

The current version requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).

| Version | Target | How to get it |
| --- | --- | --- |
| **1.x** | .NET Framework 3.5 | Tag [`v1.1.0`](https://github.com/PeterRock/KoboldCom/releases/tag/v1.1.0). Later Framework commits are on branch [`netfx-1.x`](https://github.com/PeterRock/KoboldCom/tree/netfx-1.x). Do not upgrade a Framework project to 2.x. |
| **2.x** (current) | `net8.0` + NuGet `System.IO.Ports` | Current `master`. Assembly version **2.0.0**. A .NET Framework 3.5 project cannot reference 2.x. |

See [CHANGELOG.md](CHANGELOG.md) for the list of changes.

## Components

- `Communicator`: connects the port and the analyzers. Reads bytes and dispatches them.
- `SerialPort` (implements `ICommunication`): opens, closes, reads, and writes the port.
- `ProtocolAnalyzer<T>`: maps one frame to model `T`. Built-in types: `HexProtocolAnalyzer` and `TextProtocolAnalyzer`.
- `IAnalyzerCollection`: a set of analyzers. One `Communicator` can register more than one protocol.

Processing order:

1. `SerialPort.DataReceived` fires.
2. `Communicator` reads the bytes and raises `OnRawDataReceived`.
3. Each analyzer in the collection calls `SearchBuffer`.
4. After a complete frame is found, `Analyze()` runs.

## Usage

Create an `IAnalyzerCollection` (see `Demo/MyProtocols.cs`). Pass it and a `SerialPort` to `Communicator`. Subscribe to events. Open the port.

```csharp
var protocols = new MyProtocols();
var communicator = new Communicator(new KoboldCom.SerialPort(), protocols);

communicator.OnRawDataReceived += bytes => { /* raw bytes */ };
protocols.ProtocolText.OnDataAnalyzed += m => { /* text model */ };
protocols.ProtocolBinary.OnDataAnalyzed += m => { /* hex model */ };

communicator.Com.Open(new SerialPortSetting { Port = 2, Baudrate = 9600 });
```

Set the frame format in the protocol subclass constructor. In `Analyze()`, assign `Raw` to `Data`, then set `Valid = true`.

| Event | Type | When it runs |
| --- | --- | --- |
| `OnRawDataReceived` | `Communicator` | A chunk of raw bytes was read. |
| `OnDataAnalyzed` | `ProtocolAnalyzer<T>` | The subclass sets `Valid` to `true`. After a timeout, `Valid` is set back to `false` and the event runs again. |

`ICommunication.OnDataReceived` means the port has bytes to read. `Communicator` already subscribes to that event.

A port name is `COM` plus the port number (`SerialPortSetting.Port`). When `Handshake` is `None`, `RtsEnable` and `DtrEnable` default to `true` and are written when `Setting` is applied and after `Open`.

### Text protocol

`TextProtocolAnalyzer` frame format: `[BeginOfLine][payload][EndOfLine]`. `EndOfLine` is searched after `BeginOfLine`.

```csharp
public class DemoText : TextProtocolAnalyzer<int>
{
    public DemoText()
    {
        BeginOfLine = "^&";
        EndOfLine = "$$";
    }

    public override void Analyze() { /* parse Raw; Valid = true; */ }
}
```

Optional checksum: if you set `CheckData`, a check field follows the frame. `CheckLength` defaults to 2 (two hex ASCII digits after `EndOfLine`). NMEA example: `BeginOfLine = "$"`, `EndOfLine = "*"`, `CheckData = XorCheck`. A complete frame with a bad checksum is removed from the buffer.

### Hex protocol

`HexProtocolAnalyzer` frame format: `[Mask][length][data][checksum]`. For a fixed-length frame, set `StaticLength`.

```
AA 44 05 01 02 03 04 05 EA
```

```csharp
public class DemoHex : HexProtocolAnalyzer<DemoDataModel>
{
    public DemoHex()
    {
        Mask = new byte[] { 0xAA, 0xBB, 0xCC };
        CheckData = SumCheck;
    }

    public override void Analyze() { /* Raw → Data; Valid = true; */ }
}
```

Optional checksum: `CheckLength` defaults to 1. `CheckData` defaults to `XorCheck`. For a 2-byte check, set `CheckLength = 2` and `CheckData16` (`Crc16Modbus` or `SumCheck16`). The compare is little-endian by default. For big-endian, set `CheckBigEndian = true`.

## Custom protocols

If the built-in analyzers do not apply, subclass `ProtocolAnalyzer<T>`.

- Override `SearchBuffer`: copy one frame from the `List<byte>` into `Raw`, and remove the processed bytes from the buffer.
- Override `Analyze()`: map `Raw` to the model.

`HexProtocolAnalyzer` and `TextProtocolAnalyzer` already implement `SearchBuffer`. In that case, set the frame format and override `Analyze()`.

## Demo

![Demo screenshot](docs/Screen01.png)

The Demo is a `net8.0-windows` WinForms app. It parses the text protocol `^&…$$` and the hex protocol `AA BB CC …` and lists `OnDataAnalyzed` results. Source: [`Demo/`](Demo/). Run it on Windows:

```bash
dotnet run --project Demo
```

Other systems can cross-compile the Demo (`EnableWindowsTargeting` is set) but cannot run the UI.

## Build

```bash
dotnet build KoboldCom.sln
```

Checks that need no hardware: `dotnet run --project verify/TextProtocolAnalyzerCheckData`, `verify/HexProtocolAnalyzerCheckLength`, `verify/SerialPortHandshakeNone`.

## Additional resources

- [CHANGELOG.md](CHANGELOG.md)
- [Windows virtual serial ports and debugging](https://www.petershi.net/archives/2885)
- [Original reference article](http://blog.csdn.net/wuyazhe/article/details/5598945)
- [MIT license](LICENSE)
