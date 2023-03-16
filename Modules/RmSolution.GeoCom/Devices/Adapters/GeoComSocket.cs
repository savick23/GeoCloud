//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: GeoComSocket –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Devices
{
    #region Using
    using System;
    #endregion Using

    public class GeoComSocket : IDisposable
    {
        public GeoComSocket(IDeviceContext dc)
        {
            switch (dc.OperationMode)
            {
                case GeoComAccessMode.Tcp:
                    //_sock = new GeoComNetworkDriver(dc.NetworkSetting);
                    break;
                case GeoComAccessMode.Com:
                    //_sock = new GeoComSerialDriver(dc.SerialPortSetting);
                    break;
                case GeoComAccessMode.Virtual:
                    //_sock = new GeoComVirtualDriver(dc.NetworkSetting);
                    break;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(true);
        }
    }
}
