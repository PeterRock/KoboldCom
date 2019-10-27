using System.Collections.Generic;
using System.Text;

namespace KoboldCom
{
    /// <summary>
    /// 文本通讯协议
    /// [BeginOfLine******EndOfLine]
    /// </summary>
    /// <typeparam name="T">数据解析结果类</typeparam>
    public abstract class TextProtocolAnalyzer<T> : ProtocolAnalyzer<T> where T : new()
    {
        /// <summary>
        /// 创建文本协议分析对象
        /// </summary>
        protected TextProtocolAnalyzer()
        {
            this.BeginOfLine = "";
            this.EndOfLine = "\r\n";
            this.Encoding = Encoding.ASCII;
        }
        /// <summary>
        /// 数据包解析匹配方法规则
        /// </summary>
        /// <param name="buffer">要分析的数据</param>
        /// <returns>分析结果</returns>
        public override SearchResult SearchBuffer(List<byte> buffer)
        {
            string str = this.Encoding.GetString(buffer.ToArray());
            int bgnIndex = str.IndexOf(this.BeginOfLine, System.StringComparison.Ordinal);
            if (bgnIndex == -1)
            {
                return SearchResult.None;
            }
            int endIndex = str.IndexOf(this.EndOfLine, System.StringComparison.Ordinal);
            if (endIndex == -1)
            {
                return SearchResult.Mask;
            }
            base.Raw = new byte[(endIndex - bgnIndex) + this.EndOfLine.Length];
            buffer.CopyTo(bgnIndex, base.Raw, 0, base.Raw.Length);//将Buffer中的数据拷贝到Raw中
            buffer.RemoveRange(bgnIndex, base.Raw.Length);//把拷贝好的数据从缓冲区中移除
            return SearchResult.All;
        }

        /// <summary>
        /// 数据包开始标志
        /// </summary>
        public string BeginOfLine { get; set; }
        /// <summary>
        /// 数据编码方式
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// 数据包结束标志
        /// </summary>
        public string EndOfLine { get; set; }
    }
}
