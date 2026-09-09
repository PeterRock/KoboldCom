using System;
using System.Collections.Generic;
using System.Text;

namespace KoboldCom
{
    /// <summary>
    /// 文本通讯协议
    /// [BeginOfLine******EndOfLine]
    /// 可选校验：[BeginOfLine][数据][EndOfLine][校验]
    /// NMEA 示例：$GPGGA,...*HH ，其中 * 为结束标志，HH 为 $ 与 * 之间字符的异或校验
    /// </summary>
    /// <typeparam name="T">数据解析结果类</typeparam>
    public abstract class TextProtocolAnalyzer<T> : ProtocolAnalyzer<T> where T : new()
    {
        /// <summary>
        /// 数据校验，默认不校验（保持无校验文本协议兼容）
        /// </summary>
        protected CheckDataHandler CheckData;
        private int _checkLength;

        /// <summary>
        /// 创建文本协议分析对象
        /// </summary>
        protected TextProtocolAnalyzer()
        {
            this.BeginOfLine = "";
            this.EndOfLine = "\r\n";
            this.Encoding = Encoding.ASCII;
            this.CheckLength = 2;
        }
        /// <summary>
        /// 数据包解析匹配方法规则
        /// </summary>
        /// <param name="buffer">要分析的数据</param>
        /// <returns>分析结果</returns>
        public override SearchResult SearchBuffer(List<byte> buffer)
        {
            string str = this.Encoding.GetString(buffer.ToArray());
            int searchFrom = 0;
            int discardThrough = 0;
            bool hasCheck = (this.CheckData != null) && (this.CheckLength > 0);

            while (searchFrom < str.Length)
            {
                int bgnIndex = str.IndexOf(this.BeginOfLine, searchFrom, StringComparison.Ordinal);
                if (bgnIndex == -1)
                {
                    DiscardPrefix(buffer, discardThrough);
                    return SearchResult.None;
                }

                int payloadStart = bgnIndex + this.BeginOfLine.Length;
                int endIndex = str.IndexOf(this.EndOfLine, payloadStart, StringComparison.Ordinal);
                if (endIndex == -1)
                {
                    DiscardPrefix(buffer, discardThrough);
                    return SearchResult.Mask;
                }

                int frameEnd = endIndex + this.EndOfLine.Length;
                if (hasCheck)
                {
                    if (str.Length < (frameEnd + this.CheckLength))
                    {
                        DiscardPrefix(buffer, discardThrough);
                        return SearchResult.Mask;
                    }

                    byte computed = this.CheckData(buffer, payloadStart, endIndex - payloadStart);
                    byte expected;
                    if (!this.TryGetCheckValue(buffer, str, frameEnd, out expected) || (computed != expected))
                    {
                        // 校验失败：不作为有效数据包；跳过本次开始标志继续搜寻，并记录可丢弃的完整坏帧
                        discardThrough = frameEnd + this.CheckLength;
                        searchFrom = bgnIndex + ((this.BeginOfLine.Length > 0) ? this.BeginOfLine.Length : 1);
                        continue;
                    }
                    frameEnd += this.CheckLength;
                }

                base.Raw = new byte[frameEnd - bgnIndex];
                buffer.CopyTo(bgnIndex, base.Raw, 0, base.Raw.Length);//将Buffer中的数据拷贝到Raw中
                buffer.RemoveRange(0, frameEnd);//清除坏帧前缀和本帧，与 Hex 协议在匹配成功后从 0 移除一致
                return SearchResult.All;
            }

            DiscardPrefix(buffer, discardThrough);
            return SearchResult.None;
        }

        private static void DiscardPrefix(List<byte> buffer, int count)
        {
            if ((count > 0) && (count <= buffer.Count))
            {
                buffer.RemoveRange(0, count);
            }
        }

        /// <summary>
        /// 读取结束标志后的期望校验值。
        /// CheckLength 为 1 时按单字节二进制比较；为 2 时按两位十六进制 ASCII（NMEA）解析。
        /// </summary>
        private bool TryGetCheckValue(List<byte> buffer, string str, int checkIndex, out byte value)
        {
            value = 0;
            if (this.CheckLength == 1)
            {
                value = buffer[checkIndex];
                return true;
            }
            if (this.CheckLength == 2)
            {
                try
                {
                    value = Convert.ToByte(str.Substring(checkIndex, 2), 16);
                    return true;
                }
                catch (FormatException)
                {
                    return false;
                }
                catch (ArgumentException)
                {
                    return false;
                }
                catch (OverflowException)
                {
                    return false;
                }
            }
            return false;
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

        /// <summary>
        /// 结束标志后的校验数据长度。默认 2（NMEA 两位十六进制）。
        /// 仅在 CheckData 不为空时生效。1 表示单字节二进制校验，2 表示两位十六进制 ASCII。
        /// 其他正数按 2 处理；小于等于 0 表示不读取校验段。
        /// </summary>
        public int CheckLength
        {
            get
            {
                return this._checkLength;
            }
            set
            {
                if (value == 1)
                {
                    this._checkLength = 1;
                }
                else if (value <= 0)
                {
                    this._checkLength = 0;
                }
                else
                {
                    this._checkLength = 2;
                }
            }
        }
    }
}
