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
}