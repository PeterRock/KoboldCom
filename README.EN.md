[中文](/README.CN.md)
### KoboldCom
KoboldCom is serial port communication lib.

Support Custom protocol, async data receive handler and analyzer.

Support basic HexProtocolAnalyzer and TextProtocolAnalyzer. So you can implement a protocol quickly.

TextProtocolAnalyzer can optionally validate checksums via `CheckData` (same delegate as hex protocols). In a subclass constructor, for NMEA-style `$...*HH`, set `BeginOfLine = "$"`, `EndOfLine = "*"`, and `CheckData = XorCheck`. `CheckLength` defaults to 2 hex ASCII digits after `EndOfLine`. Leave `CheckData` unset to keep existing no-checksum text protocols unchanged. `EndOfLine` is searched after `BeginOfLine`. Complete frames with a bad checksum are discarded from the buffer and are not treated as valid packets.


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
