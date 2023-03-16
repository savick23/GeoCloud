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
    public class LeicaTotalStationDevice : IDevice, IDeviceContext
    {
        #region Properties

        public long Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Descript { get; set; }

        public GeoComAccessMode OperationMode { get; }
        public NetworkSetting NetworkSetting { get; set; }
        public SerialPortSetting SerialPortSetting { get; set; }

        #endregion Properties

        public LeicaTotalStationDevice()
        {
        }
    }
}
