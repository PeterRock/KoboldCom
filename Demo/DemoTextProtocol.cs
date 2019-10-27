using KoboldCom;
using System.Text.RegularExpressions;

namespace Demo
{
    /// <summary>
    /// 常用文本协议定义示例
    /// DEMO TEXT: ^&100$$
    /// </summary>
    public class DemoTextProtocol : TextProtocolAnalyzer<int>
    {
        public DemoTextProtocol()
        {
            BeginOfLine = "^&";
            EndOfLine = "$$";
        }

        public override void Analyze()
        {
            string s = Encoding.GetString(Raw);
            Match m = Regex.Match(s, "\\d+");
            if (m.Success)
            {
                Data = int.Parse(m.Value);
                Valid = true;
            }
        }
    }
}
