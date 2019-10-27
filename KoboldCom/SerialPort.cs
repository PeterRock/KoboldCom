using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;

namespace KoboldCom
{
    /// <summary>
    /// 具体通讯类
    /// </summary>
    public class SerialPort : ICommunication
    {
        private bool _closing;
        private readonly List<DataReceivedHandler> _listDataReceivedHandler = new List<DataReceivedHandler>();
        private System.IO.Ports.SerialPort _serialPort = new System.IO.Ports.SerialPort();

        /// <summary>
        /// 串口接收到符合协议的数据包触发的事件
        /// </summary>
        public event DataReceivedHandler OnDataReceived;

        /// <summary>
        /// 关闭串口
        /// </summary>
        public void Close()
        {
            this._closing = true;
            this._serialPort.Close();
            this._closing = false;
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="setting">串口配置信息</param>
        /// <returns>操作结果</returns>
        public bool Open(ICommunicationSetting setting)
        {
            return this.Open(setting as SerialPortSetting);
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="setting">串口配置信息</param>
        /// <returns>操作结果</returns>
        public bool Open(SerialPortSetting setting)
        {
            this.Setting = setting;
            try
            {
                this._serialPort.Open();
                this._serialPort.DataReceived += new SerialDataReceivedEventHandler(this.SerialPortDataReceived);
            }
            catch
            {
                this._serialPort = new System.IO.Ports.SerialPort();
            }
            return this._serialPort.IsOpen;
        }

        /// <summary>
        /// 从串口读取消息
        /// </summary>
        /// <param name="buffer">缓冲区</param>
        /// <param name="offset">偏移量</param>
        /// <param name="count">长度</param>
        public void Read(byte[] buffer, int offset, int count)
        {
            this._serialPort.Read(buffer, offset, count);
        }

        /// <summary>
        /// 在编码基础上，读取串口中所有立即可用字节
        /// </summary>
        public string ReadExisting()
        {
            return this._serialPort.ReadExisting();
        }

        /// <summary>
        /// 一直读取到SerialPort.NewLine值
        /// </summary>
        public string ReadLine()
        {
            return this._serialPort.ReadLine();
        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if ((this.OnDataReceived != null) && !this._closing)
            {
                this._listDataReceivedHandler.Clear();
                Delegate[] invocationList = this.OnDataReceived.GetInvocationList();
                int index = 0;
                while (true)
                {
                    if (index >= invocationList.Length)
                    {
                        break;
                    }
                    var item = (DataReceivedHandler)invocationList[index];
                    try
                    {
                        item(this);
                    }
                    catch (InvalidOperationException)
                    {
                        this._listDataReceivedHandler.Add(item);
                    }
                    index++;
                }
                foreach (DataReceivedHandler handler in this._listDataReceivedHandler)
                {
                    this.OnDataReceived = (DataReceivedHandler)Delegate.Remove(this.OnDataReceived, handler);
                }
            }
        }

        /// <summary>
        /// 将指定字符串写入到串口
        /// </summary>
        /// <param name="text">要写入的内容</param>
        public void Write(string text)
        {
            this._serialPort.Write(text);
        }

        /// <summary>
        /// 把指定数据写入到串口
        /// </summary>
        /// <param name="buffer">要写入的数组</param>
        /// <param name="offset">数组起始位置</param>
        /// <param name="count">长度</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            this._serialPort.Write(buffer, offset, count);
        }

        /// <summary>
        /// 将指定字符串和SerialPort.NewLine写入到串口
        /// </summary>
        /// <param name="text">指定字符串</param>
        public void WriteLine(string text)
        {
            this._serialPort.WriteLine(text);
        }

        /// <summary>
        /// 获取缓冲区中数据的字节数
        /// </summary>
        public int BytesToRead
        {
            get { return this._serialPort.BytesToRead; }
        }

        /// <summary>
        /// 获取或设置传输前后文本的编码方式
        /// </summary>
        public Encoding Encoding
        {
            get { return this._serialPort.Encoding; }
            set { this._serialPort.Encoding = value; }
        }

        /// <summary>
        /// 获取串口打开状态
        /// </summary>
        public bool IsOpen
        {
            get { return this._serialPort.IsOpen; }
        }

        /// <summary>
        /// 获取或设置NewLine标志内容
        /// </summary>
        public string NewLine
        {
            get { return this._serialPort.NewLine; }
            set { this._serialPort.NewLine = value; }
        }

        /// <summary>
        /// 获取或设置配置信息
        /// </summary>
        public ICommunicationSetting Setting
        {
            get
            {
                return new SerialPortSetting
                {
                    StopBits = this._serialPort.StopBits,
                    Baudrate = this._serialPort.BaudRate,
                    Handshake = this._serialPort.Handshake,
                    NewLine = this._serialPort.NewLine,
                    Parity = this._serialPort.Parity,
                    Port = int.Parse(Regex.Match(this._serialPort.PortName, @"\d+").Value)
                };
            }
            set
            {
                SerialPortSetting setting = value as SerialPortSetting;
                if (value == null)
                {
                    throw new InvalidCastException("setting type invalid.");
                }
                if (setting == null) return;
                this._serialPort.PortName = "COM" + setting.Port;
                this._serialPort.Parity = setting.Parity;
                this._serialPort.NewLine = setting.NewLine;
                this._serialPort.Handshake = setting.Handshake;
                this._serialPort.BaudRate = setting.Baudrate;
                this._serialPort.StopBits = setting.StopBits;
            }
        }
    }
}
