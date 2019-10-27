using System.Text;

namespace KoboldCom
{
    /// <summary>
    /// 通讯端口类 接口
    /// </summary>
    public interface ICommunication
    {
        /// <summary>
        /// 解析出数据包之后触发的事件
        /// </summary>
        event DataReceivedHandler OnDataReceived;

        /// <summary>
        /// 通讯关闭
        /// </summary>
        void Close();

        /// <summary>
        /// 通讯开启
        /// </summary>
        /// <param name="setting">配置信息</param>
        /// <returns>执行结果</returns>
        bool Open(ICommunicationSetting setting);

        /// <summary>
        /// 读取端口数据放到指定数组中
        /// </summary>
        /// <param name="buffer">存放数据的数组</param>
        /// <param name="offset">起始位置</param>
        /// <param name="count">数据长度</param>
        void Read(byte[] buffer, int offset, int count);

        /// <summary>
        /// 在编码基础上，读取串口中所有立即可用字节
        /// </summary>
        string ReadExisting();

        /// <summary>
        /// 一直读取到SerialPort.NewLine值
        /// </summary>
        string ReadLine();

        /// <summary>
        /// 将指定字符串写入到串口
        /// </summary>
        /// <param name="text">要写入的内容</param>
        void Write(string text);

        /// <summary>
        /// 把指定数据写入到串口
        /// </summary>
        /// <param name="buffer">要写入的数组</param>
        /// <param name="offset">数组起始位置</param>
        /// <param name="count">长度</param>
        void Write(byte[] buffer, int offset, int count);

        /// <summary>
        /// 将指定字符串和SerialPort.NewLine写入到串口
        /// </summary>
        /// <param name="text">指定字符串</param>
        void WriteLine(string text);


        /// <summary>
        /// 获取缓冲区中数据的字节数
        /// </summary>
        int BytesToRead { get; }

        /// <summary>
        /// 获取或设置传输前后文本的编码方式
        /// </summary>
        Encoding Encoding { get; set; }

        /// <summary>
        /// 获取串口打开状态
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// 获取或设置NewLine标志内容
        /// </summary>
        string NewLine { get; set; }

        /// <summary>
        /// 获取或设置配置信息
        /// </summary>
        ICommunicationSetting Setting { get; set; }
    }
}
