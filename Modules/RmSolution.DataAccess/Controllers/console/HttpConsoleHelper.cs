//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: HttpConsoleHelper – Построение страницы консоли.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using System.Text;
    using System.Data;
    using System.Reflection;
    using System.Net.Sockets;
    using Microsoft.Extensions.Hosting;
    using System.Net;
    #endregion Using

    public class HttpConsoleHelper
    {
        #region Declarations

        static readonly Dictionary<string, byte[]> _htmlkeys = new Dictionary<string, byte[]>()
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

        Socket _sock;

        #endregion Declarations

        public string GetPageContent()
        {
            Connect();
            var page = new StringBuilder("<!DOCTYPE html><html lang=\"ru\"><head><meta charset=\"utf-8\"><title>РМ ГЕО 3.1 - Консоль</title><style type=\"text/css\">")
                .Append(GetResource("console.console.css")).Append("</style><script>")
                .Append(GetResource("console.console.js")).Append("</script></head><body onload=\"start()\" onkeydown=\"onKeyDown(event)\"><div id=\"console\">");

            return page.Append("<span id=\"cursor\">&nbsp;</span></div></body></html>").ToString();
        }

        #region Private methods

        internal static string GetResource(string name)
        {
            using var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream("RmSolution.DataAccess.Controllers." + name);
            using var ms = new MemoryStream();
            rs.CopyTo(ms);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        void Connect()
        {
            if (_sock == null)
            {
                _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _sock.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 23));
            }
        }

        #endregion Private methods

        public bool Write(string input)
        {
            byte[] symb = _htmlkeys.TryGetValue(input, out byte[] bytes) ? bytes : Encoding.UTF8.GetBytes(input);
            _sock.Send(symb);
            return true;
        }

        public byte[] Read()
        {
            if (_sock.Available == 0) return Array.Empty<byte>();
            var buf = new byte[65535];
            int cnt;
            while ((cnt = _sock.Receive(buf, 0, buf.Length, SocketFlags.None)) > 0)
                if (_sock.Available == 0) break;

            if (cnt == 0) return Array.Empty<byte>();

            var resp = new byte[cnt];
            Array.Copy(buf, resp, cnt);
            int start;
            for (start = 0; start < resp.Length; start++)
            {
                if (resp[start] == 255/*IAC*/)
                {
                    start += 2;
                    continue;
                }
                break;
            }
            return resp;
        }
    }
}
