//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: LeicaTotalStationDevice – Тахеометр Leica.
// Документация: Leica TPS1200+ Leica TS30/TM30 GeoCOM Reference Manual Version 1.50 
// Протокол: GEOCOM Leica
// Вызов функции прибора с консоли: mod3 call 000001 COM_GetSWVersion
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Devices
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using RmSolution.Data;
    using RmSolution.Devices.Leica;
    #endregion Using

    /// <summary> Тахеометр Leica.</summary>
    public partial class LeicaTotalStationDevice : IDevice, IDeviceContext, IDisposable
    {
        #region Declarations

        /// <summary> При посылке в начале строки (не обязательно), отчищает входной буфер команд на устройстве.</summary>
        readonly static byte[] LF = new byte[] { 0x0a };
        readonly static byte[] TERM = "\r\n"u8.ToArray();

        readonly object _lockRoot = new();

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

        public int Timeout { get; set; } = 30000;

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
                { "Подключение", OperationMode },
                { "Адрес",
                    OperationMode == GeoComAccessMode.Tcp ? NetworkSetting.HasValue ? NetworkSetting.Value.Host + ":" + NetworkSetting.Value.Port : "(null)" :
                    OperationMode == GeoComAccessMode.Com ? SerialPortSetting.HasValue ? SerialPortSetting.Value.PornName : "(null)" : "<нет подключения>" },
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

        #region Private methods
#pragma warning disable CS8604

        /// <summary> Вызов функции на устройстве с вовзратом единственного параметра.</summary>
        T? CallGet<T>(string command, params object[] parameters) =>
            Call(command, parameters, (resp) => Successful(resp.ReturnCode) && resp.Values.Length == 1 ? (T)resp.Values[0] : default);

        /// <summary> Вызов функции на устройстве с проверкой успешности выполнения и генерацией исключения в случае ошибки выполнения.</summary>
        bool CallSet(string command, params object[] parameters) =>
            Call(command, parameters, (resp) => Successful(resp.ReturnCode));

        /// <summary> Вызов функции на устройстве с проверкой успешности выполнения и генерацией исключения в случае ошибки выполнения.</summary>
        T Call<T>(string command, Func<ZResponse, T> hander) =>
            hander.Invoke(Request(RequestString(command)));

        /// <summary> Вызов функции на устройстве с проверкой успешности выполнения и генерацией исключения в случае ошибки выполнения.</summary>
        T Call<T>(string command, object p1, Func<ZResponse, T> hander) =>
            hander.Invoke(Request(RequestString(command, p1)));

        /// <summary> Вызов функции на устройстве с проверкой успешности выполнения и генерацией исключения в случае ошибки выполнения.</summary>
        T Call<T>(string command, object p1, object p2, Func<ZResponse, T> hander) =>
            hander.Invoke(Request(RequestString(command, p1, p2)));

        /// <summary> Вызов функции на устройстве с проверкой успешности выполнения и генерацией исключения в случае ошибки выполнения.</summary>
        T Call<T>(string command, object p1, object p2, object p3, Func<ZResponse, T> hander) =>
            hander.Invoke(Request(RequestString(command, p1, p2, p3)));

        /// <summary> Вызов функции на устройстве с проверкой успешности выполнения и генерацией исключения в случае ошибки выполнения.</summary>
        T Call<T>(string command, object p1, object p2, object p3, object p4, Func<ZResponse, T> hander) =>
            hander.Invoke(Request(RequestString(command, p1, p2, p3, p4)));

        /// <summary> Вызов функции на устройстве с проверкой успешности выполнения и генерацией исключения в случае ошибки выполнения.</summary>
        T Call<T>(string command, object p1, object p2, object p3, object p4, object p5, Func<ZResponse, T> hander) =>
            hander.Invoke(Request(RequestString(command, p1, p2, p3, p4, p5)));

        /// <summary> Вызов функции на устройстве с проверкой успешности выполнения и генерацией исключения в случае ошибки выполнения.</summary>
        T Call<T>(string command, object p1, object p2, object p3, object p4, object p5, object p6, Func<ZResponse, T> hander) =>
            hander.Invoke(Request(RequestString(command, p1, p2, p3, p4, p5, p6)));

        /// <summary> Возвращает истину в случа успешного выполнения или общие исключения.</summary>
        static bool Successful(GRC returnCode) =>
            (returnCode) switch {
                GRC.OK => true,
                GRC.NA => throw new LeicaException(returnCode, "GeoCOM Robotic license key not available."),
                _ => throw new LeicaException(returnCode, typeof(GRC).GetField(returnCode.ToString()).GetCustomAttribute<DescriptionAttribute>()?.Description ?? "<нет описания>")
            };

        static string ToByte(int n) => string.Concat('\'', n.ToString("x2"), '\'');

        /// <summary> Формирование ASCII строки данных для отправки на устройство.</summary>
        static byte[] RequestString(string command, params object[] parameters) =>
            LF.Concat(Encoding.ASCII.GetBytes(command + string.Join(',', parameters.Where(p => p != null).Select(p =>
            {
                if (p.GetType().IsEnum)
                    return (long)p; // Типы прибора Leica x64

                if (p is DateTime dt)
                    return string.Join(',', dt.Year, ToByte(dt.Month), ToByte(dt.Day), ToByte(dt.Hour), ToByte(dt.Minute), ToByte(dt.Second));

                if (p is string)
                    return string.Concat('"', p, '"');

                if (p is float pf)
                    return pf.ToString(CultureInfo.InvariantCulture);

                if (p is double pd)
                    return pd.ToString(CultureInfo.InvariantCulture);

                if (p is bool pb)
                    return pb ? "1" : "0";

                return p;
            }
            )))).Concat(TERM).ToArray();

        ZResponse Request(byte[] data)
        {
            byte[]? resp;
            TimeSpan executed;
            lock (_lockRoot)
                try
                {
                    Open();
                    var start = DateTime.Now;
                    Write(data);
                    int attempt = Timeout / 250;
                    do
                    {
                        Task.Delay(250).Wait();
                        resp = Read();
                        executed = DateTime.Now - start;
                    }
                    while ((resp == null || resp.Length == 0) && --attempt > 0);

                    if (resp == null || resp.Length == 0)
                        throw new CommunicationException("Не удалось подключиться к прибору. Истекло время " + Timeout + " сек, ожидание ответа от прибора.");
                }
                catch (Exception ex)
                {
                    throw new CommunicationException("Не удалось подключиться к прибору. " + ex.Message);
                }
                finally
                {
                    Close();
                }
            return new ZResponse(resp, executed);
        }

#pragma warning restore CS8604
        #endregion Private methods

        #region Nested types

        public partial class ZResponse
        {
            public readonly byte[]? Data;
            public readonly string? Response;
            public readonly GRC ReturnCode = GRC.UNDEFINED;
            public readonly object[] Values;
            /// <summary> Время выполнения комнады, мс.</summary>
            public TimeSpan? Executed;

            [GeneratedRegex("(?<=:)\\d+")]
            private static partial Regex getReturnCode();
            [GeneratedRegex("(?<=:\\d+,).*")]
            private static partial Regex getReturn();

            public ZResponse(byte[]? data, TimeSpan executed)
            {
                if (data != null)
                {
                    Data = data;
                    Response = Encoding.ASCII.GetString(data);
                    ReturnCode = int.TryParse(getReturnCode().Match(Response).Value, out var ret) ? (GRC)ret : GRC.UNDEFINED;

                    if (GRC_Resources.Errors.TryGetValue(ReturnCode, out var msg))
                        throw new Exception(string.Concat("[", (int)ReturnCode, "] ", msg));

                    Values = getReturn().Match(Response).Value.Split(new char[] { ',' })
                        .Select(ret =>
                            ret.StartsWith('"'/*string*/) ? (object)ret[1..^1] :
                            ret.StartsWith('\''/*byte*/) ? long.Parse(ret[1..^1], NumberStyles.HexNumber) :
                            ret.Contains('.') ? double.Parse(ret, CultureInfo.InvariantCulture) :
                            long.TryParse(ret, out var val) ? val : -1L
                        ).ToArray();

                    Executed = executed;
                }
            }

            public override string ToString() => Response ?? "(null)";
        }

        #endregion Nested types
    }
}
