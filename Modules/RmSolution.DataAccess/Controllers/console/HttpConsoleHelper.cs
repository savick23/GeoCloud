//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: HttpConsoleHelper – Построение страницы консоли.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using System.Text;
    using System.Reflection;
    using System.Net.Sockets;
    using System.Net;
    using Microsoft.AspNetCore.DataProtection;
    using System.IO;
    #endregion Using

    public class HttpConsoleHelper
    {
        #region Constants

        static readonly Dictionary<string, byte[]> _htmlkeys = new()
        {
            { "Enter", "\u000d"u8.ToArray() },
            { "Tab", "\u0009"u8.ToArray() },
            { "Escape", "\u001b"u8.ToArray() },
            { "Backspace", "\u0008"u8.ToArray() },
            { "Delete", "\u007f"u8.ToArray() },
            { "ArrowDown", "\u001b[B"u8.ToArray() },
            { "ArrowLeft", "\u001b[D"u8.ToArray() },
            { "ArrowRight", "\u001b[C"u8.ToArray() },
            { "ArrowUp", "\u001b[A"u8.ToArray() },
            { "F1", "\u001bOP"u8.ToArray() },
            { "F2", "\u001bOQ"u8.ToArray() },
            { "F3", "\u001bOR"u8.ToArray() },
            { "F4", "\u001bOS"u8.ToArray() },
            { "F5", "\u001b[15~"u8.ToArray() },
            { "F6", "\u001b[17~"u8.ToArray() },
            { "F7", "\u001b[18~"u8.ToArray() },
            { "F8", "\u001b[19~"u8.ToArray() },
            { "F9", "\u001b[20~"u8.ToArray() },
            { "F10", "\u001b[21~"u8.ToArray() },
            { "F11", "\u001b[23~"u8.ToArray() },
            { "F12", "\u001b[24~"u8.ToArray() },
            { "F13", "\u001b[25~"u8.ToArray() },
            { "F14", "\u001b[26~"u8.ToArray() },
            { "F15", "\u001b[28~"u8.ToArray() },
            { "F16", "\u001b[29~"u8.ToArray() },
            { "F17", "\u001b[31~"u8.ToArray() },
            { "F18", "\u001b[32~"u8.ToArray() },
            { "F19", "\u001b[33~"u8.ToArray() },
            { "F20", "\u001b[34~"u8.ToArray() }
        };

        static readonly Dictionary<string, string> _appearance = new()
        {
            { "\u001b[30m", "color:black" },
            { "\u001b[30m;1m", "color:darkgrey" },
            { "\u001b[31;1m", "color:red" },
            { "\u001b[32;1m", "color:green" },
            { "\u001b[33;1m", "color:yellow" },
            { "\u001b[34;1m", "color:blue" },
            { "\u001b[35;1m", "color:magenta" },
            { "\u001b[36;1m", "color:cyan" },
            { "\u001b[37;1m", "color:white" },
            { "\u001b[37;0m", "color:whitesmoke" },
            { "\u001b[39;49m", "color:white" }
        };

        #endregion Constants

        #region Declarations

        string _host;
        int _port;
        Socket _sock;
        /// <summary> Кэш консольного вывода.</summary>
        readonly StringBuilder _output_cache = new();

        public TelnetHtmlStream? Output { get; private set; }

        #endregion Declarations

        public HttpConsoleHelper(string host, int port = 23)
        {
            _host = host;
            _port = port;
        }

        public string GetPageContent(string title)
        {
            Connect();
            return new StringBuilder("<!DOCTYPE html><html lang=\"ru\"><head><meta charset=\"utf-8\"><title>").Append(title).Append("</title><style type=\"text/css\">")
                .Append(GetResource("console.css")).Append("</style><script>")
                .Append(GetResource("console.js")).Append("</script></head><body onload=\"start()\" onkeydown=\"onKeyDown(event)\"><div id=\"console\">")
                .Append(_output_cache)
                .Append("<span id=\"cursor\">&nbsp;</span></div></body></html>").ToString();
        }

        #region Private methods

        internal static string GetResource(string name)
        {
            using var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream("RmSolution.DataAccess.Controllers.console." + name);
            using var ms = new MemoryStream();
            rs.CopyTo(ms);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        void Connect()
        {
            if (_sock == null)
            {
                _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _sock.Connect(new IPEndPoint(IPAddress.Parse(_host), _port));
                Output = new TelnetHtmlStream(this);
            }
        }

        #endregion Private methods

        #region Telnet read/write/encoding methods

        public bool Write(string input)
        {
            byte[] symb = _htmlkeys.TryGetValue(input, out byte[] bytes) ? bytes : Encoding.UTF8.GetBytes(input);
            _sock.Send(symb);
            if (input == "Enter" && _output_cache.Length > 32768)
                for (int i = 0; i < 32768; i++)
                    if (_output_cache[i] == '<' && _output_cache[++i] == 'b' && _output_cache[++i] == 'r' && _output_cache[++i] == '/' && _output_cache[++i] == '>')
                    {
                        _output_cache.Remove(0, i + 1);
                        break;
                    }

            return true;
        }

        public byte[] Read()
        {
            if (_sock.Available == 0) return Array.Empty<byte>();
            var buf = new byte[65535];
            int cnt;
            while ((cnt = _sock.Receive(buf, 0, buf.Length, SocketFlags.None)) > 0)
                if (_sock.Available == 0) break;

            var output = ToHtmlText(buf, cnt);
            _output_cache.Append(output);
            return Encoding.UTF8.GetBytes(output);
        }

        static string ToHtmlText(byte[] buffer, int count)
        {
            var res = new StringBuilder();
            bool isspan = false;
            for (int i = 0; i < count; i++)
            {
                var c = buffer[i];
                switch (c)
                {
                    case 32:
                        res.Append("&nbsp;");
                        break;

                    case 13:
                        if (buffer[i + 1] == 10)
                        {
                            res.Append("<br/>");
                            i++;
                        }
                        break;

                    case 27:
                        for (int j = i + 1; j < count; j++)
                        {
                            if (buffer[j] == 109) // m
                            {
                                if (isspan)
                                    res.Append("</span>");
                                else
                                    res.Append("<span style=\"").Append(_appearance[Encoding.UTF8.GetString(buffer[i..(j + 1)])]).Append("\">");

                                i += j - i;
                                isspan = !isspan;
                                break;
                            }
                        }
                        break;

                    case 255: /*IAC*/
                        i += 2;
                        break;

                    case 208: // UTF-8 (RU)
                    case 209: // UTF-8 (RU)
                        res.Append(Encoding.UTF8.GetString(buffer, i++, 2));
                        break;

                    case 226: // mnemo
                        res.Append(Encoding.UTF8.GetString(buffer, i, 3));
                        i += 2;
                        break;

                    default:
                        res.Append((char)c);
                        break;
                }
            }
            return res.ToString();
        }

        #endregion Telnet read/write/encoding methods
    }
}
