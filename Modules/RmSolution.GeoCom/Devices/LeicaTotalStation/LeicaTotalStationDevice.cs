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

    public class COMFAttribute : Attribute { }

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

        /// <summary> Возвращает сведения об устройстве.</summary>
        public Dictionary<string, object?> ReadConfig()
        {
            var cfg = CSV_GetDeviceConfig();
            return new Dictionary<string, object?>()
            {
                { "Дата/Время прибора", CSV_GetDateTime() },
                { "Наименование", CSV_GetInstrumentName() },
                { "Заводской номер", CSV_GetInstrumentNo() },
                { "Device precision class", cfg[0] },
                { "Device configuration type", cfg[1] },
                { "RL type", CSV_GetReflectorlessClass() },
                { "Версия ПО", CSV_GetSWVersion() },
                { "Питание", CSV_CheckPower() },
                { "Температура прибора, °C", CSV_GetIntTemp() }
            };
        }

        #region COMMUNICATIONS (COM COMF)

        /// <summary> Retrieving server instrument version.</summary>
        /// <remarks> This function displays the current GeoCOM release (release, version and subversion) of the instrument.</remarks>
        /// <example> mod3 dev 000001 COM_GetSWVersion </example>
        public ZResponse COM_GetSWVersion()
        {
            var resp = Request(RequestString("%R1Q,110:"));
            if (resp.ReturnCode == GRC.OK)
            {
                var ver = new Version((int)resp.Value[0], (int)resp.Value[1], (int)resp.Value[2]);
            }
            return resp;
        }

        /// <summary> Turning on the instrument.</summary>
        /// <remarks> This function switches on the TPS1200 instrument.<br/><b>Note</b>: The TPS1200 instrument can be switched on by any RPC command or even by sending a single character.</remarks>
        /// <returns> If instrument is already switched on then %R1P,0,0:5 else Nothing </returns>
        /// <param name="onMode"> Run mode.</param>
        /// <example> mod3 dev 000001 COM_SwitchOnTPS 0/1 </example>
        public ZResponse COM_SwitchOnTPS(COM_TPS_STARTUP_MODE onMode)
        {
            var resp = Request(RequestString("%R1Q,111:", onMode));
            if (resp.ReturnCode == GRC.OK)
            {
            }
            return resp;
        }

        /// <summary> Turning on the instrument.</summary>
        /// <remarks> This function switches off the TPS1200 instrument.</remarks>
        /// <param name="offMode"> Stop mode.</param>
        /// <example> mod3 dev 000001 COM_SwitchOffTPS 0/1 </example>
        public ZResponse COM_SwitchOffTPS(COM_TPS_STOP_MODE offMode)
        {
            var resp = Request(RequestString("%R1Q,112:", offMode));
            if (resp.ReturnCode == GRC.OK)
            {
            }
            return resp;
        }

        /// <summary> Checking the communication.</summary>
        /// <remarks> This function does not provide any functionality except of checking if the communication is up and running.</remarks>
        /// <example> mod3 dev 000001 COM_NullProc </example>
        public ZResponse COM_NullProc()
        {
            var resp = Request(RequestString("%R1Q,0:"));
            if (resp.ReturnCode == GRC.OK)
            {
            }
            return resp;
        }

        /// <summary> Getting the binary attribute of the server.</summary>
        /// <remarks> This function gets the ability information about the server to handle binary communication. The client may make requests in binary format which speeds up the communication by about 40-50%.</remarks>
        /// <example> mod3 dev 000001 COM_GetBinaryAvailable </example>
        public ZResponse COM_GetBinaryAvailable()
        {
            var resp = Request(RequestString("%R1Q,113:"));
            if (resp.ReturnCode == GRC.OK)
            {
                var binAvailable = resp.Value[0] == 0; // binary operation enabled / ASCII operation enabled
            }
            return resp;
        }

        #endregion COMMUNICATIONS (COM COMF)

        #region CENTRAL SERVICES (CSV COMF)

        /// <summary> Getting the factory defined instrument number.</summary>
        /// <remarks> Gets the factory defined serial number of the instrument.</remarks>
        [COMF]
        public string? CSV_GetInstrumentNo()
        {
            var resp = Request(RequestString("%R1Q,5003:"));
            if (resp.ReturnCode == GRC.OK)
                return resp.Value[0].ToString();

            return null;
        }

        /// <summary> Getting the Leica specific instrument name.</summary>
        /// <remarks> Gets the instrument name, for example: TS30 0,5".</remarks>
        [COMF]
        public string? CSV_GetInstrumentName()
        {
            var resp = Request(RequestString("%R1Q,5004:"));
            if (resp.ReturnCode == GRC.OK)
                return resp.Text?.ToString() ?? string.Empty;

            return null;
        }

        /// <summary> Getting the instrument configuration.</summary>
        /// <remarks> This function returns information about the class and the configuration type of the instrument.</remarks>
        [COMF]
        public long[]? CSV_GetDeviceConfig()
        {
            var resp = Request(RequestString("%R1Q,5035:"));
            if (resp.ReturnCode == GRC.OK)
                return new long[] { resp.Value[0], resp.Value[1] };

            return null;
        }

        /// <summary> Getting the RL type.</summary>
        /// <remarks> This function returns information about the reflectorless and long range distance measurement (RL) of the sinstrument.</remarks>
        [COMF]
        public long? CSV_GetReflectorlessClass()
        {
            var resp = Request(RequestString("%R1Q,5100:"));
            if (resp.ReturnCode == GRC.OK)
                return resp.Value[0];

            return null;
        }

        /// <summary> Getting the date and time.</summary>
        /// <remarks> Gets the current date and time of the instrument. The ASCII response is formatted corresponding to the data type DATIME.A possible response can look like this: %R1P,0,0:0,1996,'07', '19','10','13','2f' (see chapter ASCII data type declaration for further information).</remarks>
        [COMF]
        public DateTime? CSV_GetDateTime()
        {
            var resp = Request(RequestString("%R1Q,5008:"));
            if (resp.ReturnCode == GRC.OK)
                return new DateTime((int)resp.Value[0], (int)resp.Value[1], (int)resp.Value[2], (int)resp.Value[3], (int)resp.Value[4], (int)resp.Value[5]);

            return null;
        }

        /// <summary> Getting the date and time.</summary>
        /// <remarks> Gets the current date and time of the instrument.</remarks>
        [COMF]
        public DateTime? CSV_GetDateTimeCentiSec()
        {
            var resp = Request(RequestString("%R1Q,5117:"));
            if (resp.ReturnCode == GRC.OK)
                return new DateTime((int)resp.Value[0], (int)resp.Value[1], (int)resp.Value[2], (int)resp.Value[3], (int)resp.Value[4], (int)resp.Value[5], (int)resp.Value[6]);

            return null;
        }

        /// <summary> Setting the date and time.</summary>
        /// <remarks> Sets the current date and time of the instrument.</remarks>
        [COMF]
        public bool CSV_SetDateTime(DateTime datetime) =>
            Request(RequestString("%R1Q,5007:", datetime)).ReturnCode == GRC.OK;

        /// <summary> Getting the software version.</summary>
        /// <remarks> Returns the system software version.</remarks>
        [COMF]
        public Version? CSV_GetSWVersion()
        {
            var resp = Request(RequestString("%R1Q,5034:"));
            if (resp.ReturnCode == GRC.OK)
                return new Version((int)resp.Value[0], (int)resp.Value[1], (int)resp.Value[2]);

            return null;
        }

        /// <summary> Checking the available power.</summary>
        /// <remarks> This command returns the capacity of the current power source and its source (internal or external).</remarks>
        [COMF]
        public string? CSV_CheckPower()
        {
            var resp = Request(RequestString("%R1Q,5039:"));
            if (resp.ReturnCode == GRC.OK || resp.ReturnCode == GRC.LOW_POWER || resp.ReturnCode == GRC.BATT_EMPTY)
            {
                var capacity = resp.Value[0];
                var powerSource = resp.Value[1] == 0 ? "От сети" : "От батареи";
                var powerSuggest = resp.Value[2]; // Not supported

                return string.Concat(powerSource, ", ", capacity, "%",
                    resp.ReturnCode == GRC.LOW_POWER ? ", Низкий разряд аккумулятора. Время работы около 30 мин."
                    : resp.ReturnCode == GRC.BATT_EMPTY ? ", Аккумулятор почти разряжен. Время работы около 1 мин." : string.Empty);
            }
            return null;
        }

        /// <summary> Getting the instrument temperature, °C.</summary>
        /// <remarks> Get the internal temperature of the instrument, measured on the Mainboard side. Values are reported in degrees Celsius.</remarks>
        [COMF]
        public double? CSV_GetIntTemp()
        {
            var resp = Request(RequestString("%R1Q,5011:"));
            if (resp.ReturnCode == GRC.OK)
                return Math.Round(double.Parse(resp.Text, CultureInfo.InvariantCulture), 1);

            return null;
        }

        #endregion CENTRAL SERVICES (CSV COMF)

        #region ELECTRONIC DISTANCE MEASUREMENT (EDM COMF)

        /// <summary> Turning on/off the laserpointer.</summary>
        /// <remarks> Laserpointer is only available on models which support distance measurement without reflector.</remarks>
        /// <example> mod3 dev 000001 EDM_Laserpointer on/off (0/1) </example>
        public ZResponse EDM_Laserpointer(ON_OFF_TYPE eOn)
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
        public ZResponse EDM_GetEglIntensity()
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
        public ZResponse EDM_SetEglIntensity(EDM_EGLINTENSITY_TYPE intensity)
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

        ZResponse Request(byte[] data)
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
            return new ZResponse(resp);
        }

        #endregion Private methods

        #region Nested types

        public class ZResponse
        {
            public readonly byte[]? Data;
            public readonly string? Response;
            public readonly GRC ReturnCode = GRC.UNDEFINED;
            public readonly string? Text;
            public readonly long[] Value;

            public ZResponse(byte[]? data)
            {
                if (data != null)
                {
                    Data = data;
                    Response = Encoding.ASCII.GetString(data);
                    ReturnCode = int.TryParse(Regex.Match(Response, @"(?<=:)\d+").Value, out var ret) ? (GRC)ret : GRC.UNDEFINED;

                    if (GRC_Resources.Errors.TryGetValue(ReturnCode, out var msg))
                        throw new Exception(string.Concat("[", (int)ReturnCode, "] ", msg));

                    Text = Regex.Match(Response, @"(?<=:\d+,).*").Value;
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
