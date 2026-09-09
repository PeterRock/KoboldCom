using System;
using System.Collections.Generic;
using KoboldCom;

namespace HexProtocolAnalyzerCheckLength
{
    internal static class Program
    {
        private static int _failures;

        private static int Main()
        {
            DefaultOneByteXorIsAccepted();
            DefaultOneByteXorRejectsBadChecksum();
            DemoLikeSumCheckFrameIsAccepted();
            TwoByteLittleEndianSumIsAccepted();
            TwoByteBigEndianSumIsAccepted();
            TwoByteLittleEndianRejectsSwappedBytes();
            TwoByteIncompleteWaits();
            InvalidThenValidTwoByteFindsValid();
            Crc16ModbusLittleEndianIsAccepted();
            StaticLengthTwoByteFrameIsAccepted();
            CheckLengthDefaultsToOne();

            if (_failures > 0)
            {
                Console.WriteLine("FAILED: {0} check(s)", _failures);
                return 1;
            }

            Console.WriteLine("All HexProtocolAnalyzer CheckLength checks passed.");
            return 0;
        }

        private static void DefaultOneByteXorIsAccepted()
        {
            DefaultXorProtocol p = new DefaultXorProtocol();
            // AA | len=2 | 01 02 | xor(01,02)=03
            List<byte> buffer = Bytes(0xAA, 0x02, 0x01, 0x02, 0x03);
            SearchResult result = p.SearchBuffer(buffer);
            Expect("xor1 result", SearchResult.All, result);
            Expect("xor1 raw", "AA02010203", Hex(p.Raw));
            Expect("xor1 leftover", 0, buffer.Count);
        }

        private static void DefaultOneByteXorRejectsBadChecksum()
        {
            DefaultXorProtocol p = new DefaultXorProtocol();
            List<byte> buffer = Bytes(0xAA, 0x02, 0x01, 0x02, 0xFF);
            SearchResult result = p.SearchBuffer(buffer);
            Expect("xor1 bad result", SearchResult.None, result);
            Expect("xor1 bad raw", 0, p.Raw.Length);
        }

        private static void DemoLikeSumCheckFrameIsAccepted()
        {
            DemoLikeProtocol p = new DemoLikeProtocol();
            // AA BB CC | 08 | 12 00 00 00 24 00 00 00 | 36
            List<byte> buffer = Bytes(
                0xAA, 0xBB, 0xCC, 0x08,
                0x12, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x00,
                0x36);
            SearchResult result = p.SearchBuffer(buffer);
            Expect("demo result", SearchResult.All, result);
            Expect("demo raw length", 13, p.Raw.Length);
            Expect("demo check", 0x36, p.Raw[12]);
        }

        private static void TwoByteLittleEndianSumIsAccepted()
        {
            TwoByteLeProtocol p = new TwoByteLeProtocol();
            // AA | 03 | 01 02 03 | sum16=0006 LE => 06 00
            List<byte> buffer = Bytes(0xAA, 0x03, 0x01, 0x02, 0x03, 0x06, 0x00);
            SearchResult result = p.SearchBuffer(buffer);
            Expect("sum16le result", SearchResult.All, result);
            Expect("sum16le raw", "AA030102030600", Hex(p.Raw));
            Expect("sum16le leftover", 0, buffer.Count);
        }

        private static void TwoByteBigEndianSumIsAccepted()
        {
            TwoByteBeProtocol p = new TwoByteBeProtocol();
            // AA | 03 | 01 02 03 | sum16=0006 BE => 00 06
            List<byte> buffer = Bytes(0xAA, 0x03, 0x01, 0x02, 0x03, 0x00, 0x06);
            SearchResult result = p.SearchBuffer(buffer);
            Expect("sum16be result", SearchResult.All, result);
            Expect("sum16be raw", "AA030102030006", Hex(p.Raw));
        }

        private static void TwoByteLittleEndianRejectsSwappedBytes()
        {
            TwoByteLeProtocol p = new TwoByteLeProtocol();
            // BE layout must not match LE protocol
            List<byte> buffer = Bytes(0xAA, 0x03, 0x01, 0x02, 0x03, 0x00, 0x06);
            SearchResult result = p.SearchBuffer(buffer);
            Expect("sum16le swapped result", SearchResult.None, result);
            Expect("sum16le swapped raw", 0, p.Raw.Length);
        }

        private static void TwoByteIncompleteWaits()
        {
            TwoByteLeProtocol p = new TwoByteLeProtocol();
            List<byte> buffer = Bytes(0xAA, 0x03, 0x01, 0x02, 0x03, 0x06);
            SearchResult result = p.SearchBuffer(buffer);
            Expect("incomplete result", SearchResult.Mask, result);
            Expect("incomplete raw", 0, p.Raw.Length);
            Expect("incomplete kept", 6, buffer.Count);
        }

        private static void InvalidThenValidTwoByteFindsValid()
        {
            TwoByteLeProtocol p = new TwoByteLeProtocol();
            List<byte> buffer = Bytes(
                0xAA, 0x03, 0x01, 0x02, 0x03, 0x00, 0x00,
                0xAA, 0x03, 0x01, 0x02, 0x03, 0x06, 0x00);
            SearchResult result = p.SearchBuffer(buffer);
            Expect("skip-bad result", SearchResult.All, result);
            Expect("skip-bad raw", "AA030102030600", Hex(p.Raw));
        }

        private static void Crc16ModbusLittleEndianIsAccepted()
        {
            Crc16Protocol p = new Crc16Protocol();
            // payload 01 03 00 00 00 0A , Modbus CRC = 0xCDC5, LE => C5 CD
            List<byte> buffer = Bytes(0xAA, 0x06, 0x01, 0x03, 0x00, 0x00, 0x00, 0x0A, 0xC5, 0xCD);
            SearchResult result = p.SearchBuffer(buffer);
            Expect("crc16 result", SearchResult.All, result);
            Expect("crc16 raw ends", "C5CD", Hex(p.Raw).Substring(Hex(p.Raw).Length - 4));
        }

        private static void StaticLengthTwoByteFrameIsAccepted()
        {
            StaticTwoByteProtocol p = new StaticTwoByteProtocol();
            // mask AA, 3 data bytes, LE sum16
            List<byte> buffer = Bytes(0xAA, 0x01, 0x02, 0x03, 0x06, 0x00);
            SearchResult result = p.SearchBuffer(buffer);
            Expect("static result", SearchResult.All, result);
            Expect("static raw", "AA0102030600", Hex(p.Raw));
        }

        private static void CheckLengthDefaultsToOne()
        {
            DefaultXorProtocol p = new DefaultXorProtocol();
            Expect("default CheckLength", 1, p.PublicCheckLength);
        }

        private static List<byte> Bytes(params byte[] values)
        {
            return new List<byte>(values);
        }

        private static string Hex(byte[] data)
        {
            char[] hex = new char[data.Length * 2];
            const string digits = "0123456789ABCDEF";
            for (int i = 0; i < data.Length; i++)
            {
                hex[i * 2] = digits[data[i] >> 4];
                hex[i * 2 + 1] = digits[data[i] & 0x0F];
            }
            return new string(hex);
        }

        private static void Expect<T>(string name, T expected, T actual)
        {
            if (!Equals(expected, actual))
            {
                _failures++;
                Console.WriteLine("FAIL {0}: expected {1}, got {2}", name, expected, actual);
            }
        }

        private sealed class DefaultXorProtocol : HexProtocolAnalyzer<int>
        {
            public DefaultXorProtocol()
            {
                Mask = new byte[] { 0xAA };
            }

            public int PublicCheckLength
            {
                get { return CheckLength; }
            }

            public override void Analyze()
            {
            }
        }

        private sealed class DemoLikeProtocol : HexProtocolAnalyzer<int>
        {
            public DemoLikeProtocol()
            {
                Mask = new byte[] { 0xAA, 0xBB, 0xCC };
                CheckData = SumCheck;
            }

            public override void Analyze()
            {
            }
        }

        private sealed class TwoByteLeProtocol : HexProtocolAnalyzer<int>
        {
            public TwoByteLeProtocol()
            {
                Mask = new byte[] { 0xAA };
                CheckLength = 2;
                CheckData16 = SumCheck16;
            }

            public override void Analyze()
            {
            }
        }

        private sealed class TwoByteBeProtocol : HexProtocolAnalyzer<int>
        {
            public TwoByteBeProtocol()
            {
                Mask = new byte[] { 0xAA };
                CheckLength = 2;
                CheckBigEndian = true;
                CheckData16 = SumCheck16;
            }

            public override void Analyze()
            {
            }
        }

        private sealed class Crc16Protocol : HexProtocolAnalyzer<int>
        {
            public Crc16Protocol()
            {
                Mask = new byte[] { 0xAA };
                CheckLength = 2;
                CheckData16 = Crc16Modbus;
            }

            public override void Analyze()
            {
            }
        }

        private sealed class StaticTwoByteProtocol : HexProtocolAnalyzer<int>
        {
            public StaticTwoByteProtocol()
            {
                Mask = new byte[] { 0xAA };
                StaticLength = 3;
                CheckLength = 2;
                CheckData16 = SumCheck16;
            }

            public override void Analyze()
            {
            }
        }
    }
}
