//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TcpServer –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    #region Using
    using System.Net;
    using System.Net.Sockets;
    #endregion Using

    class TcpServer : Socket
    {
        readonly IPEndPoint _endpoint;

        public TcpServer(int port)
            : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            _endpoint = new IPEndPoint(IPAddress.Any, port);
            Bind(_endpoint);
        }
    }
}
