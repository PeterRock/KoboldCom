using System.Collections.Generic;

namespace KoboldCom
{
    /// <summary>
    /// 校验方法委托
    /// </summary>
    /// <param name="buff">要检验的数据</param>
    /// <param name="i">起始位置</param>
    /// <param name="length">数据长度</param>
    /// <returns>校验方法计算结果</returns>
    public delegate byte CheckDataHandler(List<byte> buff, int i, int length);

    /// <summary>
    /// 双字节校验方法委托。返回 16 位校验值（主机数值，比较时再按大小端还原）。
    /// </summary>
    /// <param name="buff">要检验的数据</param>
    /// <param name="i">起始位置</param>
    /// <param name="length">数据长度</param>
    /// <returns>16 位校验计算结果</returns>
    public delegate int CheckData16Handler(List<byte> buff, int i, int length);
}