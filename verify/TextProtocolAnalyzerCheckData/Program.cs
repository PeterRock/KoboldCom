using System;
using System.Collections.Generic;
using System.Text;
using KoboldCom;

namespace TextProtocolAnalyzerCheckData
{
    internal static class Program
    {
        private static int _failures;

        private static int Main()
        {
            NoChecksumDemoFrameStillExtracts();
            ValidNmeaChecksumIsAccepted();
            ReadmeNmeaExampleIsAccepted();
            InvalidNmeaChecksumIsRejected();
            InvalidThenValidFrameFindsValid();
            IncompleteChecksumWaits();
            BinaryChecksumAfterEndOfLine();
            EndOfLineBeforeBeginIsIgnored();

            if (_failures > 0)
            {
                Console.WriteLine("FAILED: {0} check(s)", _failures);
                return 1;
            }

            Console.WriteLine("All TextProtocolAnalyzer CheckData checks passed.");
            return 0;
        }

        private static void NoChecksumDemoFrameStillExtracts()
        {
            DemoLikeProtocol p = new DemoLikeProtocol();
            List<byte> buffer = Bytes("^&100$$");
            SearchResult result = p.SearchBuffer(buffer);
            Expect("demo result", SearchResult.All, result);
            Expect("demo raw", "^&100$$", Encoding.ASCII.GetString(p.Raw));
            Expect("demo leftover", 0, buffer.Count);
        }

        private static void ValidNmeaChecksumIsAccepted()
        {
            NmeaLikeProtocol p = new NmeaLikeProtocol();
            List<byte> buffer = Bytes("$GPGGA,1*4B\r\n");
            SearchResult result = p.SearchBuffer(buffer);
            Expect("nmea valid result", SearchResult.All, result);
            Expect("nmea valid raw", "$GPGGA,1*4B", Encoding.ASCII.GetString(p.Raw));
            Expect("nmea leftover crlf", "\r\n", Encoding.ASCII.GetString(buffer.ToArray()));
        }

        private static void ReadmeNmeaExampleIsAccepted()
        {
            NmeaLikeProtocol p = new NmeaLikeProtocol();
            List<byte> buffer = Bytes("$GPGGA,121252.000,3937.3032,N,11611.6046,E,1,05,2.0,45.9,M,-5.7,M,,0000*75");
            SearchResult result = p.SearchBuffer(buffer);
            Expect("readme nmea result", SearchResult.All, result);
            Expect("readme nmea raw ends", "*75", Encoding.ASCII.GetString(p.Raw).Substring(p.Raw.Length - 3));
        }

        private static void InvalidNmeaChecksumIsRejected()
        {
            NmeaLikeProtocol p = new NmeaLikeProtocol();
            List<byte> buffer = Bytes("$GPGGA,1*00");
            SearchResult result = p.SearchBuffer(buffer);
            Expect("nmea invalid result", SearchResult.None, result);
            Expect("nmea invalid raw", 0, p.Raw.Length);
            Expect("nmea invalid buffer kept", 11, buffer.Count);
        }

        private static void InvalidThenValidFrameFindsValid()
        {
            NmeaLikeProtocol p = new NmeaLikeProtocol();
            List<byte> buffer = Bytes("$GPGGA,1*00$GPGGA,1*4B");
            SearchResult result = p.SearchBuffer(buffer);
            Expect("skip-bad result", SearchResult.All, result);
            Expect("skip-bad raw", "$GPGGA,1*4B", Encoding.ASCII.GetString(p.Raw));
        }

        private static void IncompleteChecksumWaits()
        {
            NmeaLikeProtocol p = new NmeaLikeProtocol();
            List<byte> buffer = Bytes("$GPGGA,1*4");
            SearchResult result = p.SearchBuffer(buffer);
            Expect("incomplete result", SearchResult.Mask, result);
            Expect("incomplete raw", 0, p.Raw.Length);
            Expect("incomplete buffer kept", 10, buffer.Count);
        }

        private static void BinaryChecksumAfterEndOfLine()
        {
            BinaryCheckProtocol p = new BinaryCheckProtocol();
            List<byte> buffer = Bytes("^&ABC$$");
            buffer.Add(0x40); // 'A' ^ 'B' ^ 'C'
            SearchResult result = p.SearchBuffer(buffer);
            Expect("binary result", SearchResult.All, result);
            Expect("binary raw length", 8, p.Raw.Length);
            Expect("binary check byte", 0x40, p.Raw[7]);
        }

        private static void EndOfLineBeforeBeginIsIgnored()
        {
            DemoLikeProtocol p = new DemoLikeProtocol();
            List<byte> buffer = Bytes("$$^&100$$");
            SearchResult result = p.SearchBuffer(buffer);
            Expect("prefix-end result", SearchResult.All, result);
            Expect("prefix-end raw", "^&100$$", Encoding.ASCII.GetString(p.Raw));
        }

        private static List<byte> Bytes(string s)
        {
            return new List<byte>(Encoding.ASCII.GetBytes(s));
        }

        private static void Expect<T>(string name, T expected, T actual)
        {
            if (!Equals(expected, actual))
            {
                _failures++;
                Console.WriteLine("FAIL {0}: expected {1}, got {2}", name, expected, actual);
            }
        }

        private sealed class DemoLikeProtocol : TextProtocolAnalyzer<int>
        {
            public DemoLikeProtocol()
            {
                BeginOfLine = "^&";
                EndOfLine = "$$";
            }

            public override void Analyze()
            {
            }
        }

        private sealed class NmeaLikeProtocol : TextProtocolAnalyzer<int>
        {
            public NmeaLikeProtocol()
            {
                BeginOfLine = "$";
                EndOfLine = "*";
                CheckData = XorCheck;
            }

            public override void Analyze()
            {
            }
        }

        private sealed class BinaryCheckProtocol : TextProtocolAnalyzer<int>
        {
            public BinaryCheckProtocol()
            {
                BeginOfLine = "^&";
                EndOfLine = "$$";
                CheckLength = 1;
                CheckData = XorCheck;
            }

            public override void Analyze()
            {
            }
        }
    }
}
