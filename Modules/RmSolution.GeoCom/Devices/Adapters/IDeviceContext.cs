//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: IDeviceContext –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Devices
{
    #region Using
    using System.IO.Ports;
    #endregion Using

    public interface IDeviceContext
    {
        string Name { get; }

        GeoComAccessMode OperationMode { get; }
        NetworkSetting NetworkSetting { get; set; }
        SerialPortSetting SerialPortSetting { get; set; }
    }

    public enum GeoComAccessMode
    {
        /// <summary> COM порт.</summary>
        Com,
        /// <summary> Ethernet.</summary>
        Tcp,
        /// <summary> Виртуальный порт для тестирования.</summary>
        Virtual
    }

    public struct NetworkSetting
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }

    public struct SerialPortSetting
    {
        public string Name { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public Parity Parity { get; set; }
        public int FlowControl { get; set; }
        public string Interface { get; set; }
        public bool Fifo { get; set; }

        public override string ToString()
        {
            return $"Baud rate:{BaudRate}; Stop bits:{StopBits}";
        }
    }
}
