//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: GRC – Общие коды возврата выполнения функций (COMF).
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Devices.Leica
{
    #region Using
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    #endregion Using

    /// <summary> General Return Code </summary>
    [JsonConverter(typeof(GrcJsonConverter))]
    public enum GRC : long
    {
        #region CUSTOM

        /// <summary> [128] Недопустимая длинна наименования призмы.</summary>
        PRISM_NAME_LEN_INVALID = 0x0080,

        #endregion CUSTOM

        #region TPS 0x0

        /// <summary> [0] Function successfully completed.</summary>
        [Description("Function successfully completed.")]
        OK = 0x0000,
        /// <summary> [1] Unknown error, result unspecified.</summary>
        [Description("Unknown error, result unspecified.")]
        UNDEFINED = 0x0001,
        /// <summary> [2] Invalid parameter detected. Result unspecified.</summary>
        [Description("Invalid parameter detected. Result unspecified.")]
        IVPARAM = 0x0002,
        /// <summary> [3] Invalid result.</summary>
        [Description("Invalid result.")]
        IVRESULT = 0x0003,
        /// <summary> [4] Fatal error.</summary>
        [Description("Fatal error.")]
        FATAL = 0x0004,
        /// <summary> [5] Not implemented yet.</summary>
        [Description("Not implemented yet.")]
        NOT_IMPL = 0x0005,
        /// <summary> [6] Function execution timed out. Result unspecified.</summary>
        TIME_OUT = 0x0006,
        /// <summary> [7] Parameter setup for subsystem is incomplete.</summary>
        SET_INCOMPL = 0x0007,
        /// <summary> [8] Function execution has been aborted.</summary>
        ABORT = 0x0008,
        /// <summary> [9] Fatal error - not enough memory.</summary>
        NOMEMORY = 0x0009,
        /// <summary> [10] Fatal error - subsystem not initialized.</summary>
        NOTINIT = 0x000A,
        /// <summary> [12] Subsystem is down.</summary>
        SHUT_DOWN = 0x000C,
        /// <summary> [13] System busy/already in use of another process. Cannot execute function.</summary>
        SYSBUSY = 0x000D,
        /// <summary> [14] Fatal error - hardware failure.</summary>
        HWFAILURE = 0x000E,
        /// <summary> [15] Execution of application has been aborted (SHIFT-ESC).</summary>
        ABORT_APPL = 0x000F,
        /// <summary> [16] Operation aborted - insufficient power supply level.</summary>
        LOW_POWER = 0x0010,
        /// <summary> [17] Invalid version of file, ...</summary>
        IVVERSION = 0x0011,
        /// <summary> [18] Battery empty.</summary>
        BATT_EMPTY = 0x0012,
        /// <summary> [20] no event pending.</summary>
        NO_EVENT = 0x0014,
        /// <summary> [21] out of temperature range.</summary>
        OUT_OF_TEMP = 0x0015,
        /// <summary> [22] instrument tilting out of range.</summary>
        INSTRUMENT_TILT = 0x0016,
        /// <summary> [23] communication error.</summary>
        COM_SETTING = 0x0017,
        /// <summary> [24] TYPE Input 'do no action'.</summary>
        NO_ACTION = 0x0018,
        /// <summary> [25] Instr. run into the sleep mode.</summary>
        SLEEP_MODE = 0x0019,
        /// <summary> [26] Function not successfully completed.</summary>
        NOTOK = 0x001A,
        /// <summary> [27] Not available.</summary>
        NA = 0x001B,
        /// <summary> [28] Overflow error.</summary>
        OVERFLOW = 0x001C,
        /// <summary> [29] System or subsystem has been stopped ANG 256 0x100.</summary>
        STOPPED = 0x001D,

        #endregion TPS 0x0

        #region ANG 0x100

        /// <summary> [257] Angles and Inclinations not valid.</summary>
        ANG_ERROR = 0x0101,
        /// <summary> [258] inclinations not valid.</summary>
        ANG_INCL_ERROR = 0x0102,
        /// <summary> [259] value accuracies not reached.</summary>
        ANG_BAD_ACC = 0x0103,
        /// <summary> [260] angle-accuracies not reached.</summary>
        ANG_BAD_ANGLE_ACC = 0x0104,
        /// <summary> [261] inclination accuracies not reached.</summary>
        ANG_BAD_INCLIN_ACC = 0x0105,
        /// <summary> [266] no write access allowed.</summary>
        ANG_WRITE_PROTECTED = 0x010A,
        /// <summary> [267] value out of range.</summary>
        ANG_OUT_OF_RANGE = 0x010B,
        /// <summary> [268] function aborted due to interrupt.</summary>
        ANG_IR_OCCURED = 0x010C,
        /// <summary> [269] hz moved during incline measurement.</summary>
        ANG_HZ_MOVED = 0x010D,
        /// <summary> [270] troubles with operation system.</summary>
        ANG_OS_ERROR = 0x010E,
        /// <summary> [271] overflow at parameter values.</summary>
        ANG_DATA_ERROR = 0x010F,
        /// <summary> [272] too less peaks.</summary>
        ANG_PEAK_CNT_UFL = 0x0110,
        /// <summary> [273] reading timeout.</summary>
        ANG_TIME_OUT = 0x0111,
        /// <summary> [274] too many exposures wanted.</summary>
        ANG_TOO_MANY_EXPOS = 0x0112,
        /// <summary> [275] picture height out of range.</summary>
        ANG_PIX_CTRL_ERR = 0x0113,
        /// <summary> [276] positive exposure dynamic overflow.</summary>
        ANG_MAX_POS_SKIP = 0x0114,
        /// <summary> [277] negative exposure dynamic overflow.</summary>
        ANG_MAX_NEG_SKIP = 0x0115,
        /// <summary> [278] exposure time overflow.</summary>
        ANG_EXP_LIMIT = 0x0116,
        /// <summary> [279] picture underexposured.</summary>
        ANG_UNDER_EXPOSURE = 0x0117,
        /// <summary> [280] picture overexposured.</summary>
        ANG_OVER_EXPOSURE = 0x0118,
        /// <summary> [300] too many peaks detected.</summary>
        ANG_TMANY_PEAKS = 0x012C,
        /// <summary> [301] too less peaks detected.</summary>
        ANG_TLESS_PEAKS = 0x012D,
        /// <summary> [302] peak too slim.</summary>
        ANG_PEAK_TOO_SLIM = 0x012E,
        /// <summary> [303] peak to wide.</summary>
        ANG_PEAK_TOO_WIDE = 0x012F,
        /// <summary> [304] bad peak difference.</summary>
        ANG_BAD_PEAKDIFF = 0x0130,
        /// <summary> [305] too less peak amplitude.</summary>
        ANG_UNDER_EXP_PICT = 0x0131,
        /// <summary> [306] inhomogeneous peak amplitudes.</summary>
        ANG_PEAKS_INHOMOGEN = 0x0132,
        /// <summary> [307] no peak decoding possible.</summary>
        ANG_NO_DECOD_POSS = 0x0133,
        /// <summary> [308] peak decoding not stable.</summary>
        ANG_UNSTABLE_DECOD = 0x0134,
        /// <summary> [309] too less valid finepeaks.</summary>
        ANG_TLESS_FPEAKS = 0x0135,
        /// <summary> [316] inclination plane out of time range.</summary>
        ANG_INCL_OLD_PLANE = 0x013C,
        /// <summary> [317] inclination no plane available.</summary>
        ANG_INCL_NO_PLANE = 0x013D,
        /// <summary> [326] errors in 5kHz and or 2.5kHz angle.</summary>
        ANG_FAST_ANG_ERR = 0x0146,
        /// <summary> [5] errors in 5kHz angle.</summary>
        ANG_FAST_ANG_ERR_5 = 0x0005,
        /// <summary> [25] errors in 2.5kHz angle.</summary>
        ANG_FAST_ANG_ERR_25 = 0x0019,
        /// <summary> [329] LVDS transfer error detected.</summary>
        ANG_TRANS_ERR = 0x0149,
        /// <summary> [5] LVDS transfer error detected in 5kHz mode.</summary>
        ANG_TRANS_ERR_5 = 0x0005,
        /// <summary> [25] LVDS transfer error detected in 2.5kHz mode ATA 512 0x200.</summary>
        ANG_TRANS_ERR_25 = 0x0019,

        #endregion ANG 0x100

        #region ATA 0x200

        /// <summary> [512] ATR-System is not ready.</summary>
        ATA_NOT_READY = 0x0200,
        /// <summary> [513] Result isn't available yet.</summary>
        ATA_NO_RESULT = 0x0201,
        /// <summary> [514] Several Targets detected.</summary>
        ATA_SEVERAL_TARGETS = 0x0202,
        /// <summary> [515] Spot is too big for analyse.</summary>
        ATA_BIG_SPOT = 0x0203,
        /// <summary> [516] Background is too bright.</summary>
        ATA_BACKGROUND = 0x0204,
        /// <summary> [517] No targets detected.</summary>
        ATA_NO_TARGETS = 0x0205,
        /// <summary> [518] Accuracy worse than asked for.</summary>
        ATA_NOT_ACCURAT = 0x0206,
        /// <summary> [519] Spot is on the edge of the sensing area.</summary>
        ATA_SPOT_ON_EDGE = 0x0207,
        /// <summary> [522] Blooming or spot on edge detected.</summary>
        ATA_BLOOMING = 0x020A,
        /// <summary> [523] ATR isn't in a continuous mode.</summary>
        ATA_NOT_BUSY = 0x020B,
        /// <summary> [524] Not the spot of the own target illuminator.</summary>
        ATA_STRANGE_LIGHT = 0x020C,
        /// <summary> [24] Communication error to sensor (ATR).</summary>
        ATA_V24_FAIL = 0x0018,
        /// <summary> [526] Received Arguments cannot be decoded.</summary>
        ATA_DECODE_ERROR = 0x020E,
        /// <summary> [527] No Spot detected in Hz-direction.</summary>
        ATA_HZ_FAIL = 0x020F,
        /// <summary> [528] No Spot detected in V-direction.</summary>
        ATA_V_FAIL = 0x0210,
        /// <summary> [529] Strange light in Hz-direction.</summary>
        ATA_HZ_STRANGE_L = 0x0211,
        /// <summary> [530] Strange light in V-direction.</summary>
        ATA_V_STRANGE_L = 0x0212,
        /// <summary> [531] On multiple ATA_SLDR_OpenTransfer.</summary>
        ATA_SLDR_TRANSFER_PENDING = 0x0213,
        /// <summary> [532] No ATA_SLDR_OpenTransfer happened.</summary>
        ATA_SLDR_TRANSFER_ILLEGAL = 0x0214,
        /// <summary> [533] Unexpected data format received.</summary>
        ATA_SLDR_DATA_ERROR = 0x0215,
        /// <summary> [534] Checksum error in transmitted data.</summary>
        ATA_SLDR_CHK_SUM_ERROR = 0x0216,
        /// <summary> [535] Address out of valid range.</summary>
        ATA_SLDR_ADDRESS_ERROR = 0x0217,
        /// <summary> [536] Firmware file has invalid format.</summary>
        ATA_SLDR_INV_LOADFILE = 0x0218,
        /// <summary> [537] Current (loaded) firmware doesn't support upload.</summary>
        ATA_SLDR_UNSUPPORTED = 0x0219,
        /// <summary> [538] PS-System is not ready.</summary>
        ATA_PS_NOT_READY = 0x021A,
        /// <summary> [539] ATR system error EDM 768 0x300.</summary>
        ATA_ATR_SYSTEM_ERR = 0x021B,

        #endregion ATA 0x200

        #region EDM 0x300

        /// <summary> [769] Fatal EDM sensor error. See for the exact reason the original EDM sensor error number.</summary>
        EDM_SYSTEM_ERR = 0x0301,
        /// <summary> [770] Invalid command or unknown command, see command syntax.</summary>
        EDM_INVALID_COMMAND = 0x0302,
        /// <summary> [771] Boomerang error.</summary>
        EDM_BOOM_ERR = 0x0303,
        /// <summary> [772] Received signal to low, prism to far away, or natural barrier, bad environment, etc.</summary>
        EDM_SIGN_LOW_ERR = 0x0304,
        /// <summary> [773] obsolete.</summary>
        EDM_DIL_ERR = 0x0305,
        /// <summary> [774] Received signal to strong, prism to near, stranger light effect.</summary>
        EDM_SIGN_HIGH_ERR = 0x0306,
        /// <summary> [775] Timeout, measuring time exceeded (signal too weak, beam interrupted,..).</summary>
        EDM_TIMEOUT = 0x0307,
        /// <summary> [776] to much turbulences or distractions.</summary>
        EDM_FLUKT_ERR = 0x0308,
        /// <summary> [777] filter motor defective.</summary>
        EDM_FMOT_ERR = 0x0309,
        /// <summary> [778] Device like EGL, DL is not installed.</summary>
        EDM_DEV_NOT_INSTALLED = 0x030A,
        /// <summary> [779] Search result invalid. For the exact explanation, see in the description of the called function.</summary>
        EDM_NOT_FOUND = 0x030B,
        /// <summary> [780] Communication ok, but an error reported from the EDM sensor.</summary>
        EDM_ERROR_RECEIVED = 0x030C,
        /// <summary> [781] No service password is set.</summary>
        EDM_MISSING_SRVPWD = 0x030D,
        /// <summary> [782] Communication ok, but an unexpected answer received.</summary>
        EDM_INVALID_ANSWER = 0x030E,
        /// <summary> [783] Data send error, sending buffer is full.</summary>
        EDM_SEND_ERR = 0x030F,
        /// <summary> [784] Data receive error, like parity buffer overflow.</summary>
        EDM_RECEIVE_ERR = 0x0310,
        /// <summary> [785] Internal EDM subsystem error.</summary>
        EDM_INTERNAL_ERR = 0x0311,
        /// <summary> [786] Sensor is working already, abort current measuring first.</summary>
        EDM_BUSY = 0x0312,
        /// <summary> [787] No measurement activity started.</summary>
        EDM_NO_MEASACTIVITY = 0x0313,
        /// <summary> [788] Calculated checksum, resp. received data wrong (only in binary communication mode.</summary>
        EDM_CHKSUM_ERR = 0x0314,
        /// <summary> [789] During start up or shut down phase an error occured. It is saved in the DEL buffer.</summary>
        EDM_INIT_OR_STOP_ERR = 0x0315,
        /// <summary> [790] Red laser not available on this sensor HW.</summary>
        EDM_SRL_NOT_AVAILABLE = 0x0316,
        /// <summary> [791] Measurement will be aborted (will be used for the laser security).</summary>
        EDM_MEAS_ABORTED = 0x0317,
        /// <summary> [798] Multiple OpenTransfer calls.</summary>
        EDM_SLDR_TRANSFER_PENDING = 0x031E,
        /// <summary> [799] No open transfer happened.</summary>
        EDM_SLDR_TRANSFER_ILLEGAL = 0x031F,
        /// <summary> [800] Unexpected data format received.</summary>
        EDM_SLDR_DATA_ERROR = 0x0320,
        /// <summary> [801] Checksum error in transmitted data.</summary>
        EDM_SLDR_CHK_SUM_ERROR = 0x0321,
        /// <summary> [802] Address out of valid range.</summary>
        EDM_SLDR_ADDR_ERROR = 0x0322,
        /// <summary> [803] Firmware file has invalid format.</summary>
        EDM_SLDR_INV_LOADFILE = 0x0323,
        /// <summary> [804] Current (loaded) firmware doesn't support upload.</summary>
        EDM_SLDR_UNSUPPORTED = 0x0324,
        /// <summary> [808] Undocumented error from the EDM sensor, should not occur.</summary>
        EDM_UNKNOW_ERR = 0x0328,
        /// <summary> [818] Out of distance range (dist too small or large).</summary>
        EDM_DISTRANGE_ERR = 0x0332,
        /// <summary> [819] Signal to noise ratio too small.</summary>
        EDM_SIGNTONOISE_ERR = 0x0333,
        /// <summary> [820] Noise to high.</summary>
        EDM_NOISEHIGH_ERR = 0x0334,
        /// <summary> [821] Password is not set.</summary>
        EDM_PWD_NOTSET = 0x0335,
        /// <summary> [822] Elapsed time between prepare und start fast measurement for ATR to long.</summary>
        EDM_ACTION_NO_MORE_VALID = 0x0336,
        /// <summary> [823] Possibly more than one target (also a sensor error).</summary>
        EDM_MULTRG_ERR = 0x0337,
        /// <summary> [824] eeprom consts are missing.</summary>
        EDM_MISSING_EE_CONSTS = 0x0338,
        /// <summary> [825] No precise measurement possible.</summary>
        EDM_NOPRECISE = 0x0339,
        /// <summary> [826] Measured distance is to big (not allowed) TMC 1280 0x500.</summary>
        EDM_MEAS_DIST_NOT_ALLOWED = 0x033A,

        #endregion EDM 0x300

        #region TMC 0x500

        /// <summary> [1283] Warning: measurement without full correction.</summary>
        TMC_NO_FULL_CORRECTION = 0x0503,
        /// <summary> [1284] Info: accuracy can not be guarantee.</summary>
        TMC_ACCURACY_GUARANTEE = 0x0504,
        /// <summary> [1285] Warning: only angle measurement valid.</summary>
        TMC_ANGLE_OK = 0x0505,
        /// <summary> [1288] Warning: only angle measurement valid but without full correction.</summary>
        TMC_ANGLE_NO_FULL_CORRECTION = 0x0508,
        /// <summary> [1289] Info: only angle measurement valid but accuracy can not be guarantee.</summary>
        TMC_ANGLE_NO_ACC_GUARANTY = 0x0509,
        /// <summary> [1290] Error: no angle measurement.</summary>
        TMC_ANGLE_ERROR = 0x050A,
        /// <summary> [1291] Error: wrong setting of PPM or MM on EDM.</summary>
        TMC_DIST_PPM = 0x050B,
        /// <summary> [1292] Error: distance measurement not done (no aim, etc.).</summary>
        TMC_DIST_ERROR = 0x050C,
        /// <summary> [1293] Error: system is busy (no measurement done).</summary>
        TMC_BUSY = 0x050D,
        /// <summary> [1294] Error: no signal on EDM (only in signal mode) MOT 1792 0x700.</summary>
        TMC_SIGNAL_ERROR = 0x050E,

        #endregion TMC 0x500

        #region MOT 0x700

        /// <summary> [1792] motorization is not ready.</summary>
        MOT_UNREADY = 0x0700,
        /// <summary> [1793] motorization is handling another task.</summary>
        MOT_BUSY = 0x0701,
        /// <summary> [1794] motorization is not in velocity mode.</summary>
        MOT_NOT_OCONST = 0x0702,
        /// <summary> [1795] motorization is in the wrong mode or busy.</summary>
        MOT_NOT_CONFIG = 0x0703,
        /// <summary> [1796] motorization is not in posit mode.</summary>
        MOT_NOT_POSIT = 0x0704,
        /// <summary> [1797] motorization is not in service mode.</summary>
        MOT_NOT_SERVICE = 0x0705,
        /// <summary> [1798] motorization is handling no task.</summary>
        MOT_NOT_BUSY = 0x0706,
        /// <summary> [1799] motorization is not in tracking mode.</summary>
        MOT_NOT_LOCK = 0x0707,
        /// <summary> [1800] motorization is not in spiral mode.</summary>
        MOT_NOT_SPIRAL = 0x0708,
        /// <summary> [1801] vertical encoder/motor error.</summary>
        MOT_V_ENCODER = 0x0709,
        /// <summary> [1802] horizontal encoder/motor error.</summary>
        MOT_HZ_ENCODER = 0x070A,
        /// <summary> [1803] horizontal and vertical encoder/motor error BMM 2304 0x900.</summary>
        MOT_HZ_V_ENCODER = 0x070B,

        #endregion MOT 0x700

        #region BMM 0x900

        /// <summary> [2305] Loading process already opened.</summary>
        BMM_XFER_PENDING = 0x0901,
        /// <summary> [2306] Transfer not opened.</summary>
        BMM_NO_XFER_OPEN = 0x0902,
        /// <summary> [2307] Unknown character set.</summary>
        BMM_UNKNOWN_CHARSET = 0x0903,
        /// <summary> [2308] Display module not present.</summary>
        BMM_NOT_INSTALLED = 0x0904,
        /// <summary> [2309] Character set already exists.</summary>
        BMM_ALREADY_EXIST = 0x0905,
        /// <summary> [2310] Character set cannot be deleted.</summary>
        BMM_CANT_DELETE = 0x0906,
        /// <summary> [2311] Memory cannot be allocated.</summary>
        BMM_MEM_ERROR = 0x0907,
        /// <summary> [2312] Character set still used.</summary>
        BMM_CHARSET_USED = 0x0908,
        /// <summary> [2313] Charset cannot be deleted or is protected.</summary>
        BMM_CHARSET_SAVED = 0x0909,
        /// <summary> [2314] Attempt to copy a character block outside the allocated memory.</summary>
        BMM_INVALID_ADR = 0x090A,
        /// <summary> [2315] Error during release of allocated memory.</summary>
        BMM_CANCELANDADR_ERROR = 0x090B,
        /// <summary> [2316] Number of bytes specified in header does not match the bytes read.</summary>
        BMM_INVALID_SIZE = 0x090C,
        /// <summary> [2317] Allocated memory could not be released.</summary>
        BMM_CANCELANDINVSIZE_ERROR = 0x090D,
        /// <summary> [2318] Max. number of character sets already loaded.</summary>
        BMM_ALL_GROUP_OCC = 0x090E,
        /// <summary> [2319] Layer cannot be deleted.</summary>
        BMM_CANT_DEL_LAYERS = 0x090F,
        /// <summary> [2320] Required layer does not exist.</summary>
        BMM_UNKNOWN_LAYER = 0x0910,
        /// <summary> [2321] Layer length exceeds maximum COM 3072 0xC00.</summary>
        BMM_INVALID_LAYERLEN = 0x0911,

        #endregion BMM 0x900

        #region COM 0xC00

        /// <summary> [3072] Initiate Extended Runtime Operation (ERO).</summary>
        COM_ERO = 0x0C00,
        /// <summary> [3073] Cannot encode arguments in client.</summary>
        COM_CANT_ENCODE = 0x0C01,
        /// <summary> [3074] Cannot decode results in client.</summary>
        COM_CANT_DECODE = 0x0C02,
        /// <summary> [3075] Hardware error while sending.</summary>
        COM_CANT_SEND = 0x0C03,
        /// <summary> [3076] Hardware error while receiving.</summary>
        COM_CANT_RECV = 0x0C04,
        /// <summary> [3077] Request timed out.</summary>
        COM_TIMEDOUT = 0x0C05,
        /// <summary> [3078] Packet format error.</summary>
        COM_WRONG_FORMAT = 0x0C06,
        /// <summary> [3079] Version mismatch between client and server.</summary>
        COM_VER_MISMATCH = 0x0C07,
        /// <summary> [3080] Cannot decode arguments in server.</summary>
        COM_CANT_DECODE_REQ = 0x0C08,
        /// <summary> [3081] Unknown RPC, procedure ID invalid.</summary>
        COM_PROC_UNAVAIL = 0x0C09,
        /// <summary> [3082] Cannot encode results in server.</summary>
        COM_CANT_ENCODE_REP = 0x0C0A,
        /// <summary> [3083] Unspecified generic system error.</summary>
        COM_SYSTEM_ERR = 0x0C0B,
        /// <summary> [3085] Unspecified error.</summary>
        COM_FAILED = 0x0C0D,
        /// <summary> [3086] Binary protocol not available.</summary>
        COM_NO_BINARY = 0x0C0E,
        /// <summary> [3087] Call interrupted.</summary>
        COM_INTR = 0x0C0F,
        /// <summary> [8] Protocol needs 8bit encoded characters.</summary>
        COM_REQUIRES_8DBITS = 0x0008,
        /// <summary> [3093] TRANSACTIONS ID mismatch error.</summary>
        COM_TR_ID_MISMATCH = 0x0C15,
        /// <summary> [3094] Protocol not recognizable.</summary>
        COM_NOT_GEOCOM = 0x0C16,
        /// <summary> [3095] (WIN) Invalid port address.</summary>
        COM_UNKNOWN_PORT = 0x0C17,
        /// <summary> [3099] ERO is terminating.</summary>
        COM_ERO_END = 0x0C1B,
        /// <summary> [3100] Internal error: data buffer overflow.</summary>
        COM_OVERRUN = 0x0C1C,
        /// <summary> [3101] Invalid checksum on server side received.</summary>
        COM_SRVR_RX_CHECKSUM_ERRR = 0x0C1D,
        /// <summary> [3102] Invalid checksum on client side received.</summary>
        COM_CLNT_RX_CHECKSUM_ERRR = 0x0C1E,
        /// <summary> [3103] (WIN) Port not available.</summary>
        COM_PORT_NOT_AVAILABLE = 0x0C1F,
        /// <summary> [3104] (WIN) Port not opened.</summary>
        COM_PORT_NOT_OPEN = 0x0C20,
        /// <summary> [3105] (WIN) Unable to find TPS.</summary>
        COM_NO_PARTNER = 0x0C21,
        /// <summary> [3106] Extended Runtime Operation could not be started.</summary>
        COM_ERO_NOT_STARTED = 0x0C22,
        /// <summary> [3107] Att to send cons reqs.</summary>
        COM_CONS_REQ = 0x0C23,
        /// <summary> [3108] TPS has gone to sleep. Wait and try again.</summary>
        COM_SRVR_IS_SLEEPING = 0x0C24,
        /// <summary> [3109] TPS has shut down. Wait and try again.</summary>
        COM_SRVR_IS_OFF = 0x0C25,
        /// <summary> [3110] No checksum in ASCII protocol available. AUT 8704 0x2200.</summary>
        COM_NO_CHECKSUM = 0x0C26,

        #endregion COM 0xC00

        #region AUT 0x2200

        /// <summary> [8704] Position not reached.</summary>
        AUT_TIMEOUT = 0x2200,
        /// <summary> [8705] Positioning not possible due to mounted EDM.</summary>
        AUT_DETENT_ERROR = 0x2201,
        /// <summary> [8706] Angle measurement error.</summary>
        AUT_ANGLE_ERROR = 0x2202,
        /// <summary> [8707] Motorisation error.</summary>
        AUT_MOTOR_ERROR = 0x2203,
        /// <summary> [8708] Position not exactly reached.</summary>
        AUT_INCACC = 0x2204,
        /// <summary> [8709] Deviation measurement error.</summary>
        AUT_DEV_ERROR = 0x2205,
        /// <summary> [8710] No target detected.</summary>
        AUT_NO_TARGET = 0x2206,
        /// <summary> [8711] Multiple target detected.</summary>
        AUT_MULTIPLE_TARGETS = 0x2207,
        /// <summary> [8712] Bad environment conditions.</summary>
        AUT_BAD_ENVIRONMENT = 0x2208,
        /// <summary> [8713] Error in target acquisition.</summary>
        AUT_DETECTOR_ERROR = 0x2209,
        /// <summary> [8714] Target acquisition not enabled.</summary>
        AUT_NOT_ENABLED = 0x220A,
        /// <summary> [8715] ATR-Calibration failed.</summary>
        AUT_CALACC = 0x220B,
        /// <summary> [8716] Target position not exactly reached.</summary>
        AUT_ACCURACY = 0x220C,
        /// <summary> [8717] Info: dist. measurement has been started.</summary>
        AUT_DIST_STARTED = 0x220D,
        /// <summary> [8718] external Supply voltage is too high.</summary>
        AUT_SUPPLY_TOO_HIGH = 0x220E,
        /// <summary> [8719] int. or ext. Supply voltage is too low.</summary>
        AUT_SUPPLY_TOO_LOW = 0x220F,
        /// <summary> [8720] working area not set.</summary>
        AUT_NO_WORKING_AREA = 0x2210,
        /// <summary> [8721] power search data array is filled.</summary>
        AUT_ARRAY_FULL = 0x2211,
        /// <summary> [8722] no data available KDM 12544 0x3100.</summary>
        AUT_NO_DATA = 0x2212,

        #endregion AUT 0x2200

        #region KDM 0x3100

        /// <summary> [12544] KDM device is not available. FTR 13056 0x3300.</summary>
        KDM_NOT_AVAILABLE = 0x3100,

        #endregion KDM 0x3100

        #region FTR 0x3300

        /// <summary> [13056] File access error.</summary>
        FTR_FILEACCESS = 0x3300,
        /// <summary> [13057] block number was not the expected one.</summary>
        FTR_WRONGFILEBLOCKNUMBER = 0x3301,
        /// <summary> [13058] not enough space on device to proceed uploading.</summary>
        FTR_NOTENOUGHSPACE = 0x3302,
        /// <summary> [13059] Rename of file failed.</summary>
        FTR_INVALIDINPUT = 0x3303,
        /// <summary> [13060] invalid parameter as input.</summary>
        FTR_MISSINGSETUP = 0x3304

        #endregion FTR 0x3300
    }

    public static class GRC_Resources
    {
        public static readonly Dictionary<GRC, string> Errors = new()
        {
            { GRC.EDM_DEV_NOT_INSTALLED, "Laserpointer is not implemented.\r\nLaserpointer is only available in theodolites which supports distance measurement without reflector." }
        };
    }

    #region Exceptions

    public class LeicaException : Exception
    {
        public GRC Code { get; }

        public LeicaException(GRC returnCode, string message) : base(message)
        {
            Code = returnCode;
        }
    }

    #endregion Exceptions

    /// <summary> Конвертер текстового представления значения.</summary>
    public class GrcJsonConverter : JsonConverter<GRC>
    {
        public override GRC Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            Enum.TryParse<GRC>(reader.GetString(), true, out var val) ? val : GRC.UNDEFINED;

        public override void Write(Utf8JsonWriter writer, GRC value, JsonSerializerOptions options) =>
            throw new NotImplementedException();
    }
}
