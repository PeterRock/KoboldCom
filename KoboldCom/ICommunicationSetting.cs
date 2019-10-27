namespace KoboldCom
{
    /// <summary>
    /// 串口配置
    /// </summary>
    public interface ICommunicationSetting
    {
        /// <summary>
        /// 消息内容转为一个字节数组格式返回
        /// </summary>
        /// <returns></returns>
        byte[] AsBytes();
    }
}
