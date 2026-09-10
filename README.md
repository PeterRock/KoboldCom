[English](README.EN.md)

# KoboldCom

KoboldCom 是一个 C# 串口通信类库。它打开串口、读写字节、按协议切分数据帧，并把完整帧映射到数据模型。一个端口可以同时使用多个协议。

文档里的「异步」指 `SerialPort.DataReceived` 事件驱动的接收和解析。它不是 C# 的 `async` / `await`。

## 要求

当前版本需要 [.NET 8 SDK](https://dotnet.microsoft.com/download)。

| 版本 | 目标框架 | 获取方式 |
| --- | --- | --- |
| **1.x** | .NET Framework 3.5 | 标签 [`v1.1.0`](https://github.com/PeterRock/KoboldCom/releases/tag/v1.1.0)。之后的 Framework 提交在分支 [`netfx-1.x`](https://github.com/PeterRock/KoboldCom/tree/netfx-1.x)。不要把 Framework 项目升级到 2.x。 |
| **2.x**（当前） | `net8.0` + NuGet `System.IO.Ports` | 当前 `master`。程序集版本 **2.0.0**。.NET Framework 3.5 项目不能引用 2.x。 |

变更记录见 [CHANGELOG.md](CHANGELOG.md)。

## 组成

- `Communicator`：连接串口和解析器。读取字节，并分发给解析器。
- `SerialPort`（实现 `ICommunication`）：打开、关闭、读写串口。
- `ProtocolAnalyzer<T>`：把一帧映射到模型 `T`。内置 `HexProtocolAnalyzer` 和 `TextProtocolAnalyzer`。
- `IAnalyzerCollection`：一组解析器。一个 `Communicator` 可以注册多个协议。

处理顺序：

1. `SerialPort.DataReceived` 触发。
2. `Communicator` 读取字节，并引发 `OnRawDataReceived`。
3. 集合中的每个解析器调用 `SearchBuffer`。
4. 找到完整帧后调用 `Analyze()`。

## 用法

创建 `IAnalyzerCollection`（见 `Demo/MyProtocols.cs`）。把它和 `SerialPort` 交给 `Communicator`。订阅事件。打开端口。

```csharp
var protocols = new MyProtocols();
var communicator = new Communicator(new KoboldCom.SerialPort(), protocols);

communicator.OnRawDataReceived += bytes => { /* 原始字节 */ };
protocols.ProtocolText.OnDataAnalyzed += m => { /* 文本模型 */ };
protocols.ProtocolBinary.OnDataAnalyzed += m => { /* 十六进制模型 */ };

communicator.Com.Open(new SerialPortSetting { Port = 2, Baudrate = 9600 });
```

在协议子类的构造函数中设置帧格式。在 `Analyze()` 中把 `Raw` 赋给 `Data`，然后设置 `Valid = true`。

| 事件 | 类型 | 时机 |
| --- | --- | --- |
| `OnRawDataReceived` | `Communicator` | 读到一批原始字节。 |
| `OnDataAnalyzed` | `ProtocolAnalyzer<T>` | 子类将 `Valid` 设为 `true`。超时后 `Valid` 被设回 `false`，该事件也会触发。 |

`ICommunication.OnDataReceived` 表示端口上有可读数据。`Communicator` 已经订阅该事件。

串口名格式为 `COM` 加端口号（`SerialPortSetting.Port`）。`Handshake` 为 `None` 时，`RtsEnable` 和 `DtrEnable` 默认为 `true`，在应用 `Setting` 和 `Open` 之后写入。

### 文本协议

`TextProtocolAnalyzer` 的帧格式是 `[BeginOfLine][数据][EndOfLine]`。在 `BeginOfLine` 之后查找 `EndOfLine`。

```csharp
public class DemoText : TextProtocolAnalyzer<int>
{
    public DemoText()
    {
        BeginOfLine = "^&";
        EndOfLine = "$$";
    }

    public override void Analyze() { /* 从 Raw 解析数据；Valid = true; */ }
}
```

可选校验：设置 `CheckData` 后，帧末尾还有校验字段。`CheckLength` 默认值为 2（`EndOfLine` 后的两位十六进制 ASCII）。NMEA 示例：`BeginOfLine = "$"`，`EndOfLine = "*"`，`CheckData = XorCheck`。校验失败的完整帧会从缓冲区删除。

### 十六进制协议

`HexProtocolAnalyzer` 的帧格式是 `[Mask][长度][数据][校验]`。定长帧使用 `StaticLength`。

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

可选校验：`CheckLength` 默认值为 1，`CheckData` 默认值为 `XorCheck`。双字节校验时设置 `CheckLength = 2` 和 `CheckData16`（`Crc16Modbus` 或 `SumCheck16`）。比较默认小端。大端时设置 `CheckBigEndian = true`。

## 自定义协议

若内置解析器不适用，继承 `ProtocolAnalyzer<T>`。

- 重写 `SearchBuffer`：从 `List<byte>` 取出一帧写入 `Raw`，并删除已处理的字节。
- 重写 `Analyze()`：把 `Raw` 映射到模型。

`HexProtocolAnalyzer` 和 `TextProtocolAnalyzer` 已经实现 `SearchBuffer`。这种情况下只需配置帧格式并重写 `Analyze()`。

## Demo

![运行截图](docs/Screen01.png)

Demo 是 `net8.0-windows` WinForms 程序。它同时解析文本协议 `^&…$$` 和十六进制协议 `AA BB CC …`，并在列表中显示 `OnDataAnalyzed` 的结果。源码在 [`Demo/`](Demo/)。只能在 Windows 上运行：

```bash
dotnet run --project Demo
```

其他系统可以交叉编译 Demo（已设置 `EnableWindowsTargeting`），但不能运行该界面。

## 构建

```bash
dotnet build KoboldCom.sln
```

无硬件检查：`dotnet run --project verify/TextProtocolAnalyzerCheckData`、`verify/HexProtocolAnalyzerCheckLength`、`verify/SerialPortHandshakeNone`。

## 相关链接

- [CHANGELOG.md](CHANGELOG.md)
- [Windows 虚拟串口与调试](https://www.petershi.net/archives/2885)
- [最初参考的文章](http://blog.csdn.net/wuyazhe/article/details/5598945)
- [MIT 许可证](LICENSE)
