﻿//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: LeicaTotalStationDevice – Тахеометр Leica.
// Протокол: GEOCOM Leica
// Вызов функции прибора с консоли: mod3 call 000001 COM_GetSWVersion
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Devices
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Intrinsics.Arm;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Channels;
    using RmSolution.Data;
    using RmSolution.Devices.Leica;
    #endregion Using

    public class COMFAttribute : Attribute { }

    /// <summary> Тахеометр Leica.</summary>
    public partial class LeicaTotalStationDevice : IDevice, IDeviceContext, IDisposable
    {
        #region Declarations

        /// <summary> Standard intensity of beep expressed as a percentage.</summary>
        const short IOS_BEEP_STDINTENS = 100;
        /// <summary> Prism name length.</summary>
        const int BAP_PRISMNAME_LEN = 16;

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

        #region BASIC APPLICATIONS (BAP CONF)

        /// <summary> Getting the EDM type.</summary>
        /// <remarks> Gets the current EDM type for distance measurements (Reflector (IR) or Reflectorless (RL)).</remarks>
        /// <returns> Actual target type.</returns>
        /// <example> mod3 call 000001 BAP_GetTargetType </example>
        [COMF]
        public BAP_TARGET_TYPE? BAP_GetTargetType()
        {
            var resp = Request(RequestString("%R1Q,17022:"));
            if (resp.ReturnCode == GRC.OK)
                return (BAP_TARGET_TYPE)resp.Values[0];

            return null;
        }

        /// <summary> Setting the EDM type.</summary>
        /// <remarks> Sets the current EDM type for distance measurements (Reflector (IR) or Reflectorless (RL)).<br/>For each EDM type the last used EDM mode is remembered and activated if the EDM type is changed.<br/>If EDM type IR is selected the last used Automation mode is automatically activated.<br/>BAP_SetMeasPrg can also change the target type.<br/>EDM type RL is not available on all instrument types.</remarks>
        /// <param name="targetType"> Target type </param>
        /// <example> mod3 call 000001 BAP_SetTargetType </example>
        [COMF]
        public bool BAP_SetTargetType(BAP_TARGET_TYPE targetType)
        {
            var resp = Request(RequestString("%R1Q,17021:", targetType));
            return resp.ReturnCode == GRC.OK;
        }

        /// <summary> Getting the default prism type.</summary>
        /// <remarks> Gets the current prism type.</remarks>
        /// <returns> Actual prism type.</returns>
        /// <example> mod3 call 000001 BAP_GetPrismType </example>
        [COMF]
        public BAP_PRISMTYPE? BAP_GetPrismType()
        {
            var resp = Request(RequestString("%R1Q,17009:"));
            if (resp.ReturnCode == GRC.OK)
                return (BAP_PRISMTYPE)resp.Values[0];

            return null;
        }

        /// <summary> Setting the default prism type.</summary>
        /// <remarks> Sets the prism type for measurements with a reflector. It overwrites the prism constant, set by TMC_SetPrismCorr.</remarks>
        /// <param name="prismType"> Prism type </param>
        /// <example> mod3 call 000001 BAP_SetPrismType </example>
        [COMF]
        public bool BAP_SetPrismType(BAP_PRISMTYPE prismType)
        {
            var resp = Request(RequestString("%R1Q,17008:", prismType));
            return resp.ReturnCode == GRC.OK;
        }

        /// <summary> Getting the default or user prism type.</summary>
        /// <remarks> Gets the current prism type and name.</remarks>
        /// <example> mod3 call 000001 BAP_GetPrismType2 </example>
        [COMF]
        public KeyValuePair<BAP_PRISMTYPE, string>? BAP_GetPrismType2()
        {
            var resp = Request(RequestString("%R1Q,17031:"));
            if (resp.ReturnCode == GRC.OK && resp.Values.Length == 2)
                return new KeyValuePair<BAP_PRISMTYPE, string>((BAP_PRISMTYPE)resp.Values[0], resp.Values[1].ToString());

            return null;
        }

        /// <summary> Setting the default prism type.</summary>
        /// <remarks> Sets the prism type for measurements with a reflector. It overwrites the prism constant, set by TMC_SetPrismCorr.</remarks>
        /// <param name="prismType"> Prism type </param>
        /// <param name="prismName"> Prism name. Required if prism type is BAP_PRISM_USER.</param>
        /// <example> mod3 call 000001 BAP_SetPrismType2 </example>
        [COMF]
        public bool BAP_SetPrismType2(BAP_PRISMTYPE prismType, string prismName)
        {
            var resp = Request(RequestString("%R1Q,17030:", prismType, prismName));
            return (resp.ReturnCode) switch
            {
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Prism type is not available, i.e. a user prism is not defined."),
                _ => resp.ReturnCode == GRC.OK
            };
        }

        /// <summary> Getting the default prism definition.</summary>
        /// <remarks> Get the definition of a default prism.</remarks>
        /// <param name="prismType"> Prism type </param>
        /// <example> mod3 call 000001 BAP_GetPrismDef </example>
        [COMF]
        public BAP_PRISMDEF? BAP_GetPrismDef(BAP_PRISMTYPE prismType)
        {
            var resp = Request(RequestString("%R1Q,17023:", prismType));
            if (resp.ReturnCode == GRC.IVPARAM)
                throw new LeicaException(resp.ReturnCode, "Invalid prism type.");

            if (resp.ReturnCode == GRC.OK && resp.Values.Length == 3)
                return new BAP_PRISMDEF()
                {
                    Name = resp.Values[0].ToString(),
                    AddConst = double.Parse(resp.Values[1].ToString()),
                    ReflType = (BAP_REFLTYPE)resp.Values[2]
                };

            return null;
        }

        /// <summary> Getting the user prism definition.</summary>
        /// <remarks> Gets definition of a defined user prism.</remarks>
        /// <param name="prismName"> Prism name </param>
        /// <example> mod3 call 000001 BAP_GetUserPrismDef </example>
        [COMF]
        public BAP_PRISMDEF? BAP_GetUserPrismDef(string prismName)
        {
            var resp = Request(RequestString("%R1Q,17033:", prismName));
            if (resp.ReturnCode == GRC.IVPARAM)
                throw new LeicaException(resp.ReturnCode, "Invalid prism definition.");

            if (resp.ReturnCode == GRC.OK && resp.Values.Length == 3)
                return new BAP_PRISMDEF()
                {
                    AddConst = double.Parse(resp.Values[0].ToString()),
                    ReflType = (BAP_REFLTYPE)resp.Values[1],
                    Creator = resp.Values[2].ToString()
                };

            return null;
        }

        /// <summary> Getting the default prism definition.</summary>
        /// <remarks> Get the definition of a default prism.</remarks>
        /// <param name="prismName"> Prism name </param>
        /// <param name="addConst"> Prism correction [m] </param>
        /// <param name="reflType"> Reflector type </param>
        /// <param name="creator"> Name of creator </param>
        /// <example> mod3 call 000001 BAP_SetUserPrismDef </example>
        [COMF]
        public bool BAP_SetUserPrismDef(string prismName, double addConst, BAP_REFLTYPE reflType, string creator)
        {
            var resp = Request(RequestString("%R1Q,17023:", prismName, addConst, reflType, creator));
            return (resp.ReturnCode) switch
            {
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Invalid prism definition."),
                GRC.IVRESULT => throw new LeicaException(resp.ReturnCode, "Prism definition is not set."),
                _ => resp.ReturnCode == GRC.OK
            };
        }

        /// <summary> Getting the actual distance measurement program.</summary>
        /// <remarks> Gets the current distance measurement program.</remarks>
        /// <returns> Actual measurement program </returns>
        /// <example> mod3 call 000001 BAP_GetMeasPrg </example>
        [COMF]
        public BAP_USER_MEASPRG? BAP_GetMeasPrg()
        {
            var resp = Request(RequestString("%R1Q,17018:"));
            if (resp.ReturnCode == GRC.OK && resp.Values.Length == 1)
                return (BAP_USER_MEASPRG)resp.Values[0];

            return null;
        }

        /// <summary> Setting the distance measurement program.</summary>
        /// <remarks> Defines the distance measurement program i.e. for BAP_MeasDistanceAngle.<br/>RL EDM type programs are not available on all instrument types.<br/>Changing the measurement programs may change the EDM type as well(Reflector (IR) and Reflectorless (RL)).</remarks>
        /// <example> mod3 call 000001 BAP_SetMeasPrg </example>
        [COMF]
        public bool BAP_SetMeasPrg(BAP_USER_MEASPRG measPrg)
        {
            var resp = Request(RequestString("%R1Q,17019:", measPrg));
            return (resp.ReturnCode) switch
            {
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Measurement program is not available."),
                _ => resp.ReturnCode == GRC.OK
            };
        }

        /// <summary> Measuring Hz,V angles and a single distance.</summary>
        /// <remarks> This function measures angles and a single distance depending on the mode DistMode. Note that this function is not suited for continuous measurements(LOCK mode and TRK mode). This command uses the current automation settings.</remarks>
        /// <param name="distMode"> BAP_DEF_DIST uses the predefined distance measurement program as defined in BAP_SetMeasPrg.</param>
        /// <returns> Horizontal angel [rad]<br/>Vertical angel [rad]<br/>Slopedistance [rad]<br/>Actual distance measurement mode.</returns>
        /// <example> mod3 call 000001 BAP_MeasDistanceAngle </example>
        [COMF]
        public BAP_MEASURE_PRG? BAP_MeasDistanceAngle(BAP_MEASURE_PRG distMode, out double Hz, out double V, out double dist)
        {
            var resp = Request(RequestString("%R1Q,17017:", distMode));
            switch (resp.ReturnCode)
            {
                case GRC.IVPARAM: throw new LeicaException(resp.ReturnCode, "Wrong value for DistMode!");
                case GRC.ABORT: throw new LeicaException(resp.ReturnCode, "Measurement aborted!");
                case GRC.SHUT_DOWN: throw new LeicaException(resp.ReturnCode, "System has been stopped!");
                case GRC.COM_TIMEDOUT: throw new LeicaException(resp.ReturnCode, "Error, communication timeout. (possibly increase COM timeout, see COM_SetTimeout)");

                case GRC.AUT_ANGLE_ERROR: throw new LeicaException(resp.ReturnCode, "Angle measurement error.");
                case GRC.AUT_BAD_ENVIRONMENT: throw new LeicaException(resp.ReturnCode, "Bad Environment conditions.");
                case GRC.AUT_CALACC: throw new LeicaException(resp.ReturnCode, "ATR-calibration failed.");
                case GRC.AUT_DETECTOR_ERROR: throw new LeicaException(resp.ReturnCode, "Error in target acquisition.");
                case GRC.AUT_DEV_ERROR: throw new LeicaException(resp.ReturnCode, "Deviation measurement error.");
                case GRC.AUT_INCACC: throw new LeicaException(resp.ReturnCode, "Position not exactly reached.");
                case GRC.AUT_MOTOR_ERROR: throw new LeicaException(resp.ReturnCode, "Motorization error.");
                case GRC.AUT_MULTIPLE_TARGETS: throw new LeicaException(resp.ReturnCode, "Multiple targets detected.");
                case GRC.AUT_NO_TARGET: throw new LeicaException(resp.ReturnCode, "No target detected.");
                case GRC.AUT_TIMEOUT: throw new LeicaException(resp.ReturnCode, "Position not reached.");

                case GRC.TMC_DIST_PPM: throw new LeicaException(resp.ReturnCode, "PPM or MM should be switched off when EDM is on -> no results!");
                case GRC.TMC_DIST_ERROR: throw new LeicaException(resp.ReturnCode, "Error occured during distance measurement!");
                case GRC.TMC_ANGLE_ERROR: throw new LeicaException(resp.ReturnCode, "Error occured while slope was measured!");
                case GRC.TMC_BUSY: throw new LeicaException(resp.ReturnCode, "TMC is busy!");
                case GRC.TMC_ANGLE_OK: throw new LeicaException(resp.ReturnCode, "Angle without coordinates!");
                case GRC.TMC_ACCURACY_GUARANTEE: throw new LeicaException(resp.ReturnCode, "Info, accuracy cannot be guaranteed.");
                case GRC.TMC_ANGLE_NO_ACC_GUARANTY: throw new LeicaException(resp.ReturnCode, "Info, only angle measurement valid, accuracy cannot be guaranteed.");
                case GRC.TMC_ANGLE_NOT_FULL_CORR: throw new LeicaException(resp.ReturnCode, "Warning, only angle measurement valid, accuracy cannot be guaranteed.");
                case GRC.TMC_NO_FULL_CORRECTION: throw new LeicaException(resp.ReturnCode, "Warning, measurement without full correction.");
                case GRC.TMC_SIGNAL_ERROR: throw new LeicaException(resp.ReturnCode, "Error, no signal on EDM (only in signal mode).");

                case GRC.OK:
                    if (resp.Values.Length == 4)
                    {
                        Hz = double.Parse(resp.Values[0].ToString());
                        V = double.Parse(resp.Values[1].ToString());
                        dist = double.Parse(resp.Values[2].ToString());
                        return (BAP_MEASURE_PRG)resp.Values[3];
                    }
                    break;
            }
            Hz = V = dist = 0;
            return null;
        }

        /// <summary> Searching the target.</summary>
        /// <remarks> This function searches for a target in the configured or defined ATR SearchWindow. The functionality is only available for automated instruments.</remarks>
        /// <param name="dummy"> It’s reserved for future use, set bDummy always to FALSE.</param>
        /// <example> mod3 call 000001 BAP_SearchTarget </example>
        [COMF]
        public bool BAP_SearchTarget(bool dummy = false)
        {
            var resp = Request(RequestString("%R1Q,17020:", dummy));
            switch (resp.ReturnCode)
            {
                case GRC.AUT_BAD_ENVIRONMENT: throw new LeicaException(resp.ReturnCode, "Bad Environment conditions.");
                case GRC.AUT_DEV_ERROR: throw new LeicaException(resp.ReturnCode, "Deviation measurement error.");
                case GRC.AUT_ACCURACY: throw new LeicaException(resp.ReturnCode, "Position not exactly reached.");
                case GRC.AUT_MOTOR_ERROR: throw new LeicaException(resp.ReturnCode, "Motorization error.");
                case GRC.AUT_MULTIPLE_TARGETS: throw new LeicaException(resp.ReturnCode, "Multiple targets detected.");
                case GRC.AUT_NO_TARGET: throw new LeicaException(resp.ReturnCode, "No target detected.");
                case GRC.AUT_TIMEOUT: throw new LeicaException(resp.ReturnCode, "Time out, no target found.");
                case GRC.ABORT: throw new LeicaException(resp.ReturnCode, "Error, searching aborted.");
                case GRC.FATAL: throw new LeicaException(resp.ReturnCode, "Fatal Error.");
                case GRC.OK:
                    return true;
            }
            return false;
        }

        /// <summary> Getting the current ATR low vis mode.</summary>
        /// <remarks> Gets the current low vis mode.</remarks>
        /// <example> mod3 call 000001 BAP_GetATRSetting </example>
        [COMF]
        public BAP_ATRSETTING? BAP_GetATRSetting()
        {
            var resp = Request(RequestString("%R1Q,17034:"));
            if (resp.ReturnCode == GRC.OK && resp.Values.Length == 1)
                return (BAP_ATRSETTING)resp.Values[0];

            return null;
        }

        /// <summary> Setting the current ATR low vis mode.</summary>
        /// <remarks> Sets the current low vis mode.</remarks>
        /// <example> mod3 call 000001 BAP_SetATRSetting </example>
        [COMF]
        public bool BAP_SetATRSetting(BAP_ATRSETTING ATRSetting)
        {
            var resp = Request(RequestString("%R1Q,17035:", ATRSetting));
            return resp.ReturnCode == GRC.OK;
        }

        /// <summary> Getting the reduced ATR field of view.</summary>
        /// <remarks> Get reduced ATR field of view mode.</remarks>
        /// <returns> ON: ATR uses reduced field of view(about 1/9).<br/>OFF: ATR uses full field of view.</returns>
        /// <example> mod3 call 000001 BAP_GetRedATRFov </example>
        [COMF]
        public ON_OFF_TYPE? BAP_GetRedATRFov()
        {
            var resp = Request(RequestString("%R1Q,17036:"));
            if (resp.ReturnCode == GRC.OK && resp.Values.Length == 1)
                return (ON_OFF_TYPE)resp.Values[0];

            return null;
        }

        #endregion BASIC APPLICATIONS (BAP CONF)

        #region BASIC MAN MACHINE INTERFACE (BMM COMF)

        /// <summary> Outputing an alarm signal (triple beep).</summary>
        /// <remarks> This function produces a triple beep with the configured intensity and frequency, which cannot be changed. If there is a continuous signal active, it will be stopped before.</remarks>
        /// <example> mod3 call 000001 BMM_BeepAlarm </example>
        [COMF]
        public ZResponse BMM_BeepAlarm()
        {
            var resp = Request(RequestString("%R1Q,11004:"));
            if (resp.ReturnCode == GRC.OK)
            {
            }
            return resp;
        }

        /// <summary> Outputing an alarm signal (single beep).</summary>
        /// <remarks> This function produces a single beep with the configured intensity and frequency, which cannot be changed. If a continuous signal is active, it will be stopped first.</remarks>
        /// <example> mod3 call 000001 BMM_BeepNormal </example>
        [COMF]
        public ZResponse BMM_BeepNormal()
        {
            var resp = Request(RequestString("%R1Q,11003:"));
            if (resp.ReturnCode == GRC.OK)
            {
            }
            return resp;
        }

        /// <summary> Starting a continuous beep signal.</summary>
        /// <remarks> This function switches on the beep-signal with the intensity nIntens. If a continuous signal is active, it will be stopped first.Turn off the beeping device with IOS_BeepOff.</remarks>
        /// <param name="intens">Intensity of the beep-signal (volume) expressed as a percentage(0-100 %).</param>
        /// <example> mod3 call 000001 IOS_BeepOn </example>
        [COMF]
        public ZResponse IOS_BeepOn(int intens = IOS_BEEP_STDINTENS)
        {
            var resp = Request(RequestString("%R1Q,20001:", intens));
            if (resp.ReturnCode == GRC.OK)
            {
            }
            return resp;
        }

        /// <summary> Stopping an active beep signal.</summary>
        /// <remarks> This function switches off the beep-signal.</remarks>
        /// <example> mod3 call 000001 IOS_BeepOff </example>
        [COMF]
        public ZResponse IOS_BeepOff()
        {
            var resp = Request(RequestString("%R1Q,20000:"));
            if (resp.ReturnCode == GRC.OK)
            {
            }
            return resp;
        }

        #endregion BASIC MAN MACHINE INTERFACE (BMM COMF)

        #region COMMUNICATIONS (COM COMF)

        /// <summary> Retrieving server instrument version.</summary>
        /// <remarks> This function displays the current GeoCOM release (release, version and subversion) of the instrument.</remarks>
        /// <example> mod3 call 000001 COM_GetSWVersion </example>
        [COMF]
        public Version? COM_GetSWVersion()
        {
            var resp = Request(RequestString("%R1Q,110:"));
            if (resp.ReturnCode == GRC.OK && resp.Values.Length == 3)
                return new Version((int)(long)resp.Values[0], (int)(long)resp.Values[1], (int)(long)resp.Values[2]);

            return null;
        }

        /// <summary> Turning on the instrument.</summary>
        /// <remarks> This function switches on the TPS1200 instrument.<br/><b>Note</b>: The TPS1200 instrument can be switched on by any RPC command or even by sending a single character.</remarks>
        /// <returns> If instrument is already switched on then %R1P,0,0:5 else Nothing </returns>
        /// <param name="onMode"> Run mode.</param>
        /// <example> mod3 call 000001 COM_SwitchOnTPS 0/1 </example>
        [COMF]
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
        /// <example> mod3 call 000001 COM_SwitchOffTPS 0/1 </example>
        [COMF]
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
        /// <example> mod3 call 000001 COM_NullProc </example>
        [COMF]
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
        /// <example> mod3 call 000001 COM_GetBinaryAvailable </example>
        [COMF]
        public bool? COM_GetBinaryAvailable()
        {
            var resp = Request(RequestString("%R1Q,113:"));
            if (resp.ReturnCode == GRC.OK && resp.Values.Length == 1)
                return resp.Values[0].Equals(0L); // binary operation enabled / ASCII operation enabled

            return null;
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
                return resp.Values[0].ToString();

            return null;
        }

        /// <summary> Getting the Leica specific instrument name.</summary>
        /// <remarks> Gets the instrument name, for example: TS30 0,5".</remarks>
        [COMF]
        public string? CSV_GetInstrumentName()
        {
            var resp = Request(RequestString("%R1Q,5004:"));
            if (resp.ReturnCode == GRC.OK && resp.Values.Length == 1)
                return resp.Values[0].ToString();

            return null;
        }

        /// <summary> Getting the instrument configuration.</summary>
        /// <remarks> This function returns information about the class and the configuration type of the instrument.</remarks>
        [COMF]
        public long[]? CSV_GetDeviceConfig()
        {
            var resp = Request(RequestString("%R1Q,5035:"));
            if (resp.ReturnCode == GRC.OK && resp.Values.Length == 2)
                return new long[] { (long)resp.Values[0], (long)resp.Values[1] };

            return null;
        }

        /// <summary> Getting the RL type.</summary>
        /// <remarks> This function returns information about the reflectorless and long range distance measurement (RL) of the sinstrument.</remarks>
        [COMF]
        public long? CSV_GetReflectorlessClass()
        {
            var resp = Request(RequestString("%R1Q,5100:"));
            if (resp.ReturnCode == GRC.OK)
                return (long)resp.Values[0];

            return null;
        }

        /// <summary> Getting the date and time.</summary>
        /// <remarks> Gets the current date and time of the instrument. The ASCII response is formatted corresponding to the data type DATIME.A possible response can look like this: %R1P,0,0:0,1996,'07', '19','10','13','2f' (see chapter ASCII data type declaration for further information).</remarks>
        [COMF]
        public DateTime? CSV_GetDateTime()
        {
            var resp = Request(RequestString("%R1Q,5008:"));
            if (resp.ReturnCode == GRC.OK && resp.Values.Length == 6)
                return new DateTime((int)(long)resp.Values[0], (int)(long)resp.Values[1], (int)(long)resp.Values[2], (int)(long)resp.Values[3], (int)(long)resp.Values[4], (int)(long)resp.Values[5]);

            return null;
        }

        /// <summary> Getting the date and time.</summary>
        /// <remarks> Gets the current date and time of the instrument.</remarks>
        [COMF]
        public DateTime? CSV_GetDateTimeCentiSec()
        {
            var resp = Request(RequestString("%R1Q,5117:"));
            if (resp.ReturnCode == GRC.OK && resp.Values.Length == 7)
                return new DateTime((int)(long)resp.Values[0], (int)(long)resp.Values[1], (int)(long)resp.Values[2], (int)(long)resp.Values[3], (int)(long)resp.Values[4], (int)(long)resp.Values[5], (int)(long)resp.Values[6]);

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
            if (resp.ReturnCode == GRC.OK && resp.Values.Length == 3)
                return new Version((int)(long)resp.Values[0], (int)(long)resp.Values[1], (int)(long)resp.Values[2]);

            return null;
        }

        /// <summary> Checking the available power.</summary>
        /// <remarks> This command returns the capacity of the current power source and its source (internal or external).</remarks>
        [COMF]
        public string? CSV_CheckPower()
        {
            var resp = Request(RequestString("%R1Q,5039:"));
            if ((resp.ReturnCode == GRC.OK || resp.ReturnCode == GRC.LOW_POWER || resp.ReturnCode == GRC.BATT_EMPTY) && resp.Values.Length == 3)
            {
                var capacity = resp.Values[0];
                var powerSource = resp.Values[1].Equals(0L) ? "От сети" : "От батареи";
                var powerSuggest = (long)resp.Values[2]; // Not supported

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
            if (resp.ReturnCode == GRC.OK && resp.Values.Length == 1)
                return Math.Round(double.Parse(resp.Values[0].ToString(), CultureInfo.InvariantCulture), 1);

            return null;
        }

        #endregion CENTRAL SERVICES (CSV COMF)

        #region ELECTRONIC DISTANCE MEASUREMENT (EDM COMF)

        /// <summary> Turning on/off the laserpointer.</summary>
        /// <remarks> Laserpointer is only available on models which support distance measurement without reflector.</remarks>
        /// <example> mod3 call 000001 EDM_Laserpointer on/off (0/1) </example>
        [COMF]
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
        /// <example> mod3 call 000001 EDM_GetEglIntensity </example>
        [COMF]
        public EDM_EGLINTENSITY_TYPE? EDM_GetEglIntensity()
        {
            var resp = Request(RequestString("%R1Q,1058:"));
            if (resp.ReturnCode == GRC.OK && resp.Values.Length == 1)
                return (EDM_EGLINTENSITY_TYPE)resp.Values[0];

            return null;
        }

        /// <summary> Changing the intensity of the electronic guide light.</summary>
        /// <remarks> Changes the intensity of the Electronic Guide Light.</remarks>
        /// <example> mod3 call 000001 EDM_SetEglIntensity off/low/mid/high (0/1/2/3) </example>
        [COMF]
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
        static byte[] RequestString(string command, params object[] data) =>
            LF.Concat(Encoding.ASCII.GetBytes(command + string.Join(',', data.Where(p => p != null).Select(p =>
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
