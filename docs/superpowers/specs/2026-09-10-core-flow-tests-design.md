# 核心库契约补充测试设计

- 日期：2026-09-10
- 状态：Draft（设计已批准，实现延后）

本文只定义测试范围与期望。本 PR 不改生产代码，不改现有 `verify/`，不改 Demo。

## 1. 背景

KoboldCom 是开源 C# 串口通信组件库。当前主线是 `master`，目标框架 `net8.0`，程序集版本 **2.0.0**。

库打开端口、读写字节、按协议切分数据帧，并把完整帧映射到数据模型。一个端口可以同时注册多个协议。文档里的「异步」指 `ICommunication.OnDataReceived` / `SerialPort.DataReceived` 事件驱动的接收和解析，不是 C# `async` / `await`。

`verify/` 已覆盖内置解析器的校验算法，以及 `Handshake.None` 下的 RTS/DTR 设置。这些检查直接调用 `SearchBuffer` 或读写 `SerialPort.Setting`，不经过 `Communicator`，也不验证可替换端口、事件管线、粘包/半包、`Valid` 生命周期或写路径。

本设计的目标是：**为核心库契约补充自动化用例，不含 Demo。**

## 2. 目标与非目标

### 目标

- 用可替换的 `ICommunication` 在无硬件环境下驱动 `Communicator`。
- 覆盖下列契约：端口替换、事件管线、同一端口多协议、跨接收粘包/半包、写路径与 `Open`/`Close`、`Valid` 超时、自定义 `ProtocolAnalyzer` 扩展面。
- 用例以英文 ID（A1、B2、…）编号，便于实现 PR 对照。
- 记录当前行为。本设计不要求改库。

### 非目标

- 不重复 `verify/` 已覆盖的校验算法与 RTS/DTR 设置检查（见第 3 节）。
- 不测 Demo WinForms。
- 不测真实 COM 枚举/打开的操作系统差异。
- 不覆盖 `netfx-1.x` / .NET Framework 1.x 分支。
- 本文件不实现测试，也不指定必须使用 xUnit 还是 `verify/` 风格控制台检查。

## 3. 已有覆盖（不要重复）

下列检查已经存在。补充用例可以**经过 `Communicator` 使用**无校验或默认单字节校验的帧，但不要再断言校验算法本身或 Handshake 控制线。

| 位置 | 已覆盖内容 | 补充用例不要再测 |
| --- | --- | --- |
| `verify/TextProtocolAnalyzerCheckData` | `TextProtocolAnalyzer.SearchBuffer` + `CheckData`：无校验 Demo 帧 `^&…$$`、NMEA XOR、无效/畸形校验丢弃、不完整校验等待、`EndOfLine` 出现在 `BeginOfLine` 之前 | NMEA 句子、XOR/SUM 计算结果、坏校验丢弃规则 |
| `verify/HexProtocolAnalyzerCheckLength` | `HexProtocolAnalyzer.SearchBuffer`：默认 1 字节 XOR、Demo 风格 SUM、`CheckLength=2` 小端/大端、`Crc16Modbus`、定长帧、`CheckLength=0`、不完整等待 | `CheckData16`、CRC、字节序、`CheckLength` 默认值 |
| `verify/SerialPortHandshakeNone` | `SerialPortSetting` / `Setting`：`Handshake.None` 时 RTS/DTR 默认与写入；非 None 时不抛、读出为未激活 | `RtsEnable` / `DtrEnable`、握手模式 |

`verify/` 直接构造解析器并调用 `SearchBuffer`，或只改 `SerialPort.Setting`。它们不构造 `Communicator`，不订阅 `OnRawDataReceived` / `OnDataAnalyzed`，不调用 `Analyze()` 置 `Valid`。

## 4. 可测缝：FakeCommunication

实现 PR 提供测试用 `FakeCommunication : ICommunication`。CI 不打开真实串口。

约定：

- `Push(byte[] data)` 把字节放入内部读缓冲，更新 `BytesToRead`，然后引发 `OnDataReceived`。`Communicator` 已在构造时订阅该事件。
- `Read(buffer, offset, count)` 从该缓冲复制并减少 `BytesToRead`。行为需足以让 `Communicator.ComDataReceived` 读到本次 `Push` 的全部字节。
- `Write(byte[] buffer, int offset, int count)`（以及实现接口所需的字符串 `Write` / `WriteLine`）把写出的字节追加到可断言的写记录。
- `Open(ICommunicationSetting)` / `Close()` 只改 `IsOpen` 等内存状态。不访问系统串口。
- 其余 `ICommunication` 成员给安全默认值（例如 `Encoding.ASCII`、空 `NewLine`、可空 `Setting`），使接口可编译。本目录用例不依赖真实波特率或端口名。

`Communicator` 接收路径只用 `OnDataReceived`、`BytesToRead`、`Read`。写路径只用 `Com.Write`。A–E 不要求 Fake 模拟 `System.IO.Ports.SerialPort.DataReceived` 线程模型。

## 5. 被测产品契约

下列条目是当前 `master` 上的公开行为。实现 PR 按现状断言，不在本设计中改库。

1. **可替换端口。** `Communicator` 构造接受任意 `ICommunication`。`Com` 可读写。构造参数为 `null` 时抛 `NullReferenceException`，消息为 `no reference communication module was initialized`（C1）。
2. **事件管线。** 端口引发 `OnDataReceived` 后，`Communicator` 读取字节并追加到内部 `_dataList`，然后：
   1. 若已订阅，对本次读到的 `buffer` 引发 `OnRawDataReceived`；
   2. 对 `Analyzers` 中每个 `IAnalyzer` 调用 `SearchBuffer(_dataList)`；
   3. 若 `analyzer.Raw.Length > 0`，调用 `Analyze()`，再把 `Raw` 置为空数组；
   4. 缓冲区仍有剩余且本轮有解析器抽出帧时继续循环。
   `Communicator` 不使用 `SearchResult` 返回值，只看 `Raw.Length`。
3. **同一端口多协议。** 一个 `Communicator` 持有一个 `IAnalyzerCollection`。完整且互不重叠的文本帧与十六进制帧应交由对应解析器处理，不应被另一个解析器收走（A3）。
4. **粘包 / 半包。** 内部缓冲跨多次 `Push` 累积。`SearchBuffer` 只在完整帧时写入 `Raw`；`Analyze` 只在 `Raw.Length > 0` 时运行。一包多帧应连续抽出。噪声前缀由各协议的 `SearchBuffer` 处理。
5. **写路径与开关。** 应用通过 `communicator.Com` 调用 `Write`、`Open`、`Close`。`Communicator` 不包装这些方法。Fake 记录写出的字节，并维护 `IsOpen`。
6. **`Valid` 与 `OnDataAnalyzed`。** `ProtocolAnalyzer<T>.Valid` 的 setter 在每次赋值时（`true` 或 `false`）通知 `OnDataAnalyzed`（若有订阅）。设为 `true` 时用当前 `TimeOut` 武装内部 `Timer`。`TimeOut` 的公开单位是秒（内部毫秒 = 秒 × 1000；默认 2 秒）。定时器到期且仍为 `Valid` 时，重置 `Data` 并把 `Valid` 设为 `false`，因此再次引发 `OnDataAnalyzed`（README）。
7. **订阅者异常。** `OnRawDataReceived` 的订阅者抛出 `InvalidOperationException` 时，该订阅者被移除，其余订阅者与后续 `SearchBuffer` / `Analyze` 继续（C3）。其他异常类型不在本目录。

默认 `ReadBufferSize` 为 `0x800`（2048）。每次接收时，若**追加前** `_dataList.Count > ReadBufferSize`，先 `Clear()` 再 `AddRange`。这是当前实现，C2 记录它，不在本设计中改为环形缓冲或其他策略。

## 6. 用例目录

实现 PR 必须自动化下表全部用例。ID 在全文唯一。期望列描述当前行为。

测试用解析器放在测试项目内：薄子类，在 `Analyze()` 里把 `Raw` 映射到 `Data` 并设 `Valid = true`（D3 除外）。不要引用 `Demo/` 类型。`IAnalyzerCollection` 用测试项目内的数组包装即可。

文本黄金帧与 Demo 文档一致、且无 `CheckData`：`BeginOfLine = "^&"`，`EndOfLine = "$$"`，例如 `^&100$$`。十六进制黄金帧使用简单 `Mask` + 长度字段；不要使用 `CheckData16`、`Crc16Modbus`、`CheckBigEndian`。推荐 `CheckLength = 0` 且 `CheckData = null`，避免碰到第 3 节的校验断言。若使用默认 1 字节 XOR，只为凑齐完整帧，不断言校验函数的数学结果。

### A 黄金路径

| ID | 场景 | 期望 |
| --- | --- | --- |
| A1 | Fake 端口 + 单一文本协议；`Push` 一帧完整 `^&…$$`；`Analyze` 设 `Valid = true` | 引发 `OnRawDataReceived`（本次字节）；引发 `OnDataAnalyzed` 且 `Valid == true`；`Data` 来自该帧 `Raw`（例如载荷中的整数） |
| A2 | 单一十六进制协议（无双字节/CRC 等复杂校验）；`Push` 一帧完整简单 hex | 与 A1 相同：`OnRawDataReceived` + `OnDataAnalyzed` 且 `Valid == true`；`Data` 来自 `Raw` |
| A3 | 同一 `Communicator` 注册文本与十六进制两个解析器；交错 `Push` 完整文本帧与完整 hex 帧 | 各解析器 `OnDataAnalyzed` 次数等于各自帧数；文本解析器不分析 hex 帧，hex 解析器不分析文本帧 |
| A4 | 调用 `communicator.Com.Write` 写入若干字节 | Fake 的写记录等于这些字节（含 offset/count 指定的片段） |

A4 可同时调用 Fake 的 `Open` / `Close` 并断言 `IsOpen`，以覆盖第 5 节第 5 条。写缓冲断言是本用例的硬条件。

### B 粘包 / 半包

| ID | 场景 | 期望 |
| --- | --- | --- |
| B1 | 把一帧完整文本（或与 A1 相同的协议）拆成 2–3 次 `Push` | 前几次 `Push` 不调用 `Analyze`（`Raw` 仍空，无成功的 `Valid=true` 消费）；最后一次补齐后 `Analyze` 运行，`Valid == true` |
| B2 | 一次 `Push` 含两帧完整文本 | 两帧都被分析；缓冲中不再剩下一帧完整未处理帧；`OnDataAnalyzed` 两次且 `Valid == true` |
| B3 | 噪声前缀 + 一帧完整文本（例如 `xx^&100$$`） | 抽出有效帧；`OnDataAnalyzed` 一次且 `Valid == true`；`Data` 与无噪声时的 A1 相同 |
| B4 | 先 `Push` 半包，再 `Push` 噪声，再 `Push` 剩余字节 | 按该协议的 `SearchBuffer` 规则完成或丢弃；过程不抛出未处理异常；测试进程不崩溃 |

B4 不规定唯一的完成/丢弃结果。文本协议在只有 `BeginOfLine`、没有 `EndOfLine` 时保持缓冲（`SearchResult.Mask`，`Raw` 为空）。噪声插入后是否还能拼回完整帧，取决于噪声是否破坏边界。断言「无未处理异常」是硬条件；完成或丢弃以所用协议的当前 `SearchBuffer` 为准。

### C Communicator 编排

| ID | 场景 | 期望 |
| --- | --- | --- |
| C1 | `new Communicator(null, analyzers)` | 抛出 `NullReferenceException`（现有行为；消息见第 5 节第 1 条） |
| C2 | 内部缓冲已超过 `ReadBufferSize`，再次 `Push` 更多数据 | 先清空再累积本次数据（当前实现：追加前若 `Count > ReadBufferSize` 则 `Clear()`，再 `AddRange`）。为缩短用例，测试可将 `ReadBufferSize` 设为较小正数（例如 8），先填到超过该值，再 `Push` |
| C3 | `OnRawDataReceived` 的一个订阅者抛出 `InvalidOperationException` | 该订阅者被移除；其他订阅者仍收到后续数据；`SearchBuffer` / `Analyze` 仍运行 |
| C4 | `Analyzers` 为空集合（非 `null`）；`Push` 若干字节 | 若已订阅则仍引发 `OnRawDataReceived`；不调用任何 `Analyze`；不崩溃 |

C2 记录现状，不把「丢弃旧缓冲」定义为产品理想。实现 PR 不要在本目录里「修好」溢出策略。

C4 的「空集合」是空的 `IAnalyzerCollection`。`Analyzers == null` 不在本目录。

### D 模型 / Valid 生命周期

| ID | 场景 | 期望 |
| --- | --- | --- |
| D1 | `Analyze` 先给 `Data` 赋值，再设 `Valid = true` | 在设 `Valid` 时引发 `OnDataAnalyzed`；处理器里可读到已赋值的 `Data`，且 `Valid == true` |
| D2 | `Valid = true` 之后，使用缩短的 `TimeOut`，等到定时器到期 | `Valid` 变为 `false`；再次引发 `OnDataAnalyzed`（README 超时行为）。`TimeOut` 公开单位为秒；武装定时器发生在 `Valid` 被设为 `true` 时。实现时应在置 `Valid = true` 之前把 `TimeOut` 设为较短秒数（例如 1），因为 `TimeOut` 的 setter 不会重新武装已启动的定时器 |
| D3 | `Analyze` 映射或不映射 `Data`，但从不设 `Valid` | 不出现成功的 `Valid == true` 消费；订阅 `OnDataAnalyzed` 时不应因 `Valid = true` 被调用 |

`OnDataAnalyzed` 在 `Valid` 的 setter 中触发，不在 `Analyze()` 结束时自动触发。D3 的 `Analyze` 必须避免写 `Valid`。

### E 扩展面

| ID | 场景 | 期望 |
| --- | --- | --- |
| E1 | 自定义 `ProtocolAnalyzer<T>`：重写 `SearchBuffer` 与 `Analyze`；经 `Communicator` + Fake `Push` 一帧完整自定义帧 | 与 A1 等价：`OnRawDataReceived` + `Analyze` + `OnDataAnalyzed` 且 `Valid == true`；`Data` 来自 `Raw` |
| E2 | 同一自定义解析器；`SearchBuffer` 在帧不完整时不写 `Raw`、不从缓冲删除未完成部分 | 与 B1 相同约定：不完整时不 `Analyze`；补齐后抽出一帧 |

E1/E2 的帧格式由测试自定义（例如单字节起始符 + 1 字节长度 + 载荷）。不要复用 NMEA 或 `CheckData16` 场景。

## 7. 范围外

- Demo WinForms（`Demo/`，`net8.0-windows`）。补充测试不得引用 Demo 类型，也不得改 Demo。
- 真实 COM 枚举、打开、开关的操作系统差异（Windows `COMn`、Linux 设备节点、权限、即插即用）。
- `netfx-1.x` 分支与 .NET Framework 1.x / 3.5 消费者。
- 再次测试 `verify/` 已覆盖的校验算法与 Handshake RTS/DTR。
- `ICommunication` 的 `ReadExisting` / `ReadLine` 文本读 API（本目录走字节 `Read` + `Write`）。
- `OnRawDataReceived` / `OnDataAnalyzed` 上除 `InvalidOperationException` 以外的异常处理。
- 更改 `ReadBufferSize` 溢出策略、`Communicator` 循环条件、或 `TimeOut` setter 是否重武装定时器。

## 8. 后续实现说明（非约束）

这些说明帮助实现 PR，不构成本设计的验收条件，也不指定框架。

- 在测试程序集中实现 `FakeCommunication`。用薄的 `TextProtocolAnalyzer` / `HexProtocolAnalyzer` 子类覆盖 A–D；E 用直接继承 `ProtocolAnalyzer<T>` 的子类。
- 保留现有 `verify/` 项目，继续负责校验与 RTS/DTR。不要把 A–E 搬进那三个控制台程序来「顺便」重跑校验。
- 测试宿主可以是 xUnit（或其他单元测试 SDK），也可以是与 `verify/` 相同的 `dotnet run` 控制台检查。本设计不强制。
- 测试项目目标框架与库一致：`net8.0`。不要引入 WinForms 或 `net8.0-windows`。
- A3 使用互不重叠的帧字节，避免文本 `^&`/`$$` 与 hex `Mask` 互相包含。
- D2 会等待最多约 1 秒。不要把默认 2 秒超时当作「立即失败」。不要依赖亚秒级 `TimeOut`（公开 API 以秒为单位）。
- C2 把 `ReadBufferSize` 调小，避免 `Push` 超过 2048 字节。
- `Communicator` 在 `Analyze()` 之后把 `Raw` 置为空数组。断言模型时用 `OnDataAnalyzed` 里看到的 `Data`/`Raw`，或在 `Analyze()` 内复制，不要在事件返回之后读解析器的 `Raw`。

## 9. 实现 PR 的成功标准

后续实现 PR（不是本文档 PR）在合并前应满足：

- 第 6 节全部 A–E 用例已自动化，并在无硬件的 CI 上通过。
- 现有 `verify/TextProtocolAnalyzerCheckData`、`verify/HexProtocolAnalyzerCheckLength`、`verify/SerialPortHandshakeNone` 仍通过，且职责不变。
- `Demo/` 无改动。
- 生产库无本设计要求的行为变更。若实现时发现现状与本表不一致，先更新本设计或开独立缺陷，不要在测试 PR 里默改契约。
