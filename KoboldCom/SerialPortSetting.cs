using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace KoboldCom
{
    /// <summary>
    /// 串口设置
    /// </summary>
    public class SerialPortSetting : ICommunicationSetting
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public SerialPortSetting()
        {
            this.Port = 1;
            this.Baudrate = 0x2580;
            this.StopBits = StopBits.One;
            this.Parity = Parity.None;
            this.Handshake = Handshake.None;
            this.NewLine = "\r\n";
            this.RtsEnable = true;
            this.DtrEnable = true;
        }

        /// <summary>
        /// 消息内容转为一个字节数组返回
        /// </summary>
        /// <returns></returns>
        public byte[] AsBytes()
        {
            var list = new List<byte>
            {
                1,
                (byte) this.Port
            };
            list.AddRange(BitConverter.GetBytes(this.Baudrate));
            list.Add((byte)this.StopBits);
            list.Add((byte)this.Parity);
            list.Add((byte)this.Handshake);
            list.AddRange(Encoding.ASCII.GetBytes(this.NewLine));
            return list.ToArray();
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string str = this.NewLine.Aggregate("", (current, ch) => current + ((byte)ch).ToString("X2"));
            return ("1" + this.Port.ToString() + "," + this.Baudrate.ToString() + "," + ((int)this.StopBits).ToString() +
                    "," + ((int)this.Parity).ToString() + "," + ((int)this.Handshake).ToString() + "," + str);
        }

        /// <summary>
        /// 波特率
        /// </summary>
        public int Baudrate;

        /// <summary>
        /// 握手协议
        /// </summary>
        public Handshake Handshake;

        /// <summary>
        /// Handshake 为 None 时是否拉高 RTS。默认 true，避免无硬件握手设备在 Write 后收不到应答。
        /// Handshake 不为 None 时由系统握手逻辑控制 RTS，本属性不生效。
        /// </summary>
        public bool RtsEnable;

        /// <summary>
        /// Handshake 为 None 时是否拉高 DTR。默认 true。
        /// Handshake 不为 None 时由系统握手逻辑控制 DTR，本属性不生效。
        /// </summary>
        public bool DtrEnable;

        /// <summary>
        /// 新行标识
        /// </summary>
        public string NewLine;

        /// <summary>
        /// 奇偶校验位
        /// </summary>
        public Parity Parity;

        /// <summary>
        /// 串口号
        /// </summary>
        public int Port;

        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBits;
    }
}
