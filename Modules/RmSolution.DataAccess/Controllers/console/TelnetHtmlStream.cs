//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TelnetHtmlStream – Выходной поток консоли телнет.
// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using System;
    #endregion Using

    public class TelnetHtmlStream : Stream
    {
        #region Declarations

        HttpConsoleHelper _sock;
        long _length = 0;

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
            var data = _sock.Read();
            if (data.Length == 0)
            {
                buffer[0] = 0;
                Task.Delay(100).Wait();
                return 1;
            }
            Array.Copy(data, buffer, data.Length);
            _length += data.Length;
            Position += data.Length;
            Task.Delay(100).Wait();
            return data.Length;
        }

        #endregion Stream implementation

        #region Unused methods

        public override long Seek(long offset, SeekOrigin origin) => 0;
        public override void Write(byte[] buffer, int offset, int count) { }
        public override void Flush() { }
        public override void SetLength(long value) { }

        #endregion Unused methods
    }
}