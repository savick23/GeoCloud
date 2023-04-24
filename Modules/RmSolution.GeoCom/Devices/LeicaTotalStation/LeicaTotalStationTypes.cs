//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: Различные вспомогательные классы, типы и перечисления для устройств Leica.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Devices.Leica
{
    #region Using
    using System;
    #endregion Using

    #region Enums

    /// <summary> Stop Mode </summary>
    public enum COM_TPS_STOP_MODE : long
    {
        /// <summary> Power down instrument </summary>
        COM_TPS_STOP_SHUT_DOWN = 0,
        /// <summary> Not supported by TPS1200 </summary>
        COM_TPS_STOP_SLEEP = 1
    }

    /// <summary> Start Mode </summary>
    public enum COM_TPS_STARTUP_MODE : long
    {
        /// <summary> Not supported by TPS1200 </summary>
        COM_TPS_STARTUP_LOCAL = 0,
        /// <summary> RPC’s enabled, online mode </summary>
        COM_TPS_STARTUP_REMOTE = 1
    }

    /// <summary> Target types.</summary>
    public enum BAP_TARGET_TYPE : long
    {
        /// <summary> With reflector </summary>
        BAP_REFL_USE = 0,
        /// <summary> Without reflector </summary>
        BAP_REFL_LESS = 1
    }

    /// <summary> On/off switch type </summary>
    public enum ON_OFF_TYPE : long
    {
        OFF = 0,
        ON = 1
    }

    /// <summary> Intensity of Electronic Guidelight </summary>
    public enum EDM_EGLINTENSITY_TYPE : long
    {
        OFF = 0,
        LOW = 1,
        MID = 2,
        HIGH = 3
    }

    /// <summary> Reflector type definition.</summary>
    public enum BAP_REFLTYPE : long
    {
        /// <summary> reflector not defined </summary>
        BAP_REFL_UNDEF = 0,
        /// <summary> reflector prism </summary>
        BAP_REFL_PRISM = 1,
        /// <summary> reflector tape </summary>
        BAP_REFL_TAPE = 2
    };

    /// <summary> Prism types.</summary>
    public enum BAP_PRISMTYPE : long
    {
        /// <summary> Leica Circular Prism </summary>
        BAP_PRISM_ROUND = 0,
        /// <summary> Leica Mini Prism </summary>
        BAP_PRISM_MINI = 1,
        /// <summary> Leica Reflector Tape </summary>
        BAP_PRISM_TAPE = 2,
        /// <summary> Leica 360º Prism </summary>
        BAP_PRISM_360 = 3,
        /// <summary> not supported by TPS1200 </summary>
        BAP_PRISM_USER1 = 4,
        /// <summary> not supported by TPS1200 </summary>
        BAP_PRISM_USER2 = 5,
        /// <summary> not supported by TPS1200 </summary>
        BAP_PRISM_USER3 = 6,
        /// <summary> Leica Mini 360º Prism </summary>
        BAP_PRISM_360_MINI = 7,
        /// <summary> Leica Mini Zero Prism </summary>
        BAP_PRISM_MINI_ZERO = 8,
        /// <summary> User Defined Prism </summary>
        BAP_PRISM_USER = 9,
        /// <summary> Leica HDS Target </summary>
        BAP_PRISM_NDS_TAPE = 10,
        /// <summary> GRZ121 360º Prism for Machine Guidance </summary>
        BAP_PRISM_GRZ121_ROUND = 11,
        /// <summary> MPR122 360º Prism for Machine Guidance </summary>
        BAP_PRISM_MA_MPR122 = 12
    };

    /// <summary> Distance measurement programs.</summary>
    public enum BAP_USER_MEASPRG : long
    {
        /// <summary> IR Standard </summary>
        BAP_SINGLE_REF_STANDARD = 0,
        /// <summary> IR Fast </summary>
        BAP_SINGLE_REF_FAST = 1,
        /// <summary> LO Standard </summary>
        BAP_SINGLE_REF_VISIBLE = 2,
        /// <summary> RL Standard </summary>
        BAP_SINGLE_RLESS_VISIBLE = 3,
        /// <summary> IR Tracking </summary>
        BAP_CONT_REF_STANDARD = 4,
        /// <summary> not supported by TPS1200 </summary>
        BAP_CONT_REF_FAST = 5, // 
        /// <summary> RL Fast Tracking </summary>
        BAP_CONT_RLESS_VISIBLE = 6,
        /// <summary> IR Average </summary>
        BAP_AVG_REF_STANDARD = 7,
        /// <summary> LO Average </summary>
        BAP_AVG_REF_VISIBLE = 8,
        /// <summary> RL Average </summary>
        BAP_AVG_RLESS_VISIBLE = 9,
        /// <summary> IR Synchro Tracking </summary>
        BAP_CONT_REF_SYNCHRO = 10,
        /// <summary> IR Precise (TS30,TM30) </summary>
        BAP_SINGLE_REF_PRECISE = 11
    };

    /// <summary> Measurement Modes.</summary>
    public enum BAP_MEASURE_PRG
    {
        /// <summary> no measurements, take last one </summary>
        BAP_NO_MEAS = 0,
        /// <summary> no dist. measurement, angles only </summary>
        BAP_NO_DIST = 1,
        /// <summary> default distance measurements, pre-defined using BAP_SetMeasPrg </summary>
        BAP_DEF_DIST = 2,
        /// <summary> clear distances </summary>
        BAP_CLEAR_DIST = 5,
        /// <summary> stop tracking </summary>
        BAP_STOP_TRK = 6
    }

    /// <summary> TPS Device Precision Class </summary>
    public enum TPS_DEVICE_CLASS : long
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
    public enum TPS_DEVICE_TYPE : long
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
    public enum TPS_REFLESS_CLASS : long
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

    /// <summary> ATR low vis mode definition.</summary>
    public enum BAP_ATRSETTING : long
    {
        /// <summary> ATR is using no special flags or modes.</summary>
        BAP_ATRSET_NORMAL = 0,
        /// <summary> ATR low vis mode on.</summary>
        BAP_ATRSET_LOWVIS_ON = 1,
        /// <summary> ATR low vis mode always on.</summary>
        BAP_ATRSET_LOWVIS_AON = 2,
        /// <summary> ATR high reflectivity mode on.</summary>
        BAP_ATRSET_SRANGE_ON = 3,
        /// <summary> ATR high reflectivity mode always on.</summary>
        BAP_ATRSET_SRANGE_AON = 4
    }

    #endregion Enums

    #region Structures

    /// <summary> TPS Device Configuration Type </summary>
    public struct TPS_DEVICE
    {
        /// <summary> device precision class </summary>
        public TPS_DEVICE_CLASS Class;
        /// <summary> device configuration type </summary>
        public TPS_DEVICE_TYPE Type;
    };

    /// <summary> General Date and Time </summary>
    public struct DATIME
    {
        public DATE_TYPE Date;
        public TIME_TYPE Time;
    }

    /// <summary> General Date </summary>
    public struct DATE_TYPE
    {
        /// <summary> year </summary>
        public short Year;
        /// <summary> month in year 1..12 </summary>
        public byte Month; 
        /// <summary> day in month 1..31 </summary>
        public byte Day;
    }

    /// <summary> General Time </summary>
    public struct TIME_TYPE
    {
        /// <summary> 24 hour per day 0..23 </summary>
        public byte Hour;
        /// <summary> minute 0..59 </summary>
        public byte Minute;
        /// <summary> seconds 0..59 </summary>
        public byte Second;
    }

    /// <summary> Power sources </summary>
    public struct CSV_POWER_PATH
    {
        /// <summary> power source is external </summary>
        public byte CSV_EXTERNAL_POWER = 1;
        /// <summary> power source is the internal battery </summary>
        public byte CSV_INTERNAL_POWER = 2;

        public CSV_POWER_PATH()
        { }
    }

    /// <summary> Prism definition.</summary>
    public struct BAP_PRISMDEF
    {
        /// <summary> Name (16 characters).</summary>
        public string Name;
        /// <summary> Prism correction.</summary>
        public double AddConst;
        /// <summary> Reflector type.</summary>
        public BAP_REFLTYPE ReflType;
        /// <summary> Name of creator.</summary>
        public string Creator;
    }

    #endregion Structures
}
