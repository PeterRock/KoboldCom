[English](README.EN.md)

# KoboldCom

KoboldCom 是一个 C# 串口通信类库：把收包、多协议解析和数据模型映射收成一条流水线，用来快速实现常见十六进制 / 文本协议。

这里的「异步」指 `SerialPort.DataReceived` 事件驱动的收包与解析，**不是** C# 的 `async` / `await`。

## 功能

- 封装串口打开、读写与原始收包（`Communicator` + `SerialPort`）
- 内置 `HexProtocolAnalyzer`（二进制帧）和 `TextProtocolAnalyzer`（起止符文本帧）
- 同一端口上同时匹配多种协议，解析后映射到自己的数据模型
- 可选校验：文本 `CheckData`（如 NMEA XOR）；十六进制 1 字节或 2 字节（`CheckData` / `CheckData16`）
- `Handshake.None` 时默认拉高 RTS/DTR，避免部分无握手设备只收不回

## 要求与版本

当前主线需要 [.NET 8 SDK](https://dotnet.microsoft.com/download)。这是一次有意的主版本中断。

| 版本线 | 目标框架 | 从哪里取 |
| --- | --- | --- |
| **1.x** | .NET Framework 3.5 | 标签 [`v1.1.0`](https://github.com/PeterRock/KoboldCom/releases/tag/v1.1.0)。若还需要其后的十六进制双字节校验与 Handshake.None RTS/DTR，请用分支 [`netfx-1.x`](https://github.com/PeterRock/KoboldCom/tree/netfx-1.x)。不要把 Framework 项目升到 2.x。 |
| **2.x**（当前） | `net8.0` + NuGet `System.IO.Ports` | 当前 `master`，程序集版本 **2.0.0**。包含上述 1.x 协议 / 串口行为。Framework 3.5 项目无法引用 2.x。 |

公开 C# API 与中文 XML 文档尽量保持不变。更细的变更见 [CHANGELOG.md](CHANGELOG.md)。

串口名仍按 Windows 习惯写成 `COM` + 端口号（`SerialPortSetting.Port`）。Demo 是 `net8.0-windows` WinForms，只能在 Windows 上运行。

## 快速开始

```bash
dotnet build KoboldCom.sln

dotnet run --project verify/TextProtocolAnalyzerCheckData
dotnet run --project verify/HexProtocolAnalyzerCheckLength
dotnet run --project verify/SerialPortHandshakeNone
```

三个 `verify` 项目不需要真实硬件，用来核对文本校验、十六进制校验长度，以及 Handshake.None 下的 RTS/DTR 默认值。

在 Windows 上打开 Demo：

```bash
dotnet run --project Demo
```

非 Windows 环境可以交叉编译 Demo（工程已开启 `EnableWindowsTargeting`），但不能运行该界面。完整多协议用法见 [`Demo/`](Demo/)。

## 概念

流水线是：

`SerialPort.DataReceived` → `Communicator` 读入字节 → 各 `IAnalyzer.SearchBuffer` 切帧 → `Analyze()` 映射模型

`Communicator` 持有一个 `ICommunication`（通常是 `KoboldCom.SerialPort`）和一个 `IAnalyzerCollection`。收到字节后先触发 `OnRawDataReceived`，再让集合里的每个解析器搜索缓冲区。

上层通常订阅这两个事件：

| 事件 | 在哪里 | 何时触发 |
| --- | --- | --- |
| `OnRawDataReceived` | `Communicator` | 串口刚读到一批原始字节 |
| `OnDataAnalyzed` | 具体的 `ProtocolAnalyzer<T>` | 子类把 `Valid = true`（或之后超时把 `Valid` 置回 `false`） |

`ICommunication.OnDataReceived` 是更底层的「有数据可读」钩子，`Communicator` 已经接好，一般不必再订。

```csharp
var protocols = new MyProtocols();
var communicator = new Communicator(new KoboldCom.SerialPort(), protocols);

communicator.OnRawDataReceived += bytes => { /* 原始字节 */ };
protocols.ProtocolText.OnDataAnalyzed += m => { /* 文本模型 */ };
protocols.ProtocolBinary.OnDataAnalyzed += m => { /* 十六进制模型 */ };

communicator.Com.Open(new SerialPortSetting { Port = 2, Baudrate = 9600 });
```

`SerialPortSetting` 默认 `Handshake = None`、`RtsEnable = true`、`DtrEnable = true`，在应用 `Setting` 以及 `Open` 之后写入端口。`Handshake` 不是 `None` 时这两根线仍由系统握手逻辑控制，不会被改写；读取时也显示为未拉高。需要某根线保持低电平，把对应标志设为 `false`。

## 协议

常见帧长这样：

```
头 + 长度 + 正文 + 校验
AA 44 05 01 02 03 04 05 EA
```

或文本句：

```
$GPGGA,121252.000,3937.3032,N,11611.6046,E,1,05,2.0,45.9,M,-5.7,M,,0000*75
```

`$` 开始，`*` 结束，`75` 是 `$` 与 `*` 之间字符的异或。这两种形态分别对应 `HexProtocolAnalyzer` 和 `TextProtocolAnalyzer`。在子类**构造函数**里配置帧格式，在 `Analyze()` 里把 `Raw` 映射到 `Data`，再设 `Valid = true`。

### 文本：`TextProtocolAnalyzer`

帧形：`[BeginOfLine][正文][EndOfLine]`，可选再跟校验：`[EndOfLine][校验]`。

- 不设 `CheckData` 则不做校验（Demo 的 `^&100$$` 就是这样）
- `EndOfLine` 从 `BeginOfLine` **之后**查找，避免缓冲区里靠前的结束符被误用
- 完整但校验失败的帧不会当作有效包，并从缓冲区丢掉；后面若还有合法帧会继续匹配

NMEA 风格（`$...*HH`）：

```csharp
public class NmeaProtocol : TextProtocolAnalyzer<MyModel>
{
    public NmeaProtocol()
    {
        BeginOfLine = "$";
        EndOfLine = "*";
        CheckData = XorCheck; // CheckLength 默认 2：* 后两位十六进制 ASCII
    }

    public override void Analyze() { /* Raw → Data; Valid = true; */ }
}
```

`CheckLength = 1` 时按 `EndOfLine` 后的单字节二进制比较。细节与边界情况见 `verify/TextProtocolAnalyzerCheckData`。

### 十六进制：`HexProtocolAnalyzer`

帧形：`[Mask][长度][数据][校验]`。默认 `CheckLength = 1`、`CheckData = XorCheck`，现有单字节 XOR/SUM 协议不用改。

```csharp
public class DemoHex : HexProtocolAnalyzer<DemoDataModel>
{
    public DemoHex()
    {
        Mask = new byte[] { 0xAA, 0xBB, 0xCC };
        CheckData = SumCheck; // 默认仍是帧尾 1 字节
    }

    public override void Analyze() { /* Raw → Data; Valid = true; */ }
}
```

双字节校验时，在子类构造函数中设置：

```csharp
CheckLength = 2;
CheckData16 = Crc16Modbus; // 或 SumCheck16；返回 16 位主机数值
// 默认小端（低字节在前），与 Modbus CRC-16 一致
// 高字节在前：CheckBigEndian = true;
```

未设置 `CheckData16` 时，会把原来的单字节 `CheckData` 结果当作 16 位值比较。`CheckLength <= 0` 表示帧里没有校验段。定长帧用 `StaticLength`。细节见 `verify/HexProtocolAnalyzerCheckLength` 和 `Demo/DemoHexProtocol.cs`。

## 扩展

内置两种解析器不够时，继承抽象类 `ProtocolAnalyzer<T>`：

- 一般只需要重写 `SearchBuffer`：从 `List<byte>` 里切出一帧写到 `Raw`，并视情况从缓冲区删掉已消费字节
- 再实现 `Analyze()` 做模型转换

`HexProtocolAnalyzer` / `TextProtocolAnalyzer` 已经实现了 `SearchBuffer`，多数项目只需配帧头、校验并重写 `Analyze()`。多个解析器放进自己的 `IAnalyzerCollection`（见 `Demo/MyProtocols.cs`）即可同时工作。

## Demo

![运行截图](docs/Screen01.png)

WinForms 示例同时跑文本协议 `^&…$$` 和十六进制 `AA BB CC …`，并在列表里显示 `OnDataAnalyzed` 结果。源码在 [`Demo/`](Demo/)。

## 串口模拟与调试

[Windows 虚拟串口与调试](https://www.petershi.net/archives/2885)

## 致谢

动手做这个库的灵感来自[这篇文章](http://blog.csdn.net/wuyazhe/article/details/5598945)。

## 许可证

[MIT](LICENSE)
