[English](README.EN.md)

# KoboldCom

KoboldCom 是一个 C# 串口通信类库。它从串口读取字节，用一个或多个协议解析器切分数据帧，并把完整帧映射到数据模型。

库包含 `HexProtocolAnalyzer`（二进制帧）和 `TextProtocolAnalyzer`（带起止标志的文本帧）。

文档里的「异步」指 `SerialPort.DataReceived` 事件驱动的接收和解析。它不是 C# 的 `async` / `await`。

## 要求

当前版本需要 [.NET 8 SDK](https://dotnet.microsoft.com/download)。2.0.0 更改了目标框架。

| 版本 | 目标框架 | 获取方式 |
| --- | --- | --- |
| **1.x** | .NET Framework 3.5 | 标签 [`v1.1.0`](https://github.com/PeterRock/KoboldCom/releases/tag/v1.1.0)。若还需要该标签之后的十六进制双字节校验，以及 `Handshake.None` 下的 RTS/DTR 行为，使用分支 [`netfx-1.x`](https://github.com/PeterRock/KoboldCom/tree/netfx-1.x)。不要把 Framework 项目升级到 2.x。 |
| **2.x**（当前） | `net8.0` + NuGet `System.IO.Ports` | 当前 `master`。程序集版本 **2.0.0**。包含上表 1.x 的协议和串口行为。.NET Framework 3.5 项目不能引用 2.x。 |

2.0.0 的公开 C# API 和中文 XML 文档与 1.x 相同。协议逻辑没有改写。变更记录见 [CHANGELOG.md](CHANGELOG.md)。

串口名格式为 `COM` 加端口号（`SerialPortSetting.Port`）。Demo 的目标框架是 `net8.0-windows`。Demo 只能在 Windows 上运行。

## 构建

```bash
dotnet build KoboldCom.sln
```

不接硬件时可以运行这些检查：

```bash
dotnet run --project verify/TextProtocolAnalyzerCheckData
dotnet run --project verify/HexProtocolAnalyzerCheckLength
dotnet run --project verify/SerialPortHandshakeNone
```

这些项目分别检查文本校验、十六进制校验长度，以及 `Handshake.None` 时的 RTS/DTR 默认值。

在 Windows 上运行 Demo：

```bash
dotnet run --project Demo
```

其他系统可以交叉编译 Demo（已设置 `EnableWindowsTargeting`），但不能运行该界面。示例代码在 [`Demo/`](Demo/)。

## 工作方式

`Communicator` 持有一个 `ICommunication`（通常是 `KoboldCom.SerialPort`）和一个 `IAnalyzerCollection`。

处理顺序如下：

1. `SerialPort.DataReceived` 触发。
2. `Communicator` 读取字节。
3. `Communicator` 引发 `OnRawDataReceived`。
4. 集合中的每个 `IAnalyzer` 调用 `SearchBuffer`。
5. 找到完整帧后调用 `Analyze()`。

一个端口可以注册多个解析器。

## 用法

### 事件

| 事件 | 类型 | 时机 |
| --- | --- | --- |
| `OnRawDataReceived` | `Communicator` | 读到一批原始字节。 |
| `OnDataAnalyzed` | `ProtocolAnalyzer<T>` | 子类将 `Valid` 设为 `true`。超时后 `Valid` 被设回 `false`，该事件也会触发。 |

`ICommunication.OnDataReceived` 表示端口上有可读数据。`Communicator` 已经订阅该事件。

```csharp
var protocols = new MyProtocols();
var communicator = new Communicator(new KoboldCom.SerialPort(), protocols);

communicator.OnRawDataReceived += bytes => { /* 原始字节 */ };
protocols.ProtocolText.OnDataAnalyzed += m => { /* 文本模型 */ };
protocols.ProtocolBinary.OnDataAnalyzed += m => { /* 十六进制模型 */ };

communicator.Com.Open(new SerialPortSetting { Port = 2, Baudrate = 9600 });
```

在子类构造函数中设置帧格式。在 `Analyze()` 中把 `Raw` 赋给 `Data`，然后设置 `Valid = true`。

### 串口设置

`SerialPortSetting` 的默认值：

- `Handshake = None`
- `RtsEnable = true`
- `DtrEnable = true`

应用 `Setting` 时写入 RTS/DTR。`Open` 之后再写一次。`Handshake` 不是 `None` 时，库不写这两根线；读取设置时这两项为 `false`。若某根线需要保持低电平，把对应属性设为 `false`。

### `TextProtocolAnalyzer`

帧格式：`[BeginOfLine][数据][EndOfLine]`。如果设置了 `CheckData`，帧末尾还有校验：`[EndOfLine][校验]`。

- `CheckData` 为 `null` 时不做校验。Demo 文本协议 `^&100$$` 不设置 `CheckData`。
- 在 `BeginOfLine` 之后查找 `EndOfLine`。
- 完整但校验失败的帧不是有效包。该帧从缓冲区删除。之后的合法帧仍可匹配。
- `CheckLength` 默认值为 2，表示 `EndOfLine` 后的两位十六进制 ASCII。`CheckLength = 1` 时比较后一个二进制字节。

见 `verify/TextProtocolAnalyzerCheckData`。

### `HexProtocolAnalyzer`

帧格式：`[Mask][长度][数据][校验]`。

- `CheckLength` 默认值为 1。
- `CheckData` 默认值为 `XorCheck`。
- `CheckLength = 2` 时使用 `CheckData16`（`Crc16Modbus` 或 `SumCheck16`）。比较默认小端（低字节在前）。大端时设置 `CheckBigEndian = true`。
- 未设置 `CheckData16` 时，把单字节 `CheckData` 结果当作 16 位值比较。
- `CheckLength <= 0` 表示没有校验段。
- 定长帧使用 `StaticLength`。

见 `verify/HexProtocolAnalyzerCheckLength` 和 `Demo/DemoHexProtocol.cs`。

### 自定义协议

若内置解析器不适用，继承 `ProtocolAnalyzer<T>`。

- 重写 `SearchBuffer`：从 `List<byte>` 取出一帧写入 `Raw`，并从缓冲区删除已处理的字节。
- 重写 `Analyze()`：把 `Raw` 映射到模型。

`HexProtocolAnalyzer` 和 `TextProtocolAnalyzer` 已经实现 `SearchBuffer`。这种情况下只需在构造函数中配置，并重写 `Analyze()`。

把多个解析器放在一个 `IAnalyzerCollection` 中。示例：`Demo/MyProtocols.cs`。

## 示例

二进制帧：

```
头 + 长度 + 数据 + 校验
AA 44 05 01 02 03 04 05 EA
```

文本帧：

```
$GPGGA,121252.000,3937.3032,N,11611.6046,E,1,05,2.0,45.9,M,-5.7,M,,0000*75
```

`$` 是起始标志。`*` 是结束标志。`75` 是 `$` 与 `*` 之间字符的异或。

NMEA（`$...*HH`）：

```csharp
public class NmeaProtocol : TextProtocolAnalyzer<MyModel>
{
    public NmeaProtocol()
    {
        BeginOfLine = "$";
        EndOfLine = "*";
        CheckData = XorCheck; // CheckLength 默认值为 2
    }

    public override void Analyze() { /* Raw → Data; Valid = true; */ }
}
```

十六进制，单字节和校验：

```csharp
public class DemoHex : HexProtocolAnalyzer<DemoDataModel>
{
    public DemoHex()
    {
        Mask = new byte[] { 0xAA, 0xBB, 0xCC };
        CheckData = SumCheck; // CheckLength 默认值为 1
    }

    public override void Analyze() { /* Raw → Data; Valid = true; */ }
}
```

十六进制，双字节校验（写在子类构造函数中）：

```csharp
CheckLength = 2;
CheckData16 = Crc16Modbus; // 或 SumCheck16
// 默认小端。大端：CheckBigEndian = true;
```

## Demo

![运行截图](docs/Screen01.png)

Demo 是 WinForms 程序。它同时解析文本协议 `^&…$$` 和十六进制协议 `AA BB CC …`，并在列表中显示 `OnDataAnalyzed` 的结果。源码在 [`Demo/`](Demo/)。

## 相关链接

- [CHANGELOG.md](CHANGELOG.md)
- [Windows 虚拟串口与调试](https://www.petershi.net/archives/2885)
- [最初参考的文章](http://blog.csdn.net/wuyazhe/article/details/5598945)
- [MIT 许可证](LICENSE)
