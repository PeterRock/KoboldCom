### KoboldCom
KoboldCom is serial port communication lib.

Support Custom protocol, async data receive handler and analyzer.

Support basic HexProtocolAnalyzer and TextProtocolAnalyzer. So you can implement a protocol quickly.


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