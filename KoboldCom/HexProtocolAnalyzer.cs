using System;
using System.Collections.Generic;

namespace KoboldCom
{
    /// <summary>
    /// 通用二进制通讯协议解析类
    /// [HEAD_MASK, DATALEN, DATA, CHECK]
    /// </summary>
    public abstract class HexProtocolAnalyzer<T> : ProtocolAnalyzer<T> where T : new()
    {
        /// <summary>
        /// 数据校验，默认使用异或校验
        /// </summary>
        protected CheckDataHandler CheckData;
        /// <summary>
        /// 双字节校验。CheckLength 为 2 时优先使用；未设置则把 CheckData 的单字节结果按 16 位值比较。
        /// </summary>
        protected CheckData16Handler CheckData16;
        private int _stickLength;//定长数据-数据中不含数据长度的情况
        private int _checkLength;

        /// <summary>
        /// 构造函数 Protected
        /// </summary>
        protected HexProtocolAnalyzer()
        {
            Mask = new byte[0];
            CheckData = new CheckDataHandler(HexProtocolAnalyzer<T>.XorCheck);
            _stickLength = 0;//置为0：默认非定长数据
            LenLength = 1;
            CheckLength = 1;
        }

        /// <summary>
        /// 数据=>协议处理转换
        /// </summary>
        /// <param name="buffer">缓冲区数据</param>
        /// <returns>匹配结果</returns>
        public override SearchResult SearchBuffer(List<byte> buffer)
        {
            SearchResult sResult = SearchResult.None;//结果
            int index = 0;		//数组包起始位置游标
            int minLen = Mask.Length + 1;//一个数据包的最小长度 数据头+1
            while ((buffer.Count - index) > minLen)	//buff数据不够一个数据包就不循环
            {
                if (sResult != SearchResult.None)	//搜索结果不为数据头
                {
                    continue;	//直接执行下次循环
                }
                // 查找数据头
                sResult = SearchResult.Mask;	// 满足以上条件之后--设置搜索结果为数据头
                int maskIndex = 0;
                while (maskIndex < Mask.Length)
                {
                    if (buffer[index + maskIndex] != Mask[maskIndex])
                    {
                        index += (maskIndex == 0) ? 1 : maskIndex;
                        sResult = SearchResult.None;
                        break;
                    }
                    maskIndex++;
                }
                if (sResult == SearchResult.Mask)
                {
                    //从数据包中读取数据长度，即在数据头后面的值，并且根据单字节还是多字节进行处理计算
                    int dataLength;
                    if (_stickLength == 0)
                    {
                        //非定长情况
                        switch (LenLength)
                        {
                            case 2:
                                dataLength = BitConverter.ToInt16(buffer.ToArray(), index + Mask.Length);//转换双字节数据
                                break;
                            case 4:
                                dataLength = BitConverter.ToInt32(buffer.ToArray(), index + Mask.Length);//转换4字节数据
                                break;
                            default:
                                dataLength = buffer[index + Mask.Length];//-转换单字节数据
                                break;
                        }
                    }
                    else
                    {
                        //定长情况
                        LenLength = 0;//对于定长数据这个值应该为0，避免下面调用这个值出错
                        dataLength = _stickLength;
                    }

                    // 判断Buff中的数据是否够一个数据包, 如果不够：返回数据头标志，等待buff继续缓存新数据
                    if (buffer.Count - index - (Mask.Length + LenLength) - CheckLength < dataLength)
                    {
                        return SearchResult.Mask;
                    }
                    int dataIndex = (index + Mask.Length) + LenLength;
                    int checkIndex = index + (Mask.Length + dataLength) + LenLength;
                    //-数据校验
                    if (!ChecksumMatches(buffer, dataIndex, dataLength, checkIndex))
                    {
                        Console.WriteLine(index + "-" + buffer.Count + "-" + maskIndex);
                        // 对于校验不符合要求的数据，继续向后搜寻。(数据头是协议中定义出来的特异标志肯定不会出现在数据中)
                        index += Mask.Length > 0 ? Mask.Length : 1;
                        sResult = SearchResult.None;
                    }
                    else
                    {
                        int count = ((Mask.Length + CheckLength) + dataLength) + LenLength;//数据包总长度
                        //拷贝数据包到Raw，然后以该数据包为分界点，清除buffer中匹配过的数据
                        Raw = new byte[count];
                        Console.WriteLine(index + "~~" + count);
                        buffer.CopyTo(index, Raw, 0, count);
                        buffer.RemoveRange(0, index + count);
                        return SearchResult.All;
                    }
                }
            }
            return SearchResult.None;
        }

        /// <summary>
        /// 异或校验方法
        /// </summary>
        public static byte XorCheck(List<byte> buf, int index, int len)
        {
            byte num = 0;
            for (int i = index; i < (index + len); i++)
            {
                num ^= buf[i];
            }
            return num;
        }
        /// <summary>
        /// 和校验方法
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="index"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static byte SumCheck(List<byte> buf, int index, int len)
        {
            byte num = 0;
            for (int i = index; i < (index + len); i++)
            {
                num = (byte)(num + buf[i]);
            }
            return num;
        }

        /// <summary>
        /// 16 位累加和（取低 16 位）。配合 CheckLength = 2 使用。
        /// </summary>
        public static int SumCheck16(List<byte> buf, int index, int len)
        {
            int num = 0;
            for (int i = index; i < (index + len); i++)
            {
                num = (num + buf[i]) & 0xFFFF;
            }
            return num;
        }

        /// <summary>
        /// Modbus CRC-16（多项式 0xA001，初值 0xFFFF）。默认按小端与帧尾两字节比较。
        /// </summary>
        public static int Crc16Modbus(List<byte> buf, int index, int len)
        {
            int crc = 0xFFFF;
            for (int i = index; i < (index + len); i++)
            {
                crc ^= buf[i];
                for (int n = 0; n < 8; n++)
                {
                    if ((crc & 1) != 0)
                    {
                        crc = (crc >> 1) ^ 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return crc & 0xFFFF;
        }

        private bool ChecksumMatches(List<byte> buffer, int dataIndex, int dataLength, int checkIndex)
        {
            if (CheckLength <= 0)
            {
                return true;
            }
            if (CheckLength == 1)
            {
                if (CheckData == null)
                {
                    return true;
                }
                return CheckData(buffer, dataIndex, dataLength) == buffer[checkIndex];
            }

            int computed;
            if (CheckData16 != null)
            {
                computed = CheckData16(buffer, dataIndex, dataLength) & 0xFFFF;
            }
            else if (CheckData != null)
            {
                computed = CheckData(buffer, dataIndex, dataLength);
            }
            else
            {
                return true;
            }

            int expected = CheckBigEndian
                ? ((buffer[checkIndex] << 8) | buffer[checkIndex + 1])
                : (buffer[checkIndex] | (buffer[checkIndex + 1] << 8));
            return computed == expected;
        }

        /// <summary>
        /// 定长数据长度-默认不含数据校验位
        /// </summary>
        public int StaticLength
        {
            get { return _stickLength; }
            set { _stickLength = value < 0 ? 0 : value; }
        }

        /// <summary>
        /// 数据长度值是几个字节的数据
        /// </summary>
        protected int LenLength { get; set; }

        /// <summary>
        /// 帧尾校验字节数。默认 1，保持现有单字节 XOR/SUM 协议兼容。
        /// 设为 2 时按二进制双字节比较：默认小端（低字节在前，与 Modbus CRC-16 一致）；
        /// 高字节在前的协议请设置 CheckBigEndian = true。
        /// 小于等于 0 表示帧中不含校验段、也不做校验。其他正数按 2 处理。
        /// </summary>
        public int CheckLength
        {
            get { return _checkLength; }
            set
            {
                if (value == 1)
                {
                    _checkLength = 1;
                }
                else if (value <= 0)
                {
                    _checkLength = 0;
                }
                else
                {
                    _checkLength = 2;
                }
            }
        }

        /// <summary>
        /// CheckLength 为 2 时，帧尾两字节是否按大端（高字节在前）解释。默认 false（小端）。
        /// </summary>
        public bool CheckBigEndian { get; set; }

        /// <summary>
        /// 数据头
        /// </summary>
        public byte[] Mask { get; protected set; }
    }
}
