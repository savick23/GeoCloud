//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: RmNetworkConnection - Класс TCP клиента.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Devices
{
    #region Using
    using System;
    using System.Net;
    using System.Net.Sockets;
    #endregion Using

    /// <summary> TCP-клиент.</summary>
    public class RmNetworkConnection : Socket, IDeviceConnection
    {
        public IPEndPoint EndPoint { get; }

        public bool DataAvailable => Available > 0;

        public int Timeout { get; set; } = 10000;

        public RmNetworkConnection(string host, int port)
            : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            EndPoint = new IPEndPoint(IPAddress.Parse(host), port);
        }

        public RmNetworkConnection(NetworkSetting sets)
            : base(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            EndPoint = new IPEndPoint(IPAddress.Parse(sets.Host), sets.Port);
        }

        public void Open()
        {
            Connect(EndPoint);
        }

        public byte[] Read()
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

        public void Write(byte[] data)
        {
            Send(data);
        }
    }
}
