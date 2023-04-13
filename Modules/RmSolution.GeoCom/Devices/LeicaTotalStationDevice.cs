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
        #region Properties

        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Descript { get; set; }

        public GeoComAccessMode OperationMode { get; }
        public IDeviceConnection Connection { get; }
        public NetworkSetting? NetworkSetting { get; set; }
        public SerialPortSetting? SerialPortSetting { get; set; }

        #endregion Properties

        public LeicaTotalStationDevice(string code, string name, SerialPortSetting serialPortSetting)
        {
            Code = code;
            Name = name;
            OperationMode = GeoComAccessMode.Com;
            SerialPortSetting = serialPortSetting;
            Connection = new RmSerialConnection(serialPortSetting);
        }

        public LeicaTotalStationDevice(TEquipment info, NetworkSetting networkSetting)
        {
            Code = info.Code;
            Name = info.Name;
            OperationMode = GeoComAccessMode.Tcp;
            NetworkSetting = networkSetting;
            Connection = new RmNetworkConnection(networkSetting);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(true);
        }
    }
}
