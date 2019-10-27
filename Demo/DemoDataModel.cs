using System.Globalization;

namespace Demo
{
    /// <summary>
    /// 数据模型示例
    /// </summary>
    public class DemoDataModel
    {
        public int Version { get; set; }
        public float Voltage { get; set; }
        public DemoDataModel()
        {
            Version = 0;
            Voltage = 0;
        }
        public override string ToString()
        {
            return string.Format("{0},{1}", Version.ToString(), Voltage.ToString(CultureInfo.InvariantCulture));
        }
    }
}
