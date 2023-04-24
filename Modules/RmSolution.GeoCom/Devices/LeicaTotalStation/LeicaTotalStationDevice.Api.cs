//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: LeicaTotalStationDevice – Тахеометр Leica. Встроенные функции прибора.
// Документация: Leica TPS1200+ Leica TS30/TM30 GeoCOM Reference Manual Version 1.50 
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Devices
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.Metrics;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.PortableExecutable;
    using System.Runtime.Intrinsics.Arm;
    using System.Runtime.Intrinsics.X86;
    using System.Security.Principal;
    using System.Text;
    using RmSolution.Devices.Leica;
    using static System.Runtime.InteropServices.JavaScript.JSType;
    #endregion Using

    /// <summary> Communication; a module, which handles the basic communication parameters. Most of these functions relate to both client and server side.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class COMFAttribute : Attribute { }

    public partial class LeicaTotalStationDevice
    {
        #region Constants

        /// <summary> Standard intensity of beep expressed as a percentage.</summary>
        const short IOS_BEEP_STDINTENS = 100;
        /// <summary> Prism name length.</summary>
        const int BAP_PRISMNAME_LEN = 16;

        /// <summary> Number of axis.</summary>
        const int MOT_AXES = 2;

        const double MIN_TOL = 3.141592654e-05;
        const int MOT_HZ_AXLE = 0;
        const int MOT_V_AXLE = 1;

        #endregion Constants

        #region AUTOMATION (AUT CONF)

        /// <summary> Reading the current setting for the positioning tolerances.</summary>
        /// <remarks> This command reads the current setting for the positioning tolerances of the Hz- and V- instrument axis.<br/>This command is valid for motorized instruments only.</remarks>
        /// <example> mod3 call 000001 AUT_ReadTol </example>
        [COMF]
        public AUT_POSTOL? AUT_ReadTol()
        {
            var resp = Request(RequestString("%R1Q,9008:"));
            switch (resp.ReturnCode)
            {
                case GRC.NA:
                    throw new LeicaException(resp.ReturnCode, "GeoCOM Robotic license key not available.");
                case GRC.OK:
                    if (resp.Values.Length == MOT_AXES)
                        return new AUT_POSTOL()
                        {
                            PosTol = new double[] { double.Parse(resp.Values[MOT_HZ_AXLE].ToString()), double.Parse(resp.Values[MOT_V_AXLE].ToString()) }
                        };
                    break;
            };
            return null;
        }

        /// <summary> Setting the positioning tolerances.</summary>
        /// <remarks> This command sets new values for the positioning tolerances of the Hz- and V- instrument axes. This command is valid for motorized instruments only.<br/>The tolerances must be in the range of 1[cc] ( =1.57079 E-06[rad] ) to 100[cc] ( =1.57079 E-04[rad]).<br/><br/>
        /// <b>Note:</b> The maximum resolution of the angle measurement system depends on the instrument accuracy class. If smaller positioning tolerances are required, the positioning time can increase drastically.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_SetTol 0 0 </example>
        [COMF]
        public bool AUT_SetTol(double toleranceHz, double toleranceV)
        {
            var resp = Request(RequestString("%R1Q,9007:", toleranceHz, toleranceV));
            return (resp.ReturnCode) switch
            {
                GRC.NA => throw new LeicaException(resp.ReturnCode, "GeoCOM Robotic license key not available."),
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Invalid parameters. One or both tolerance values not within the boundaries(1.57079E-06[rad] =1[cc]to 1.57079E-04[rad] =100[cc])."),
                GRC.MOT_UNREADY => throw new LeicaException(resp.ReturnCode, "Instrument has no motorization."),
                _ => Successful(resp.ReturnCode)
            };
        }

        /// <summary> Setting the positioning tolerances.</summary>
        /// <remarks> This command sets new values for the positioning tolerances of the Hz- and V- instrument axes. This command is valid for motorized instruments only.<br/>The tolerances must be in the range of 1[cc] ( =1.57079 E-06[rad] ) to 100[cc] ( =1.57079 E-04[rad]).<br/><br/>
        /// <b>Note:</b> The maximum resolution of the angle measurement system depends on the instrument accuracy class. If smaller positioning tolerances are required, the positioning time can increase drastically.</remarks>
        /// <param name="tolPar"> The values for the positioning tolerances in Hz and V direction[rad].</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_SetTol </example>
        [COMF]
        public bool AUT_SetTol(AUT_POSTOL tolPar) => AUT_SetTol(tolPar.PosTol[MOT_HZ_AXLE], tolPar.PosTol[MOT_V_AXLE]);

        /// <summary> Reading the current timeout setting for positioning.</summary>
        /// <remarks> This command reads the current setting for the positioning time out (maximum time to perform positioning).</remarks>
        /// <returns> The values for the positioning time out in Hz and V direction [sec].</returns>
        /// <example> mod3 call 000001 AUT_ReadTimeout </example>
        [COMF]
        public AUT_TIMEOUT? AUT_ReadTimeout()
        {
            var resp = Request(RequestString("%R1Q,9012:"));
            switch (resp.ReturnCode)
            {
                case GRC.NA:
                    throw new LeicaException(resp.ReturnCode, "GeoCOM Robotic license key not available.");
                case GRC.OK:
                    if (resp.Values.Length == MOT_AXES)
                        return new AUT_TIMEOUT()
                        {
                            PosTimeout = new double[] { double.Parse(resp.Values[MOT_HZ_AXLE].ToString()), double.Parse(resp.Values[MOT_V_AXLE].ToString()) }
                        };
                    break;
            };
            return null;
        }

        /// <summary> Setting the timeout for positioning.</summary>
        /// <remarks> This command set the positioning timeout (set maximum time to perform a positioning). The timeout is reset on 7[sec] after each power on.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_SetTimeout </example>
        [COMF]
        public bool AUT_SetTimeout(double timeoutHz, double timeoutV)
        {
            var resp = Request(RequestString("%R1Q,9011:", timeoutHz, timeoutV));
            return (resp.ReturnCode) switch
            {
                GRC.NA => throw new LeicaException(resp.ReturnCode, "GeoCOM Robotic license key not available."),
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "One or both time out values not within the boundaries (7[sec] to 60[sec])."),
                _ => Successful(resp.ReturnCode)
            };
        }

        /// <summary> Setting the timeout for positioning.</summary>
        /// <remarks> This command set the positioning timeout (set maximum time to perform a positioning). The timeout is reset on 7[sec] after each power on.</remarks>
        /// <param name="timeoutPar"> The values for the positioning timeout in Hz and V direction [s]. Valid values are between 7 [sec] and 60 [sec].</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_SetTimeout </example>
        [COMF]
        public bool AUT_SetTimeout(AUT_TIMEOUT timeoutPar) => AUT_SetTimeout(timeoutPar.PosTimeout[MOT_HZ_AXLE], timeoutPar.PosTimeout[MOT_V_AXLE]);

        /// <summary> Turning the telescope to a specified position.</summary>
        /// <remarks> This procedure turns the telescope absolute to the in Hz and V specified position, taking tolerance settings for positioning(see AUT_POSTOL) into account.Any active control function is terminated by this function call.<br/><br/>
        /// If the position mode is set to normal (PosMode = AUT_NORMAL) it is assumed that the current value of the compensator measurement is valid.Positioning precise(PosMode = AUT_PRECISE) forces a new compensator measurement at the specified position and includes this information for positioning.<br/><br/>
        /// If ATR mode is activated and the ATR mode is set to AUT_TARGET, the instrument tries to position onto a target in the destination area.<br/><br/>
        /// If LOCK mode is activated and the ATR mode is set to AUT_TARGET, the instrument tries to lock onto a target in the destination area.</remarks>
        /// <param name="Hz"> Horizontal (instrument) position [rad].</param>
        /// <param name="V"> Vertical (telescope) position [rad].</param>
        /// <param name="POSMode"> Position mode:<br/>
        /// AUT_NORMAL: (default) uses the current value of the compensator(no compensator measurement while positioning). For positioning distances >25GON AUT_NORMAL might tend to inaccuracy.<br/>
        /// AUT_PRECISE: tries to measure exact inclination of target. Tend to longer position time (check AUT_TIMEOUT and/or COM-time out if necessary).<br/>
        /// AUT_Fast: for TS30 / TM30 instruments, positions with the last valid inclination and an increased positioning tolerance.Suitable in combination with ATRMode AUT_Target.</param>
        /// <param name="ATRMode">Mode of ATR:<br/>
        /// AUT_POSITION: (default) conventional position using values Hz and V.<br/>
        /// AUT_TARGET: tries to position onto a target in the destination area.This mode is only possible if ATR exists and is activated.</param>
        /// <param name="dummy"> It’s reserved for future use, set bDummy always to FALSE.</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_MakePositioning </example>
        [COMF]
        public bool AUT_MakePositioning(double Hz, double V, AUT_POSMODE POSMode, AUT_ATRMODE ATRMode, bool dummy = false)
        {
            var resp = Request(RequestString("%R1Q,9027:", Hz, V, POSMode, ATRMode, dummy));
            return (resp.ReturnCode) switch
            {
                GRC.NA => throw new LeicaException(resp.ReturnCode, "GeoCOM Robotic license key not available."),
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Invalid parameter (e.g. no valid position)."),
                GRC.AUT_TIMEOUT => throw new LeicaException(resp.ReturnCode, "Time out while positioning of one or both axes. (perhaps increase AUT time out, see AUT_SetTimeout)."),
                GRC.AUT_MOTOR_ERROR => throw new LeicaException(resp.ReturnCode, "Instrument has no ‘motorization’."),
                GRC.TMC_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "Error with angle measurement occurs if the instrument is not levelled properly during positioning."),
                GRC.ABORT => throw new LeicaException(resp.ReturnCode, "Function aborted."),
                GRC.COM_TIMEDOUT => throw new LeicaException(resp.ReturnCode, "Communication timeout. (perhaps increase COM timeout, see COM_SetTimeout)."),
                // Additionally with position mode AUT_TARGET -->
                GRC.AUT_NO_TARGET => throw new LeicaException(resp.ReturnCode, "No target found."),
                GRC.AUT_MULTIPLE_TARGETS => throw new LeicaException(resp.ReturnCode, "Multiple targets found."),
                GRC.AUT_BAD_ENVIRONMENT => throw new LeicaException(resp.ReturnCode, "Inadequate environment conditions."),
                GRC.AUT_ACCURACY => throw new LeicaException(resp.ReturnCode, "Inexact fine position, repeat positioning."),
                GRC.AUT_DEV_ERROR => throw new LeicaException(resp.ReturnCode, "During the determination of the angle deviation error detected, repeat positioning."),
                GRC.AUT_NOT_ENABLED => throw new LeicaException(resp.ReturnCode, "ATR mode not enabled, enable ATR mode."),

                _ => Successful(resp.ReturnCode)
            };
        }

        /// <summary> Turning the telescope to the other face.</summary>
        /// <remarks> This procedure turns the telescope to the other face. If another function is active, for example locking onto a target, then this function is terminated and the procedure is executed.<br/><br/>
        /// If the position mode is set to normal (PosMode = AUT_NORMAL) it is allowed that the current value of the compensator measurement is inexact. Positioning precise (PosMode = AUT_PRECISE) forces a new compensator measurement.If this measurement is not possible, the position does not take place.<br/><br/>
        /// If <i>ATR</i> mode is activated and the ATR mode is set to AUT_TARGET, the instrument tries to position onto a target in the destination area.<br/><br/>
        /// If <i>LOCK</i> mode is activated and the ATR mode is set to AUT_TARGET, the instrument tries to lock onto a target in the destination area.</remarks>
        /// <param name="POSMode"> Position mode:<br/>
        /// AUT_NORMAL: uses the current value of the compensator. For positioning distances >25GON AUT_NORMAL might tend to inaccuracy.<br/>
        /// AUT_PRECISE: tries to measure exact inclination of target. Tends to long position time (check AUT_TIMEOUT and/or COM-time out if necessary).</param>
        /// <param name="ATRMode"> Mode of ATR:<br/>
        /// AUT_POSITION: conventional position to other face.<br/>
        /// AUT_TARGET: tries to position onto a target in the destination area.This set is only possible if ATR exists and is activated.</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_ChangeFace </example>
        [COMF]
        public bool AUT_ChangeFace(AUT_POSMODE POSMode, AUT_ATRMODE ATRMode, bool dummy = false)
        {
            var resp = Request(RequestString("%R1Q,9028:", POSMode, ATRMode, dummy));
            return (resp.ReturnCode) switch
            {
                GRC.NA => throw new LeicaException(resp.ReturnCode, "GeoCOM Robotic license key not available."),
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Invalid parameter."),
                GRC.AUT_TIMEOUT => throw new LeicaException(resp.ReturnCode, "Timeout while positioning of one or both axes. (perhaps increase AUT timeout, see AUT_SetTimeout)."),
                GRC.AUT_MOTOR_ERROR => throw new LeicaException(resp.ReturnCode, "Instrument has no ‘motorization’."),
                GRC.TMC_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "Error with angle measurement occurs if the instrument is not levelled properly during positioning."),
                GRC.FATAL => throw new LeicaException(resp.ReturnCode, "Fatal error."),
                GRC.ABORT => throw new LeicaException(resp.ReturnCode, "Function aborted."),
                GRC.COM_TIMEDOUT => throw new LeicaException(resp.ReturnCode, "Communication timeout. (perhaps increase COM timeout,\r\nsee COM_SetTimeout)."),
                // Additionally with position mode AUT_TARGET -->
                GRC.AUT_NO_TARGET => throw new LeicaException(resp.ReturnCode, "No target found."),
                GRC.AUT_MULTIPLE_TARGETS => throw new LeicaException(resp.ReturnCode, "Multiple targets found."),
                GRC.AUT_BAD_ENVIRONMENT => throw new LeicaException(resp.ReturnCode, "Inadequate environment conditions."),
                GRC.AUT_ACCURACY => throw new LeicaException(resp.ReturnCode, "Inexact fine position, repeat positioning."),
                GRC.AUT_DEV_ERROR => throw new LeicaException(resp.ReturnCode, "During the determination of the angle deviation error\r\ndetected, repeat change face."),
                GRC.AUT_NOT_ENABLED => throw new LeicaException(resp.ReturnCode, "ATR mode not enabled, enable ATR mode."),

                _ => Successful(resp.ReturnCode)
            };
        }

        /// <summary> Automatic target positioning.</summary>
        /// <remarks> This procedure precisely positions the telescope crosshairs onto the target prism and measures the ATR Hz and V deviations.If the target is not within the visible area of the ATR sensor (Field of View) a target search will be executed.The target search range is limited by the parameter dSrchV in V- direction and by parameter dSrchHz in Hz - direction.If no target found the instrument turns back to the initial start position.<br/><br/>
        /// A current Fine Adjust LockIn towards a target is terminated by this procedure call. After positioning, the lock mode is active.The timeout of this operation is set to 5s, regardless of the general position timeout settings.The positioning tolerance is depends on the previously set up the fine adjust mode (see AUT_SetFineAdjustMoed and AUT_GetFineAdjustMode).<br/><br/>
        /// Tolerance settings(with AUT_SetTol and AUT_ReadTol) have no influence to this operation.The tolerance settings as well as the ATR measure precision depends on the instrument’s class and the used EDM measure mode(The EDM measure modes are handled by the subsystem TMC).</remarks>
        /// <param name="srchHz"> Search range Hz-axis [rad].</param>
        /// <param name="srchV"> Search range V-axis [rad].</param>
        /// <param name="dummy"> It’s reserved for future use, set bDummy always to FALSE.</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_FineAdjust </example>
        [COMF]
        public bool AUT_FineAdjust(double srchHz, double srchV, bool dummy = false)
        {
            var resp = Request(RequestString("%R1Q,9037:", srchHz, srchV, dummy));
            return (resp.ReturnCode) switch
            {
                GRC.NA => throw new LeicaException(resp.ReturnCode, "GeoCOM Robotic license key not available."),
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Invalid parameter."),
                GRC.AUT_TIMEOUT => throw new LeicaException(resp.ReturnCode, "Timeout while positioning of one or both axes. The position fault lies above 100[cc]. (perhaps increase AUT timeout, see AUT_SetTimeout)."),
                GRC.AUT_MOTOR_ERROR => throw new LeicaException(resp.ReturnCode, "Instrument has no ‘motorization’."),
                GRC.FATAL => throw new LeicaException(resp.ReturnCode, "Fatal error."),
                GRC.ABORT => throw new LeicaException(resp.ReturnCode, "Function aborted."),
                GRC.AUT_NO_TARGET => throw new LeicaException(resp.ReturnCode, "No target found."),
                GRC.AUT_MULTIPLE_TARGETS => throw new LeicaException(resp.ReturnCode, "Multiple targets found."),
                GRC.AUT_BAD_ENVIRONMENT => throw new LeicaException(resp.ReturnCode, "Inadequate environment conditions."),
                GRC.AUT_DEV_ERROR => throw new LeicaException(resp.ReturnCode, "During the determination of the angle deviation error detected, repeat fine positioning."),
                GRC.AUT_DETECTOR_ERROR => throw new LeicaException(resp.ReturnCode, "Error in target acquisition."),
                GRC.COM_TIMEDOUT => throw new LeicaException(resp.ReturnCode, "Communication time out. (perhaps increase COM timeout, see COM_SetTimeout)."),
                _ => Successful(resp.ReturnCode)
            };
        }

        /// <summary> Performing an automatic target search.</summary>
        /// <remarks> This procedure performs an automatically target search within a given area. The search is terminated once the prism appears in the field of view of the ATR sensor.If no prism is found within the specified area, the instrument turns back to the initial start position.For an exact positioning onto the prism centre, use fine adjust (see AUT_FineAdjust) afterwards.<br/><br/>
        /// <b>Note:</b> If you expand the search range of the function AUT_FineAdjust, then you have a target search and a fine positioning in one function.</remarks>
        /// <param name="Hz_Area"> Horizontal search region [rad].</param>
        /// <param name="V_Area"> Vertical search region [rad].</param>
        /// <param name="dummy"> It’s reserved for future use, set bDummy always to FALSE.</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_Search </example>
        [COMF]
        public bool AUT_Search(double Hz_Area, double V_Area, bool dummy = false)
        {
            var resp = Request(RequestString("%R1Q,9029:", Hz_Area, V_Area, dummy));
            return (resp.ReturnCode) switch
            {
                GRC.NA => throw new LeicaException(resp.ReturnCode, "GeoCOM Robotic license key not available."),
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Invalid parameter."),
                GRC.AUT_MOTOR_ERROR => throw new LeicaException(resp.ReturnCode, "Instrument has no ‘motorization’."),
                GRC.FATAL => throw new LeicaException(resp.ReturnCode, "Fatal error."),
                GRC.ABORT => throw new LeicaException(resp.ReturnCode, "Function aborted."),
                GRC.AUT_NO_TARGET => throw new LeicaException(resp.ReturnCode, "No target found."),
                GRC.AUT_BAD_ENVIRONMENT => throw new LeicaException(resp.ReturnCode, "Inadequate environment conditions."),
                GRC.AUT_DETECTOR_ERROR => throw new LeicaException(resp.ReturnCode, "AZE error, at repeated occur call service."),
                GRC.COM_TIMEDOUT => throw new LeicaException(resp.ReturnCode, "Communication time out. (perhaps increase COM timeout, see COM_SetTimeout)."),
                _ => Successful(resp.ReturnCode)
            };
        }

        /// <summary> Getting the fine adjust positioning mode.</summary>
        /// <remarks> This function returns the current activated fine adjust positioning mode. This command is valid for all instruments, but has only effects for instruments equipped with ATR.</remarks>
        /// <returns> Current fine adjust positioning mode.</returns>
        /// <example> mod3 call 000001 AUT_GetFineAdjustMode </example>
        [COMF]
        public AUT_ADJMODE? AUT_GetFineAdjustMode()
        {
            var resp = Request(RequestString("%R1Q,9030:"));
            switch (resp.ReturnCode)
            {
                case GRC.NA:
                    throw new LeicaException(resp.ReturnCode, "GeoCOM Robotic license key not available.");
                case GRC.OK:
                    return (AUT_ADJMODE)resp.Values[0];
            };
            return null;
        }

        #endregion AUTOMATION (AUT CONF)

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
            return Successful(resp.ReturnCode);
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
            return Successful(resp.ReturnCode);
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
            if (prismName.Length > BAP_PRISMNAME_LEN)
                throw new LeicaException(GRC.PRISM_NAME_LEN_INVALID, "Длинна наименования призмы не должна превышать " + BAP_PRISMNAME_LEN + " символов.");

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
                _ => Successful(resp.ReturnCode)
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
                _ => Successful(resp.ReturnCode)
            };
        }

        /// <summary> Measuring Hz,V angles and a single distance.</summary>
        /// <remarks> This function measures angles and a single distance depending on the mode DistMode. Note that this function is not suited for continuous measurements(LOCK mode and TRK mode). This command uses the current automation settings.</remarks>
        /// <param name="distMode"> BAP_DEF_DIST uses the predefined distance measurement program as defined in BAP_SetMeasPrg.</param>
        /// <param name="Hz"> [out] Horizontal angle [rad]x, depends on DistMode.</param>
        /// <param name="V"> [out] Vertical angle [rad]x, depends on DistMode.</param>
        /// <param name="dist"> [out] Slopedistance [m]x, depends on DistMode.</param>
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
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 BAP_SearchTarget </example>
        [COMF]
        public bool BAP_SearchTarget(bool dummy = false)
        {
            var resp = Request(RequestString("%R1Q,17020:", dummy));
            return (resp.ReturnCode) switch
            {
                GRC.AUT_BAD_ENVIRONMENT => throw new LeicaException(resp.ReturnCode, "Bad Environment conditions."),
                GRC.AUT_DEV_ERROR => throw new LeicaException(resp.ReturnCode, "Deviation measurement error."),
                GRC.AUT_ACCURACY => throw new LeicaException(resp.ReturnCode, "Position not exactly reached."),
                GRC.AUT_MOTOR_ERROR => throw new LeicaException(resp.ReturnCode, "Motorization error."),
                GRC.AUT_MULTIPLE_TARGETS => throw new LeicaException(resp.ReturnCode, "Multiple targets detected."),
                GRC.AUT_NO_TARGET => throw new LeicaException(resp.ReturnCode, "No target detected."),
                GRC.AUT_TIMEOUT => throw new LeicaException(resp.ReturnCode, "Time out, no target found."),
                GRC.ABORT => throw new LeicaException(resp.ReturnCode, "Error, searching aborted."),
                GRC.FATAL => throw new LeicaException(resp.ReturnCode, "Fatal Error."),
                _ => Successful(resp.ReturnCode)
            };
        }

        /// <summary> Getting the current ATR low vis mode.</summary>
        /// <remarks> Gets the current low vis mode.</remarks>
        /// <example> mod3 call 000001 BAP_GetATRSetting </example>
        [COMF]
        public BAP_ATRSETTING? BAP_GetATRSetting() => GetComf<BAP_ATRSETTING>("%R1Q,17034:");

        /// <summary> Setting the current ATR low vis mode.</summary>
        /// <remarks> Sets the current low vis mode.</remarks>
        /// <param name="ATRSetting"> ATR low vis mode.</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 BAP_SetATRSetting </example>
        [COMF]
        public bool BAP_SetATRSetting(BAP_ATRSETTING ATRSetting) => SetComf("%R1Q,17035:", ATRSetting);

        /// <summary> Getting the reduced ATR field of view.</summary>
        /// <remarks> Get reduced ATR field of view mode.</remarks>
        /// <returns> ON: ATR uses reduced field of view(about 1/9).<br/>OFF: ATR uses full field of view.</returns>
        /// <example> mod3 call 000001 BAP_GetRedATRFov </example>
        [COMF]
        public ON_OFF_TYPE? BAP_GetRedATRFov() => GetComf<ON_OFF_TYPE>("%R1Q,17036:");

        /// <summary> Setting the reduced ATR field of view.</summary>
        /// <remarks> Set reduced ATR field of view mode.</remarks>
        /// <param name="redFov"> true/false.</param>
        /// <example> mod3 call 000001 BAP_SetRedATRFov </example>
        [COMF]
        public bool BAP_SetRedATRFov(ON_OFF_TYPE redFov) => SetComf("%R1Q,17037:", redFov);

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
    }
}
