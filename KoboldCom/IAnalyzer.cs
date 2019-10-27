using System.Collections.Generic;

namespace KoboldCom
{
    /// <summary>
    /// 数据解析接口
    /// </summary>
    public interface IAnalyzer
    {
        /// <summary>
        /// 解析数据
        /// </summary>
        void Analyze();
        /// <summary>
        /// 缓冲区数据查询匹配操作
        /// </summary>
        /// <param name="buffer">数据缓冲区</param>
        /// <returns>匹配结果</returns>
        SearchResult SearchBuffer(List<byte> buffer);

        /// <summary>
        /// 未经协议转换处理的原始字节流数据
        /// </summary>
        byte[] Raw { get; set; }
    }
}
