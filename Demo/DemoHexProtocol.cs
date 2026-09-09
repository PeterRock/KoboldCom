using KoboldCom;
using System;

namespace Demo
{
    /// <summary>
    /// 常用十六进制数据字节流协议定义示例
    /// DEMO HEX DATA: AA BB CC 08 12 00 00 00 24 00 00 00 36
    /// </summary>
    public class DemoHexProtocol : HexProtocolAnalyzer<DemoDataModel>
    {
        public DemoHexProtocol()
        {
            this.Mask = new byte[] { 0xAA, 0xBB, 0xCC };
            this.TimeOut = 5;//超过5秒，收不到数据，则此数据无效。
            // 默认 CheckLength = 1（单字节校验）。双字节时设置 CheckLength = 2 并指定 CheckData16。
            this.CheckData = SumCheck; // 使用 HexProtocolAnalyzer 提供的和校验方法
        }
        /// <summary>
        /// 数据解析协议
        /// 根据协议内容，对接收到的数据包进行解析，转换成Model抛出
        /// </summary>
        public override void Analyze()
        {
            int offset = Mask.Length + LenLength;//_mask.Length表示标记后的一个字节,_mask.Length+1表示标记后的第二个字节，有一个字节表示长度。
            this.Data.Version = BitConverter.ToInt32(Raw, offset + 0);
            this.Data.Voltage = BitConverter.ToInt32(Raw, offset + 4); // int32有4个byte, 
            this.Valid = true;//注意要设置数据有效状态,会触发DataAnalyzed通知事件
        }
    }
}
