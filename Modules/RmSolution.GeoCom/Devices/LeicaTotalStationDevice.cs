//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: LeicaTotalStationDevice – Тахеометр Leica.
// Протокол: GEOCOM Leica
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Devices
{
    #region Using
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using RmSolution.Data;
    using RmSolution.Devices.Leica;
    #endregion Using

    /// <summary> Тахеометр Leica.</summary>
    public class LeicaTotalStationDevice : IDevice, IDeviceContext, IDisposable
    {
        #region Declarations

        /// <summary> При посылке в начале строки (не обязательно), отчищает входной буфер команд на устройстве.</summary>
        readonly static byte[] LF = new byte[] { 0x0a };
        readonly static byte[] TERM = "\r\n"u8.ToArray();

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

        #region Leica functions (COMF).

        /// <summary> Turning on/off the laserpointer.</summary>
        /// <remarks> Laserpointer is only available on models which support distance measurement without reflector.</remarks>
        /// <example> mod3 dev 000001 EDM_Laserpointer on </example>
        public byte[]? EDM_Laserpointer(ON_OFF_TYPE eOn)
        {
            var resp = Request(RequestString("%R1Q,1004:", eOn));
            if (resp.ReturnCode == GRC.OK)
            {
            }
            return resp.Data;
        }

        /// <summary> Getting the value of the intensity of the electronic guide light.</summary>
        /// <remarks> Displays the intensity of the Electronic Guide Light.</remarks>
        /// <example> mod3 dev 000001 EDM_GetEglIntensity </example>
        public byte[]? EDM_GetEglIntensity()
        {
            var resp = Request(RequestString("%R1Q,1058:"));
            if (resp.ReturnCode == GRC.OK)
            {
                var intensity = (EDM_EGLINTENSITY_TYPE)resp.Value;
            }
            return resp.Data;
        }

        #endregion Leica functions (COMF).

        #region Private methods

        /// <summary> Формирование ASCII строки данных для отправки на устройство.</summary>
        static byte[] RequestString(params object[] data) =>
            LF.Concat(Encoding.ASCII.GetBytes(string.Concat(data.Select(p =>
            {
                if (p.GetType().IsEnum)
                    return (int)p;

                return p;
            }
            )))).Concat(TERM).ToArray();

        XResponse Request(byte[] data)
        {
            byte[]? resp;
            try
            {
                Open();
                Write(data);
                int attempt = 120;
                do
                {
                    Task.Delay(250).Wait();
                    resp = Read();
                }
                while ((resp == null || resp.Length == 0) && attempt-- > 0);
            }
            finally
            {
                Close();
            }
            return new XResponse(resp);
        }

        #endregion Private methods

        class XResponse
        {
            public readonly byte[]? Data;
            public readonly string? Response;
            public readonly GRC ReturnCode = GRC.UNDEFINED;
            public readonly long Value;

            public XResponse(byte[]? data)
            {
                if (data != null)
                {
                    Data = data;
                    Response = Encoding.ASCII.GetString(data);
                    ReturnCode = int.TryParse(Regex.Match(Response, @"(?<=:)\d+").Value, out var ret) ? (GRC)ret : GRC.UNDEFINED;

                    if (GRC_Resources.Errors.TryGetValue(ReturnCode, out var msg))
                        throw new Exception(string.Concat("[", (int)ReturnCode, "] ", msg));

                    Value = long.TryParse(Regex.Match(Response, @"(?<=,)\d+$").Value, out var val) ? val : -1;
                }
            }
        }
    }
}
