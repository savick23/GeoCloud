//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: LeicaTotalStationDevice – Тахеометр Leica.
// Протокол: GEOCOM Leica
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Devices
{
    #region Using
    using System;
    using RmSolution.Data;
    #endregion Using

    /// <summary> Тахеометр Leica.</summary>
    public class LeicaTotalStationDevice : IDevice, IDeviceContext, IDisposable
    {
        #region Declarations

        IDeviceConnection? _connection;

        #endregion Declarations

        #region Properties

        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Descript { get; set; }

        public GeoComAccessMode OperationMode { get; }
        public NetworkSetting? NetworkSetting { get; set; }
        public SerialPortSetting? SerialPortSetting { get; set; }

        #endregion Properties

        #region IDeviceConnection implementation

        public bool Connected => _connection?.Connected ?? false;

        public bool DataAvailable => _connection?.DataAvailable ?? false;

        public void Open()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection = null;
            }
            if (OperationMode == GeoComAccessMode.Com && SerialPortSetting != null)
                _connection = new RmSerialConnection((SerialPortSetting)SerialPortSetting);

            else if (OperationMode == GeoComAccessMode.Tcp && NetworkSetting != null)
                _connection = new RmNetworkConnection((NetworkSetting)NetworkSetting);

            _connection?.Open();
        }

        public void Close()
        {
            _connection?.Close();
            _connection = null;
        }

        public byte[] Read() => _connection?.Read() ?? Array.Empty<byte>();

        public void Write(byte[] data) => _connection?.Write(data);

        #endregion IDeviceConnection implementation

        public LeicaTotalStationDevice(string code, string name, SerialPortSetting serialPortSetting)
        {
            Code = code;
            Name = name;
            OperationMode = GeoComAccessMode.Com;
            SerialPortSetting = serialPortSetting;
        }

        public LeicaTotalStationDevice(TEquipment info, NetworkSetting networkSetting)
        {
            Code = info.Code;
            Name = info.Name;
            OperationMode = GeoComAccessMode.Tcp;
            NetworkSetting = networkSetting;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(true);
        }
    }
}
