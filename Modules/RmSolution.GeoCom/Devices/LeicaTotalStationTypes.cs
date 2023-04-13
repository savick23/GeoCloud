//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: Различные вспомогательные классы, типы и перечисления для устройств Leica.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Devices.Leica
{
    #region Using
    using System;
    using System.ComponentModel.DataAnnotations;
    #endregion Using

    /// <summary> TPS Device Precision Class </summary>
    internal enum TPS_DEVICE_CLASS
    {
        /// <summary> TPS1000 family member, 1 mgon, 3" </summary>
        TPS_CLASS_1100 = 0,
        /// <summary> TPS1000 family member, 0.5 mgon, 1.5" </summary>
        TPS_CLASS_1700 = 1,
        /// <summary> TPS1000 family member, 0.3 mgon, 1" </summary>
        TPS_CLASS_1800 = 2,
        /// <summary> TPS2000 family member </summary>
        TPS_CLASS_5000 = 3,
        /// <summary> TPS2000 family member </summary>
        TPS_CLASS_6000 = 4,
        /// <summary> TPS1000 family member </summary>
        TPS_CLASS_1500 = 5,
        /// <summary> TPS2000 family member </summary>
        TPS_CLASS_2003 = 6,
        /// <summary> TPS5000 family member </summary>
        TPS_CLASS_5005 = 7,
        /// <summary> TPS5000 family member </summary>
        TPS_CLASS_5100 = 8,
        /// <summary> TPS1100 family member, 2"</summary>
        TPS_CLASS_1102 = 100,
        /// <summary> TPS1100 family member, 3" </summary>
        TPS_CLASS_1103 = 101,
        /// <summary> TPS1100 family member, 5" </summary>
        TPS_CLASS_1105 = 102,
        /// <summary> TPS1100 family member, 1" </summary>
        TPS_CLASS_1101 = 103,
        /// <summary> TPS1200 family member, 2" </summary>
        TPS_CLASS_1202 = 200,
        /// <summary> TPS1200 family member, 3" </summary>
        TPS_CLASS_1203 = 201,
        /// <summary> TPS1200 family member, 5" </summary>
        TPS_CLASS_1205 = 202,
        /// <summary> TPS1200 family member, 1" </summary>
        TPS_CLASS_1201 = 203,
        /// <summary> TS30,TM30 family member, 0.5" </summary>
        TPS_CLASS_Tx30 = 300,
        /// <summary> TS30,TM30 family member, 1" </summary>
        TPS_CLASS_Tx31 = 301
    }

    /// <summary> TPS Device Configuration Type </summary>
    internal enum TPS_DEVICE_TYPE
    {
        #region TPS1x00 common

        /// <summary> Theodolite without built-in EDM </summary>
        TPS_DEVICE_T = 0x00000,
        /// <summary> Motorized device </summary>
        TPS_DEVICE_MOT = 0x00004,
        /// <summary> Automatic Target Recognition </summary>
        TPS_DEVICE_ATR = 0x00008,
        /// <summary> Electronic Guide Light </summary>
        TPS_DEVICE_EGL = 0x00010,
        /// <summary> reserved (Database, not GSI) </summary>
        TPS_DEVICE_DB = 0x00020,
        /// <summary> Diode laser </summary>
        TPS_DEVICE_DL = 0x00040,
        /// <summary> Laser plumbed </summary>
        TPS_DEVICE_LP = 0x00080,

        #endregion TPS1x00 common

        #region TPS1000 specific

        /// <summary> tachymeter (TCW1) </summary>
        TPS_DEVICE_TC1 = 0x00001,
        /// <summary> tachymeter (TCW2) </summary>
        TPS_DEVICE_TC2 = 0x00002,

        #endregion TPS1000 specific

        #region TPS1100/TPS1200 specific

        /// <summary> tachymeter (TCW3) </summary>
        TPS_DEVICE_TC = 0x00001,
        /// <summary> tachymeter (TCW3 with red laser) </summary>
        TPS_DEVICE_TCR = 0x00002,
        /// <summary> Autocollimation lamp (used only PMU) </summary>
        TPS_DEVICE_ATC = 0x00100,
        /// <summary> Laserpointer </summary>
        TPS_DEVICE_LPNT = 0x00200,
        /// <summary> Reflectorless EDM with extended range </summary>
        TPS_DEVICE_RL_EXT = 0x00400,

        #endregion TPS1100/TPS1200 specific

        #region Pinpoint R100,R300

        /// <summary> Power Search </summary>
        TPS_DEVICE_PS = 0x00800,

        #endregion Pinpoint R100,R300

        #region TPSSim specific
        /// <summary> runs on Simulation, no Hardware </summary>
        TPS_DEVICE_SIM = 0x04000

        #endregion TPSSim specific
    }

    /// <summary> Reflectorless Class </summary>
    internal enum TPS_REFLESS_CLASS
    {
        /// <summary> None </summary>
        TPS_REFLESS_NONE = 0,
        /// <summary> Pinpoint R100 </summary>
        TPS_REFLESS_R100 = 1,
        /// <summary> Pinpoint R300 </summary>
        TPS_REFLESS_R300 = 2,
        /// <summary> Pinpoint R400 </summary>
        TPS_REFLESS_R400 = 3,
        /// <summary> Pinpoint R1000 </summary>
        TPS_REFLESS_R1000 = 4
    }

    /// <summary> TPS Device Configuration Type </summary>
    internal struct TPS_DEVICE
    {
        /// <summary> device precision class </summary>
        public TPS_DEVICE_CLASS Class;
        /// <summary> device configuration type </summary>
        public TPS_DEVICE_TYPE Type;
    };

    /// <summary> General Date and Time </summary>
    internal struct DATIME
    {
        public DATE_TYPE Date;
        public TIME_TYPE Time;
    }

    /// <summary> General Date </summary>
    internal struct DATE_TYPE
    {
        /// <summary> year </summary>
        public short Year;
        /// <summary> month in year 1..12 </summary>
        public byte Month; 
        /// <summary> day in month 1..31 </summary>
        public byte Day;
    }

    /// <summary> General Time </summary>
    internal struct TIME_TYPE
    {
        /// <summary> 24 hour per day 0..23 </summary>
        public byte Hour;
        /// <summary> minute 0..59 </summary>
        public byte Minute;
        /// <summary> seconds 0..59 </summary>
        public byte Second;
    }

    /// <summary> Power sources </summary>
    internal struct CSV_POWER_PATH
    {
        /// <summary> power source is external </summary>
        public byte CSV_EXTERNAL_POWER = 1;
        /// <summary> power source is the internal battery </summary>
        public byte CSV_INTERNAL_POWER = 2;

        public CSV_POWER_PATH()
        { }
    }

    /// <summary> On/off switch type </summary>
    public enum ON_OFF_TYPE
    {
        OFF = 0,
        ON = 1
    }

    /// <summary> Intensity of Electronic Guidelight </summary>
    public enum EDM_EGLINTENSITY_TYPE
    {
        EDM_EGLINTEN_OFF = 0,
        EDM_EGLINTEN_LOW = 1,
        EDM_EGLINTEN_MID = 2,
        EDM_EGLINTEN_HIGH = 3
    }

    public enum EDM_RETURN_CODE
    {
        /// <summary> [0] Function successfully completed.</summary>
        GRC_OK = 0x0000,
        /// <summary> [778] Device like EGL, DL is not installed.</summary>
        GRC_EDM_DEV_NOT_INSTALLED = 0x030A, 
        /// <summary> [3080] Cannot decode arguments in server.</summary>
        GRC_COM_CANT_DECODE_REQ = 0x0C08,
    }
}
