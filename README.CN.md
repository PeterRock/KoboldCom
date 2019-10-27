### KoboldCom

`KoboldCom`是一个串口通信类库，提供自定义多协议异步解析、数据模型转换等功能。

封装最复杂的数据流异步收发，以及根据协议把接收的数据进行模型映射转换等功能。

类库提供了常见的16进制字节流协议`(HexProtocolAnalyzer)`和文本字节串协议`(TextProtocolAnalyzer)`的解析，所以可以使用KoboldCom，快速实现指定协议的数据收发

### Wiki How

常见的通讯协议一般是这样的：头+数据长度+数据正文+校验

    AA 44 05 01 02 03 04 05 EA

还有一种长这个样子：

    $GPGGA,121252.000,3937.3032,N,11611.6046,E,1,05,2.0,45.9,M,-5.7,M,,0000*77

其中$表示开始；GPGGA：命令字；*表示结尾;77校验

这两种协议很常见，所以SCommunicator默认提供了这两种常用的协议解析类（`HexProtocolAnalyzer`类和`TextProtocolAnalyzer`类）。

### Code Demo
可直接参考`/Demo`中的例子，包含完整的多协议处理

#### Usage 
```
// 创建通讯协议和模型转换方法
// Protocol1.cs
...Analyze(){
    // 处理数据模型转换
}


// 创建通讯器
var communicator = new KoboldCom.Communicator(new KoboldCom.SerialPort(), new MyProtocols());
// 通讯器处理接收原始数据
communicator.OnRawDataReceived += Communicator_OnRawDataReceived;
// 自定义模型转换后续操作
Protocol1.OnDataAnalyzed += ProtocolText_OnDataAnalyzed;
```

### Events

KoboldCom.Communicator对上层开放了两个事件

1. OnDataAnalyzed
从串口接收的数据中解析到符合协议要求的数据包，会触发的事件

2. OnDataReceived
串口收到新数据，会触发该事件

可以给这两个事件分别绑定处理方法就可以进行对应操作。

### More
如果这两种协议的解析方式不适合你的项目要求，可以继承抽象`ProtocolAnalyzer`类编写子类，一般只需要复写`SearchBuffer`方法,，也就是数据到协议的处理逻辑就可以了，其他的不需要改动。


### TODO:
- i18n
- TextProtocolAnalyzer CheckData 数据校验支持

### Thanks
触发要做这个东西的灵感来自[文章](http://blog.csdn.net/wuyazhe/article/details/5598945)


### License
MIT
