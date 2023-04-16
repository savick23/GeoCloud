//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: LeicaTotalStationDevice – Тахеометр Leica.
// Протокол: GEOCOM Leica
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Devices
{
    #region Using
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Intrinsics.X86;
    using System.Text;
    using System.Text.RegularExpressions;
    using RmSolution.Data;
    using RmSolution.Devices.Leica;
    using static System.Runtime.InteropServices.JavaScript.JSType;
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

        #region CENTRAL SERVICES (CSV COMF)

        /// <summary> Getting the factory defined instrument number.</summary>
        /// <remarks> Gets the factory defined serial number of the instrument.</remarks>
        /// <example> mod3 dev 000001 CSV_GetInstrumentNo </example>
        public XResponse CSV_GetInstrumentNo()
        {
            var resp = Request(RequestString("%R1Q,5003:"));
            if (resp.ReturnCode == GRC.OK)
            {
                var serial = resp.Value;
            }
            return resp;
        }

        /// <summary> Getting the Leica specific instrument name.</summary>
        /// <remarks> Gets the instrument name, for example: TS30 0,5".</remarks>
        /// <example> mod3 dev 000001 CSV_GetInstrumentName </example>
        public XResponse CSV_GetInstrumentName()
        {
            var resp = Request(RequestString("%R1Q,5004:"));
            if (resp.ReturnCode == GRC.OK)
            {
                var name = resp.Text;
            }
            return resp;
        }

        /// <summary> Getting the instrument configuration.</summary>
        /// <remarks> This function returns information about the class and the configuration type of the instrument.</remarks>
        /// <example> mod3 dev 000001 CSV_GetDeviceConfig </example>
        public XResponse CSV_GetDeviceConfig()
        {
            var resp = Request(RequestString("%R1Q,5035:"));
            if (resp.ReturnCode == GRC.OK)
            {
                var devicePrecisionClass = resp.Value[0];
                var deviceConfigurationType = resp.Value[1];
            }
            return resp;
        }

        /// <summary> Getting the RL type.</summary>
        /// <remarks> This function returns information about the reflectorless and long range distance measurement (RL) of the sinstrument.</remarks>
        /// <example> mod3 dev 000001 CSV_GetReflectorlessClass </example>
        public XResponse CSV_GetReflectorlessClass()
        {
            var resp = Request(RequestString("%R1Q,5100:"));
            if (resp.ReturnCode == GRC.OK)
            {
                var reRefLessClass = resp.Value[0];
            }
            return resp;
        }

        /// <summary> Getting the date and time.</summary>
        /// <remarks> Gets the current date and time of the instrument. The ASCII response is formatted corresponding to the data type DATIME.A possible response can look like this: %R1P,0,0:0,1996,'07', '19','10','13','2f' (see chapter ASCII data type declaration for further information).</remarks>
        /// <example> mod3 dev 000001 CSV_GetDateTime </example>
        public XResponse CSV_GetDateTime()
        {
            var resp = Request(RequestString("%R1Q,5008:"));
            if (resp.ReturnCode == GRC.OK)
            {
                var dt = new DateTime((int)resp.Value[0], (int)resp.Value[1], (int)resp.Value[2], (int)resp.Value[3], (int)resp.Value[4], (int)resp.Value[5]);
            }
            return resp;
        }

        /// <summary> Getting the date and time.</summary>
        /// <remarks> Gets the current date and time of the instrument.</remarks>
        /// <example> mod3 dev 000001 CSV_GetDateTimeCentiSec </example>
        public XResponse CSV_GetDateTimeCentiSec()
        {
            var resp = Request(RequestString("%R1Q,5117:"));
            if (resp.ReturnCode == GRC.OK)
            {
                var dt = new DateTime((int)resp.Value[0], (int)resp.Value[1], (int)resp.Value[2], (int)resp.Value[3], (int)resp.Value[4], (int)resp.Value[5], (int)resp.Value[6]);
            }
            return resp;
        }

        /// <summary> Setting the date and time.</summary>
        /// <remarks> Sets the current date and time of the instrument.</remarks>
        /// <example> mod3 dev 000001 CSV_SetDateTime 2023,'03','19','10','13','2f' </example>
        public XResponse CSV_SetDateTime(DateTime datetime)
        {
            var resp = Request(RequestString("%R1Q,5007:", datetime));
            if (resp.ReturnCode == GRC.OK)
            {
            }
            return resp;
        }

        /// <summary> Getting the software version.</summary>
        /// <remarks> Returns the system software version.</remarks>
        /// <example> mod3 dev 000001 CSV_GetSWVersion </example>
        public XResponse CSV_GetSWVersion()
        {
            var resp = Request(RequestString("%R1Q,5034:"));
            if (resp.ReturnCode == GRC.OK)
            {
                var ver = new Version((int)resp.Value[0], (int)resp.Value[1], (int)resp.Value[2]);
            }
            return resp;
        }

        /// <summary> Checking the available power.</summary>
        /// <remarks> This command returns the capacity of the current power source and its source (internal or external).</remarks>
        /// <example> mod3 dev 000001 CSV_CheckPower </example>
        public XResponse CSV_CheckPower()
        {
            var resp = Request(RequestString("%R1Q,5039:"));
            if (resp.ReturnCode == GRC.OK)
            {
                var unCapacity = resp.Value[0]; // Actual capacity [%]
                var eActivePower = resp.Value[1]; // Actual power source
                var ePowerSuggest = resp.Value[2]; // Not supported
            }
            return resp;
        }

        /// <summary> Getting the instrument temperature, °C.</summary>
        /// <remarks> Get the internal temperature of the instrument, measured on the Mainboard side. Values are reported in degrees Celsius.</remarks>
        /// <example> mod3 dev 000001 CSV_GetIntTemp </example>
        public XResponse CSV_GetIntTemp()
        {
            var resp = Request(RequestString("%R1Q,5011:"));
            if (resp.ReturnCode == GRC.OK)
            {
                var temp = resp.Value[0]; // %R1P,0,0:0,11.742000000000001
            }
            return resp;
        }

        #endregion CENTRAL SERVICES (CSV COMF)

        #region ELECTRONIC DISTANCE MEASUREMENT (EDM COMF)

        /// <summary> Turning on/off the laserpointer.</summary>
        /// <remarks> Laserpointer is only available on models which support distance measurement without reflector.</remarks>
        /// <example> mod3 dev 000001 EDM_Laserpointer on/off (0/1) </example>
        public XResponse EDM_Laserpointer(ON_OFF_TYPE eOn)
        {
            var resp = Request(RequestString("%R1Q,1004:", eOn));
            if (resp.ReturnCode == GRC.OK)
            {
            }
            return resp;
        }

        /// <summary> Getting the value of the intensity of the electronic guide light.</summary>
        /// <remarks> Displays the intensity of the Electronic Guide Light.</remarks>
        /// <example> mod3 dev 000001 EDM_GetEglIntensity </example>
        public XResponse EDM_GetEglIntensity()
        {
            var resp = Request(RequestString("%R1Q,1058:"));
            if (resp.ReturnCode == GRC.OK)
            {
                var intensity = (EDM_EGLINTENSITY_TYPE)resp.Value[0];
            }
            return resp;
        }

        /// <summary> Changing the intensity of the electronic guide light.</summary>
        /// <remarks> Changes the intensity of the Electronic Guide Light.</remarks>
        /// <example> mod3 dev 000001 EDM_SetEglIntensity off/low/mid/high (0/1/2/3) </example>
        public XResponse EDM_SetEglIntensity(EDM_EGLINTENSITY_TYPE intensity)
        {
            var resp = Request(RequestString("%R1Q,1059:", intensity));
            if (resp.ReturnCode == GRC.OK)
            {
            }
            return resp;
        }

        #endregion ELECTRONIC DISTANCE MEASUREMENT (EDM COMF)

        #region Private methods

        static string ToByte(int n) => string.Concat('\'', n.ToString("x2"), '\'');

        /// <summary> Формирование ASCII строки данных для отправки на устройство.</summary>
        static byte[] RequestString(params object[] data) =>
            LF.Concat(Encoding.ASCII.GetBytes(string.Concat(data.Select(p =>
            {
                if (p.GetType().IsEnum)
                    return (int)p;

                if (p is DateTime dt)
                    return string.Join(',', dt.Year, ToByte(dt.Month), ToByte(dt.Day), ToByte(dt.Hour), ToByte(dt.Minute), ToByte(dt.Second));

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

        #region Nested types

        public class XResponse
        {
            public readonly byte[]? Data;
            public readonly string? Response;
            public readonly GRC ReturnCode = GRC.UNDEFINED;
            public readonly string? Text;
            public readonly long[] Value;

            public XResponse(byte[]? data)
            {
                if (data != null)
                {
                    Data = data;
                    Response = Encoding.ASCII.GetString(data);
                    ReturnCode = int.TryParse(Regex.Match(Response, @"(?<=:)\d+").Value, out var ret) ? (GRC)ret : GRC.UNDEFINED;

                    if (GRC_Resources.Errors.TryGetValue(ReturnCode, out var msg))
                        throw new Exception(string.Concat("[", (int)ReturnCode, "] ", msg));

                    Text = Regex.Match(Response, @"(?<=\d+,).*").Value;
                    if (Text.StartsWith('"'/*string*/))
                        Text = Text[1..^1];
                    else
                        Value = Text.Split(new char[] { ',' }).Select(n =>
                            n.StartsWith('\''/*byte*/) ? long.Parse(n[1..^1], NumberStyles.HexNumber) : long.TryParse(n, out var val) ? val : -1L).ToArray();
                }
            }

            public override string ToString() => Response ?? "(null)";
        }

        #endregion Nested types
    }
}
