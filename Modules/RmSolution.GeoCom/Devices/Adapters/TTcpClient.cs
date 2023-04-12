//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TTcpClient - Класс TCP клиента.
//--------------------------------------------------------------------------------------------------
namespace SmartMinex.Runtime
{
    #region Using
    using System;
    using System.Net;
    using System.Net.Sockets;
    #endregion Using

    /// <summary> TCP-клиент.</summary>
    public class TTcpClient : Socket
    {
        public IPEndPoint EndPoint { get; }

        public TTcpClient(string host, int port)
            : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            EndPoint = new IPEndPoint(IPAddress.Parse(host), port);
        }

        public void Connect()
        {
            Connect(EndPoint);
        }

        public byte[] Receive()
        {
            var buf = new byte[64];
            int cnt;
            if (Available == 0) return new byte[0];

            while ((cnt = Receive(buf, buf.Length, SocketFlags.None)) > 0)
                if (Available == 0) break;

            if (cnt == 0) return new byte[0];

            var res = new byte[cnt];
            Array.Copy(buf, res, cnt);
            return res;
        }

        public static bool Receive(Socket sock, out byte[] data)
        {
            if (sock.Available == 0)
            {
                data = null;
                return false;
            }
            var buf = new byte[64];
            int cnt;
            while ((cnt = sock.Receive(buf, buf.Length, SocketFlags.None)) > 0)
                if (sock.Available == 0) break;

            if (cnt == 0)
            {
                data = null;
                return false;
            }
            data = new byte[cnt];
            Array.Copy(buf, data, cnt);
            return true;
        }
    }
}
