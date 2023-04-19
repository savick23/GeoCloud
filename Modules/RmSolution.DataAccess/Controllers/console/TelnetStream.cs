//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TelnetStream – Выходной поток консоли телнет.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using System.Threading;
    #endregion Using

    public class TelnetStream : MemoryStream
    {
        ConsolePageBuilder _sock;

        public TelnetStream(ConsolePageBuilder socket)
        {
            _sock = socket;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var data = File.ReadAllBytes(@"d:\test.txt");
            Array.Copy(data, buffer, data.Length);
            return data.Length;
        }

        public override int Read(Span<byte> buffer)
        {
            return base.Read(buffer);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return base.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return base.ReadAsync(buffer, cancellationToken);
        }

        public override int ReadByte()
        {
            return base.ReadByte();
        }
    }
}