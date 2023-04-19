//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TelnetStream – Выходной поток консоли телнет.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    using System.Text;
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
            var data = Encoding.UTF8.GetBytes("LETUNOVSKY");
            Array.Copy(data, buffer, data.Length);
            return data.Length;
        }
    }
}