//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TelnetHtmlStream – Выходной поток консоли телнет.
// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using System;
    using System.Text;
    #endregion Using

    public class TelnetHtmlStream : Stream
    {
        #region Constants

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

        HttpConsoleHelper _sock;
        long _length = 0;

        /// <summary> Кэш консольного вывода.</summary>
        readonly StringBuilder _output_cache = new();

        #endregion Declarations

        public TelnetHtmlStream(HttpConsoleHelper socket)
        {
            _sock = socket;
        }

        #region Stream implementation

        public override bool CanRead => Position <= _length;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _length;

        public override long Position { get; set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Task.Delay(100).Wait();
            var data = _sock.Read();
            if (data.Length == 0)
            {
                buffer[0] = 0;
                return 1;
            }
            var output = ToHtmlText(data);
            _output_cache.Append(output);
            data = Encoding.UTF8.GetBytes(output);
            Array.Copy(data, buffer, data.Length);
            _length += data.Length;
            Position += data.Length;
            return data.Length;
        }

        public override void Flush()
        {
            if (_output_cache.Length > 32768)
                for (int i = 0; i < 32768; i++)
                    if (_output_cache[i] == '<' && _output_cache[++i] == 'b' && _output_cache[++i] == 'r' && _output_cache[++i] == '/' && _output_cache[++i] == '>')
                    {
                        _output_cache.Remove(0, i + 1);
                        break;
                    }
        }

        static string ToHtmlText(byte[] buffer)
        {
            var cnt = buffer.Length;
            var res = new StringBuilder();
            bool isspan = false;
            for (int i = 0; i < cnt; i++)
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
                        for (int j = i + 1; j < cnt; j++)
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

        public override string ToString() =>
            _output_cache.ToString();

        #endregion Stream implementation

        #region Unused methods

        public override long Seek(long offset, SeekOrigin origin) => 0;
        public override void Write(byte[] buffer, int offset, int count) { }
        public override void SetLength(long value) { }

        #endregion Unused methods
    }
}