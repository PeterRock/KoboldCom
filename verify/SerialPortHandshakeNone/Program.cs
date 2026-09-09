using System;
using System.IO.Ports;
using KoboldCom;
using SerialPort = KoboldCom.SerialPort;

namespace SerialPortHandshakeNone
{
    internal static class Program
    {
        private static int _failures;

        private static int Main()
        {
            // Settings-only: does not Open a real COM port. ApplyControlLines is the same helper used after Open.
            SettingDefaultsEnableRtsDtr();
            HandshakeNoneAppliesRtsDtr();
            HandshakeNoneHonorsDisabledFlags();
            RequestToSendDoesNotThrow();
            RequestToSendReportsControlLinesInactive();

            if (_failures > 0)
            {
                Console.WriteLine("FAILED: {0} check(s)", _failures);
                return 1;
            }

            Console.WriteLine("All SerialPort Handshake.None RTS/DTR checks passed.");
            return 0;
        }

        private static void SettingDefaultsEnableRtsDtr()
        {
            SerialPortSetting setting = new SerialPortSetting();
            Expect("default handshake", Handshake.None, setting.Handshake);
            Expect("default RtsEnable", true, setting.RtsEnable);
            Expect("default DtrEnable", true, setting.DtrEnable);
        }

        private static void HandshakeNoneAppliesRtsDtr()
        {
            SerialPort port = new SerialPort();
            port.Setting = new SerialPortSetting
            {
                Port = 1,
                Handshake = Handshake.None,
                RtsEnable = true,
                DtrEnable = true
            };
            SerialPortSetting applied = (SerialPortSetting)port.Setting;
            Expect("applied handshake", Handshake.None, applied.Handshake);
            Expect("applied RtsEnable", true, applied.RtsEnable);
            Expect("applied DtrEnable", true, applied.DtrEnable);
        }

        private static void HandshakeNoneHonorsDisabledFlags()
        {
            SerialPort port = new SerialPort();
            port.Setting = new SerialPortSetting
            {
                Port = 1,
                Handshake = Handshake.None,
                RtsEnable = false,
                DtrEnable = false
            };
            SerialPortSetting applied = (SerialPortSetting)port.Setting;
            Expect("disabled RtsEnable", false, applied.RtsEnable);
            Expect("disabled DtrEnable", false, applied.DtrEnable);
        }

        private static void RequestToSendDoesNotThrow()
        {
            SerialPort port = new SerialPort();
            try
            {
                port.Setting = new SerialPortSetting
                {
                    Port = 1,
                    Handshake = Handshake.RequestToSend,
                    RtsEnable = true,
                    DtrEnable = true
                };
                Expect("rts handshake", Handshake.RequestToSend, ((SerialPortSetting)port.Setting).Handshake);
            }
            catch (Exception ex)
            {
                _failures++;
                Console.WriteLine("FAIL request-to-send throw: {0}", ex.GetType().Name);
            }
        }

        private static void RequestToSendReportsControlLinesInactive()
        {
            SerialPort port = new SerialPort();
            port.Setting = new SerialPortSetting
            {
                Port = 1,
                Handshake = Handshake.RequestToSend,
                RtsEnable = true,
                DtrEnable = true
            };
            SerialPortSetting applied = (SerialPortSetting)port.Setting;
            Expect("rts mode RtsEnable inactive", false, applied.RtsEnable);
            Expect("rts mode DtrEnable inactive", false, applied.DtrEnable);
        }

        private static void Expect<T>(string name, T expected, T actual)
        {
            if (!Equals(expected, actual))
            {
                _failures++;
                Console.WriteLine("FAIL {0}: expected {1}, got {2}", name, expected, actual);
            }
        }
    }
}
