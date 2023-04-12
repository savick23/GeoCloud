//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: IDeviceContext –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Devices
{
    #region Using
    using System.IO.Ports;
    #endregion Using

    /// <summary> Интерфейс подключения к устройству.</summary>
    public interface IDeviceConnection
    {
        /// <summary> Состояние подключения.</summary>
        bool Connected { get; }
        /// <summary> Данные содержаться на устройстве.</summary>
        bool DataAvailable { get; }
        /// <summary> Подключиться к устройству.</summary>
        void Open();
        /// <summary> Закрыть подключение к устройству.</summary>
        void Close();
        /// <summary> Прочитать данные с устройства.</summary>
        byte[] Read();
        /// <summary> Записать данные на устройство.</summary>
        void Write(byte[] data);
    }

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
}
