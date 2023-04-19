//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TelnetStream – Выходной поток консоли телнет.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using System.Text;
    #endregion Using

    public class TelnetStream : Stream
    {
        ConsolePageBuilder _sock;
        long _length = 0;

        public TelnetStream(ConsolePageBuilder socket)
        {
            _sock = socket;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _length;

        public override long Position { get; set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var data = Encoding.UTF8.GetBytes("LETUNOVSKY");
            Array.Copy(data, buffer, data.Length);
            _length += data.Length;
            Position += data.Length;
            return data.Length;
        }

        public override long Seek(long offset, SeekOrigin origin) => 0;
        public override void Write(byte[] buffer, int offset, int count) { }
        public override void Flush() { }
        public override void SetLength(long value) { }
    }
}