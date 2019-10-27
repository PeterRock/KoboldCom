namespace KoboldCom
{
    /// <summary>
    /// 匹配结果类型
    /// </summary>
    public enum SearchResult
    {
        /// <summary>
        /// 在缓冲区中没有符合协议的内容
        /// </summary>
        None,

        /// <summary>
        /// 在缓冲区中找到协议头
        /// </summary>
        Mask,

        /// <summary>
        /// 在缓冲区中找到协议数据段
        /// </summary>
        Data,

        /// <summary>
        /// 在缓冲区中找到协议完整数据包
        /// </summary>
        All
    }
}
