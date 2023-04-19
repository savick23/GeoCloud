//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: ConsolePageBuilder – Построение страницы консоли.
// https://learn.javascript.ru/range-textrange-selection
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

    public class ConsolePageBuilder
    {
        Socket _sock;

        public string Build()
        {
            var _page = new StringBuilder("<!DOCTYPE html><html lang=\"ru\"><head><meta charset=\"utf-8\"><title>РМ ГЕО 3.1 - Консоль</title><style type=\"text/css\">")
                .Append(GetResource("console.console.css")).Append("</style><script>")
                .Append(GetResource("console.console.js")).Append("</script></head><body onkeydown=\"onKeyDown(event)\"><div id=\"console\">");

            _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _sock.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 23));
            var lines = ReadLines(Array.Empty<byte>());

            lines[lines.Length - 1] += "<div id=\"input\"><span id=\"cursor\">&nbsp;</span></div>";
            _page.Append(string.Concat(lines.Select(r => string.Concat("<div>", r, "</div>"))));

            _page.Append("</div></body></html>");
            return _page.ToString();
        }

        #region Private methods

        internal static string GetResource(string name)
        {
            using var rs = Assembly.GetExecutingAssembly().GetManifestResourceStream("RmSolution.DataAccess.Controllers." + name);
            using var ms = new MemoryStream();
            rs.CopyTo(ms);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        #endregion Private methods

        public string[] ReadLines(byte[] data)
        {
            _sock.Send(data);
            Task.Delay(250).Wait();
            var buf = new byte[1024];
            int cnt;
            if (_sock.Available == 0) return Array.Empty<string>();

            while ((cnt = _sock.Receive(buf, 0, buf.Length, SocketFlags.None)) > 0)
                if (_sock.Available == 0) break;

            if (cnt == 0) return Array.Empty<string>();

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
            return Encoding.UTF8.GetString(resp, start, resp.Length - start).Replace(" ", "&nbsp;").Split(new string[] { "\r\n" }, StringSplitOptions.None);
        }
    }
}
