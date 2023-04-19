//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TelnetHtmlStream – Выходной поток консоли телнет.
// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using System.Text;
    #endregion Using

    public class TelnetHtmlStream : Stream
    {
        ConsolePageBuilder _sock;
        long _length = 0;

        public TelnetHtmlStream(ConsolePageBuilder socket)
        {
            _sock = socket;
        }

        public override bool CanRead => Position <= _length;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _length;

        public override long Position { get; set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            //var data = _sock.Read();
            //int start;
            //for (start = 0; start < data.Length; start++)
            //{
            //    if (data[start] == 255/*IAC*/)
            //    {
            //        start += 2;
            //        continue;
            //    }
            //    break;
            //}
          //  data = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(data, start, data.Length - start).Replace(" ", "&nbsp;").Replace("\r\n", "<br/>"));
            var data = Encoding.UTF8.GetBytes("LETUNOVSKY SERGEY\r\n".Replace(" ", "&nbsp;").Replace("\r\n", "<br/>"));
            Array.Copy(data, buffer, data.Length);
            _length += data.Length;
            Position += data.Length;
            Task.Delay(250).Wait();
            return data.Length;
        }

        public override long Seek(long offset, SeekOrigin origin) => 0;
        public override void Write(byte[] buffer, int offset, int count) { }
        public override void Flush() { }
        public override void SetLength(long value) { }
    }
}