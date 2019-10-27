using KoboldCom;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Demo
{
    public partial class Form1 : Form
    {
        readonly Communicator communicator = new Communicator(new SerialPort(), new MyProtocols());

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();
            Array.Sort(ports);
            comboPort.Items.AddRange(ports);
            comboPort.SelectedIndex = comboPort.Items.Count > 0 ? 0 : -1;
            comboBaudrate.SelectedIndex = 0;
            communicator.OnRawDataReceived += Communicator_OnRawDataReceived;

            //注册具体每个事件
            MyProtocols results = communicator.Analyzers as MyProtocols;
            if (results != null)
            {
                results.ProtocolText.OnDataAnalyzed += ProtocolText_OnDataAnalyzed; ;
                results.ProtocolBinary.OnDataAnalyzed += ProtocolBinary_OnDataAnalyzed;
            }
        }

        private void Communicator_OnRawDataReceived(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            textBoxRaw.Invoke((EventHandler)delegate
            {
                if (checkHex.Checked)
                {
                    foreach (byte b in bytes)
                    {
                        builder.Append(b.ToString("X2") + " ");
                    }
                }
                else
                {
                    builder.Append(communicator.Com.Encoding.GetString(bytes));
                }
                textBoxRaw.AppendText(builder.ToString());
            });
        }

        private void ProtocolBinary_OnDataAnalyzed(ProtocolAnalyzer<DemoDataModel> m)
        {
            ListViewItem item = new ListViewItem("DemoHexProtocol");
            item.SubItems.Add(m.ToString());
            item.SubItems.Add(m.Valid ? "DemoDataModel Analyzed" : "Data timeout");
            item.SubItems.Add(DateTime.Now.ToString("HH:mm:ss"));
            listViewData.Invoke((EventHandler)delegate
            {
                listViewData.Items.Add(item);
                listViewData.EnsureVisible(listViewData.Items.Count - 1);
            });
        }

        private void ProtocolText_OnDataAnalyzed(ProtocolAnalyzer<int> m)
        {
            ListViewItem item = new ListViewItem("DemoTextProtocol");
            item.SubItems.Add(m.ToString());
            item.SubItems.Add(m.Valid ? "New Data Analyzed" : "Data timeout");
            item.SubItems.Add(DateTime.Now.ToString("HH:mm:ss"));
            listViewData.Invoke((EventHandler)delegate
            {
                listViewData.Items.Add(item);
                listViewData.EnsureVisible(listViewData.Items.Count - 1);
            });
        }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (String.Compare(buttonConnect.Tag.ToString(), "OffLine", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    SerialPortSetting sps = new SerialPortSetting();
                    sps.Baudrate = int.Parse(comboBaudrate.Text);
                    sps.Port = int.Parse(Regex.Match(comboPort.Text, @"\d+").Value);
                    if (communicator.Com.Open(sps))
                    {
                        buttonConnect.Text = "Disconnect";
                        buttonConnect.Tag = "OnLine";
                    }
                }
                else
                {
                    System.Windows.Forms.Application.DoEvents();
                    communicator.Com.Close();
                    buttonConnect.Tag = "OffLine";
                    buttonConnect.Text = "Connect";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CheckWordWrap_CheckedChanged(object sender, EventArgs e)
        {
            textBoxRaw.WordWrap = checkWordWrap.Checked;
        }
    }
}
