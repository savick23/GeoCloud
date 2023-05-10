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
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Metadata;
    using System.Runtime.Intrinsics.X86;
    using System.Text.Json;
    using RmSolution.Devices.Leica;
    using static System.Collections.Specialized.BitVector32;
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

        /// <summary> Карта доступных функций устройства.</summary>
        static GrcFunctionCollection _api = JsonSerializer.Deserialize<GrcFunctionCollection>(Assembly.GetAssembly(typeof(LeicaTotalStationDevice)).GetManifestResourceStream("RmSolution.GeoCom.Devices.LeicaTotalStation.LeicaTotalStationDevice.json"), new JsonSerializerOptions()
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,

        });

        #endregion Constants

        #region GENERAL GEOCOM FUNCTIONS

        /// <summary> Getting the double precision setting.</summary>
        /// <remarks> This function returns the precision - number of digits to the right of the decimal point - when double floatingpoint values are transmitted.The usage of this function is only meaningful if the communication is set to ASCII transmission mode.Precision is equal in both transmission directions.In the case of an ASCII request, the precision of the server side will be returned.</remarks>
        /// <returns> Number of digits to the right of the decimal point. 0 <= digit <= 15 </returns>
        /// <example> mod3 call 000001 COM_GetDoublePrecision </example>
        [COMF]
        public int COM_GetDoublePrecision() =>
            Call("%R1Q,108:", (resp) => Successful(resp.ReturnCode) && resp.Values.Length == 1 ? (int)(long)resp.Values[0] : default);

        /// <summary> Setting the double precision setting.</summary>
        /// <remarks> This function sets the precision - number of digits to the right of the decimal - when double floating-point values are transmitted.The TPS’ system software always calculates with highest possible precision. The default precision is fifteen digits. However, if this precision is not needed then transmission of double data (ASCII transmission) can be speeded up by choosing a lower precision. Especially when many double values are transmitted this may enhance the operational speed.The usage of this function is only meaningful if the communication is set to ASCII transmission mode.In the case of an ASCII request, the precision of the server side will be set.Notice that trailing Zeros will not be sent by the server and values may be rounded. E.g. if precision is set to 3 and the exact value is 1.99975 the resulting value will be 2.0<br/><b>Note: </b>With this function it is possible to decrease the accuracy of the delivered values.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 COM_SetDoublePrecision </example>
        [COMF]
        public bool COM_SetDoublePrecision(int digit)
        {
            if (digit < 0 || digit > 15) throw new LeicaException(GRC.IVPARAM, "Invalid parameter detected. Result unspecified. 0 > Digits > 15");

            return Call("%R1Q,107:", digit, (resp) => Successful(resp.ReturnCode));
        }

        #endregion GENERAL GEOCOM FUNCTIONS

        #region ALT USER (AUS CONF)

        /// <summary> Getting the status of the ATR mode.</summary>
        /// <remarks> Get the current status of the ATR mode on automated instrument models. This command does not indicate whether the ATR has currently acquired a prism.Note the difference between GetUserATR and GetUserLOCK state.</remarks>
        /// <returns> State of the ATR mode.</returns>
        /// <example> mod3 call 000001 AUS_GetUserAtrState </example>
        [COMF]
        public ON_OFF_TYPE? AUS_GetUserAtrState() =>
            Call("%R1Q,18006:", (resp) => (resp.ReturnCode) switch
            {
                GRC.NOT_IMPL => throw new LeicaException(resp.ReturnCode, "ATR not available; no automated instrument."),
                _ => Successful(resp.ReturnCode) && resp.Values.Length == 1 ? (ON_OFF_TYPE)resp.Values[0] : default
            });

        /// <summary> Getting the status of the ATR mode.</summary>
        /// <remarks> Get the current status of the ATR mode on automated instrument models. This command does not indicate whether the ATR has currently acquired a prism.Note the difference between GetUserATR and GetUserLOCK state.</remarks>
        /// <returns> State of the ATR mode.</returns>
        /// <example> mod3 call 000001 AUS_SetUserAtrState </example>
        [COMF]
        public ON_OFF_TYPE? AUS_SetUserAtrState() =>
            Call("%R1Q,18005:", (resp) => (resp.ReturnCode) switch
            {
                GRC.NOT_IMPL => throw new LeicaException(resp.ReturnCode, "ATR not available; no automated instrument."),
                _ => Successful(resp.ReturnCode) && resp.Values.Length == 1 ? (ON_OFF_TYPE)resp.Values[0] : default
            });

        /// <summary> Getting the status of the LOCK mode.</summary>
        /// <remarks> This command gets the current status of the LOCK mode. This command is valid for automated instruments only.TheGetUserLockState command does not indicate if the instrument is currently locked to a prism.For this function the MotReadLockStatus has to be used.</remarks>
        /// <returns> State of the LOCK mode.</returns>
        /// <example> mod3 call 000001 AUS_GetUserLockState </example>
        [COMF]
        public ON_OFF_TYPE? AUS_GetUserLockState() =>
            Call("%R1Q,18008:", (resp) => (resp.ReturnCode) switch
            {
                GRC.NOT_IMPL => throw new LeicaException(resp.ReturnCode, "ATR not available; no automated instrument."),
                _ => Successful(resp.ReturnCode) && resp.Values.Length == 1 ? (ON_OFF_TYPE)resp.Values[0] : default
            });

        /// <summary> Setting the status of the LOCK mode.</summary>
        /// <remarks> Activates or deactivates the LOCK mode.<br/>Status ON: The LOCK mode is activated. This does not mean that the instrument is locked onto a prism. In order to lock and follow a moving target, see the function AUT_LockIn.<br/>
        /// Status OFF: The LOCK mode is deactivated. A moving target, which is being tracked, will be aborted and the manual drive wheel is activated.<br/>
        /// This command is valid for automated instruments only.</remarks>
        /// <param name="onOff"> State of the ATR lock switch.</param>
        /// <returns> State of the LOCK mode.</returns>
        /// <example> mod3 call 000001 AUS_SetUserLockState </example>
        [COMF]
        public bool AUS_SetUserLockState(bool onOff) =>
            Call("%R1Q,8007:", onOff, (resp) => (resp.ReturnCode) switch
            {
                GRC.NOT_IMPL => throw new LeicaException(resp.ReturnCode, "ATR not available; no automated instrument."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> Setting the status of the LOCK mode.</summary>
        /// <remarks> Activates or deactivates the LOCK mode.<br/>Status ON: The LOCK mode is activated. This does not mean that the instrument is locked onto a prism. In order to lock and follow a moving target, see the function AUT_LockIn.<br/>
        /// Status OFF: The LOCK mode is deactivated. A moving target, which is being tracked, will be aborted and the manual drive wheel is activated.<br/>
        /// This command is valid for automated instruments only.</remarks>
        /// <param name="onOff"> State of the ATR lock switch.</param>
        /// <returns> State of the LOCK mode.</returns>
        /// <example> mod3 call 000001 AUS_SetUserLockState </example>
        [COMF]
        public bool AUS_SetUserLockState(ON_OFF_TYPE onOff) =>
            AUS_SetUserLockState(onOff == ON_OFF_TYPE.ON);

        #endregion ALT USER (AUS CONF)

        #region AUTOMATION (AUT CONF)

        /// <summary> Reading the current setting for the positioning tolerances.</summary>
        /// <remarks> This command reads the current setting for the positioning tolerances of the Hz- and V- instrument axis.<br/>This command is valid for motorized instruments only.</remarks>
        /// <returns> State of the ATR mode.</returns>
        /// <example> mod3 call 000001 AUT_ReadTol </example>
        [COMF]
        public AUT_POSTOL? AUT_ReadTol() =>
            Call("%R1Q,9008:", (resp) => Successful(resp.ReturnCode) && resp.Values.Length == MOT_AXES ? new AUT_POSTOL
            {
                PosTol = new double[] { double.Parse(resp.Values[MOT_HZ_AXLE].ToString()), double.Parse(resp.Values[MOT_V_AXLE].ToString()) }
            }
            : default);

        /// <summary> Setting the positioning tolerances.</summary>
        /// <remarks> This command sets new values for the positioning tolerances of the Hz- and V- instrument axes. This command is valid for motorized instruments only.<br/>The tolerances must be in the range of 1[cc] ( =1.57079 E-06[rad] ) to 100[cc] ( =1.57079 E-04[rad]).<br/><br/>
        /// <b>Note:</b> The maximum resolution of the angle measurement system depends on the instrument accuracy class. If smaller positioning tolerances are required, the positioning time can increase drastically.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_SetTol 0 0 </example>
        [COMF]
        public bool AUT_SetTol(double toleranceHz, double toleranceV) =>
            Call("%R1Q,9007:", toleranceHz, toleranceV, (resp) => (resp.ReturnCode) switch
            {
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Invalid parameters. One or both tolerance values not within the boundaries(1.57079E-06[rad] =1[cc]to 1.57079E-04[rad] =100[cc])."),
                GRC.MOT_UNREADY => throw new LeicaException(resp.ReturnCode, "Instrument has no motorization."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> Setting the positioning tolerances.</summary>
        /// <remarks> This command sets new values for the positioning tolerances of the Hz- and V- instrument axes. This command is valid for motorized instruments only.<br/>The tolerances must be in the range of 1[cc] ( =1.57079 E-06[rad] ) to 100[cc] ( =1.57079 E-04[rad]).<br/><br/>
        /// <b>Note:</b> The maximum resolution of the angle measurement system depends on the instrument accuracy class. If smaller positioning tolerances are required, the positioning time can increase drastically.</remarks>
        /// <param name="tolPar"> The values for the positioning tolerances in Hz and V direction[rad].</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_SetTol </example>
        [COMF]
        public bool AUT_SetTol(AUT_POSTOL tolPar) =>
            AUT_SetTol(tolPar.PosTol[MOT_HZ_AXLE], tolPar.PosTol[MOT_V_AXLE]);

        /// <summary> Reading the current timeout setting for positioning.</summary>
        /// <remarks> This command reads the current setting for the positioning time out (maximum time to perform positioning).</remarks>
        /// <returns> The values for the positioning time out in Hz and V direction [sec].</returns>
        /// <example> mod3 call 000001 AUT_ReadTimeout </example>
        [COMF]
        public AUT_TIMEOUT? AUT_ReadTimeout() =>
            Call("%R1Q,9012:", (resp) => Successful(resp.ReturnCode) && resp.Values.Length == MOT_AXES ? new AUT_TIMEOUT
            {
                PosTimeout = new double[] { double.Parse(resp.Values[MOT_HZ_AXLE].ToString()), double.Parse(resp.Values[MOT_V_AXLE].ToString()) }
            }
            : default);

        /// <summary> Setting the timeout for positioning.</summary>
        /// <remarks> This command set the positioning timeout (set maximum time to perform a positioning). The timeout is reset on 7[sec] after each power on.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_SetTimeout </example>
        [COMF]
        public bool AUT_SetTimeout(double timeoutHz, double timeoutV) =>
            Call("%R1Q,9011:", timeoutHz, timeoutV, (resp) => (resp.ReturnCode) switch
            {
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "One or both time out values not within the boundaries (7[sec] to 60[sec])."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> Setting the timeout for positioning.</summary>
        /// <remarks> This command set the positioning timeout (set maximum time to perform a positioning). The timeout is reset on 7[sec] after each power on.</remarks>
        /// <param name="timeoutPar"> The values for the positioning timeout in Hz and V direction [s]. Valid values are between 7 [sec] and 60 [sec].</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_SetTimeout </example>
        [COMF]
        public bool AUT_SetTimeout(AUT_TIMEOUT timeoutPar) =>
            AUT_SetTimeout(timeoutPar.PosTimeout[MOT_HZ_AXLE], timeoutPar.PosTimeout[MOT_V_AXLE]);

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
        public bool AUT_MakePositioning(double Hz, double V, AUT_POSMODE POSMode, AUT_ATRMODE ATRMode, bool dummy = false) =>
            Call("%R1Q,9027:", Hz, V, POSMode, ATRMode, dummy, (resp) => (resp.ReturnCode) switch
            {
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
            });

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
        public bool AUT_ChangeFace(AUT_POSMODE POSMode, AUT_ATRMODE ATRMode, bool dummy = false) =>
            Call("%R1Q,9028:", POSMode, ATRMode, dummy, (resp) => (resp.ReturnCode) switch
            {
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
            });

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
        public bool AUT_FineAdjust(double srchHz, double srchV, bool dummy = false) =>
            Call("%R1Q,9037:", srchHz, srchV, dummy, (resp) => (resp.ReturnCode) switch
            {
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
            });

        /// <summary> Performing an automatic target search.</summary>
        /// <remarks> This procedure performs an automatically target search within a given area. The search is terminated once the prism appears in the field of view of the ATR sensor.If no prism is found within the specified area, the instrument turns back to the initial start position.For an exact positioning onto the prism centre, use fine adjust (see AUT_FineAdjust) afterwards.<br/><br/>
        /// <b>Note:</b> If you expand the search range of the function AUT_FineAdjust, then you have a target search and a fine positioning in one function.</remarks>
        /// <param name="Hz_Area"> Horizontal search region [rad].</param>
        /// <param name="V_Area"> Vertical search region [rad].</param>
        /// <param name="dummy"> It’s reserved for future use, set bDummy always to FALSE.</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_Search </example>
        [COMF]
        public bool AUT_Search(double Hz_Area, double V_Area, bool dummy = false) =>
            Call("%R1Q,9029:", Hz_Area, V_Area, dummy, (resp) => (resp.ReturnCode) switch
            {
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Invalid parameter."),
                GRC.AUT_MOTOR_ERROR => throw new LeicaException(resp.ReturnCode, "Instrument has no ‘motorization’."),
                GRC.FATAL => throw new LeicaException(resp.ReturnCode, "Fatal error."),
                GRC.ABORT => throw new LeicaException(resp.ReturnCode, "Function aborted."),
                GRC.AUT_NO_TARGET => throw new LeicaException(resp.ReturnCode, "No target found."),
                GRC.AUT_BAD_ENVIRONMENT => throw new LeicaException(resp.ReturnCode, "Inadequate environment conditions."),
                GRC.AUT_DETECTOR_ERROR => throw new LeicaException(resp.ReturnCode, "AZE error, at repeated occur call service."),
                GRC.COM_TIMEDOUT => throw new LeicaException(resp.ReturnCode, "Communication time out. (perhaps increase COM timeout, see COM_SetTimeout)."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> Getting the fine adjust positioning mode.</summary>
        /// <remarks> This function returns the current activated fine adjust positioning mode. This command is valid for all instruments, but has only effects for instruments equipped with ATR.</remarks>
        /// <returns> Current fine adjust positioning mode.</returns>
        /// <example> mod3 call 000001 AUT_GetFineAdjustMode </example>
        [COMF]
        public AUT_ADJMODE? AUT_GetFineAdjustMode() =>
            Call("%R1Q,9030:", (resp) => Successful(resp.ReturnCode) ? (AUT_ADJMODE)resp.Values[0] : default);

        /// <summary> Setting the fine adjust positioning mode.</summary>
        /// <remarks> This function sets the positioning tolerances (default values for both modes) relating the angle accuracy or the point accuracy for the fine adjust.This command is valid for all instruments, but has only effects for instruments equipped with ATR.If a target is very near or held by hand, it’s recommended to set the adjust-mode to AUT_POINT_MODE.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_SetFineAdjustMode </example>
        [COMF]
        public bool AUT_SetFineAdjustMode(AUT_ADJMODE adjMode) =>
            Call("%R1Q,9031:", adjMode, (resp) => (resp.ReturnCode) switch
            {
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Invalid mode."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> Starting the target tracking.</summary>
        /// <remarks> If LOCK mode is activated (AUS_SetUserLockState) then the function starts the target tracking. The AUT_LockIn command is only possible if a AUT_FineAdjust command has been previously sent and successfully executed.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_LockIn </example>
        [COMF]
        public bool AUT_LockIn() =>
            Call("%R1Q,9013:", (resp) => (resp.ReturnCode) switch
            {
                GRC.AUT_MOTOR_ERROR => throw new LeicaException(resp.ReturnCode, "Instrument has no ‘motorization’."),
                GRC.AUT_DETECTOR_ERROR => throw new LeicaException(resp.ReturnCode, "Error in target acquisition, at repeated occur call service."),
                GRC.AUT_NO_TARGET => throw new LeicaException(resp.ReturnCode, "No target detected, no previous Fine Adjust."),
                GRC.AUT_BAD_ENVIRONMENT => throw new LeicaException(resp.ReturnCode, "Bad environment conditions."),
                GRC.ATA_STRANGE_LIGHT => throw new LeicaException(resp.ReturnCode, "No target detected, no previous Fine Adjust."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> Getting the dimensions of the PowerSearch window.</summary>
        /// <remarks> This function returns the current position and size of the PowerSearch Window. This command is valid for all instruments, but has only effects for instruments equipped with PowerSearch.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_GetSearchArea </example>
        [COMF]
        public AUT_SEARCH_AREA? AUT_GetSearchArea() =>
            Call("%R1Q,9042:", (resp) => Successful(resp.ReturnCode) && resp.Values.Length == 5 ? new AUT_SEARCH_AREA
            {
                CenterHz = double.Parse(resp.Values[0].ToString()),
                CenterV = double.Parse(resp.Values[1].ToString()),
                RangeHz = double.Parse(resp.Values[2].ToString()),
                RangeV = double.Parse(resp.Values[3].ToString()),
                Enabled = resp.Values[4].Equals(1L)
            }
            : default);

        /// <summary> Setting the PowerSearch window.</summary>
        /// <remarks> This function defines the position and dimensions and activates the PowerSearch window. This command is valid for all instruments, but has only effects for instruments equipped with PowerSearch.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_SetSearchArea 0.5 1.5708 0.4 0.2 1 </example>
        [COMF]
        public bool AUT_SetSearchArea(double centerHz, double centerV, double rangeHz, double rangeV, bool enabled) =>
            Call("%R1Q,9043:", centerHz, centerV, rangeHz, rangeV, enabled, (resp) => Successful(resp.ReturnCode));

        /// <summary> Setting the PowerSearch window.</summary>
        /// <remarks> This function defines the position and dimensions and activates the PowerSearch window. This command is valid for all instruments, but has only effects for instruments equipped with PowerSearch.</remarks>
        /// <param name="area"> User defined searching area.</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_SetSearchArea 0.5 1.5708 0.4 0.2 1 </example>
        [COMF]
        public bool AUT_SetSearchArea(AUT_SEARCH_AREA area) =>
            AUT_SetSearchArea(area.CenterHz, area.CenterV, area.RangeHz, area.RangeV, area.Enabled);

        /// <summary> Getting the ATR search window.</summary>
        /// <remarks> This function returns the current dimension of ATR search window. This command is valid for all instruments, but has only affects automated instruments.</remarks>
        /// <returns> AUT_SEARCH_SPIRAL </returns>
        /// <example> mod3 call 000001 AUT_GetUserSpiral </example>
        [COMF]
        public AUT_SEARCH_SPIRAL? AUT_GetUserSpiral() =>
            Call("%R1Q,9040:", (resp) => Successful(resp.ReturnCode) && resp.Values.Length == 2 ? new AUT_SEARCH_SPIRAL
            {
                RangeHz = double.Parse(resp.Values[0].ToString()),
                RangeV = double.Parse(resp.Values[1].ToString())
            }
            : default);

        /// <summary> Setting the ATR search window.</summary>
        /// <remarks> This function sets the dimension of the ATR search window. This command is valid for all instruments, but has only effects for instruments equipped with ATR.</remarks>
        /// <param name="rangeHz"> ATR search window [rad].</param>
        /// <param name="rangeV"> ATR search window [rad].</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_SetUserSpiral 0.4 0.2 </example>
        [COMF]
        public bool AUT_SetUserSpiral(double rangeHz, double rangeV) =>
            Call("%R1Q,9041:", rangeHz, rangeV, (resp) => Successful(resp.ReturnCode));

        /// <summary> Setting the ATR search window.</summary>
        /// <remarks> This function sets the dimension of the ATR search window. This command is valid for all instruments, but has only effects for instruments equipped with ATR.</remarks>
        /// <param name="spiralDim"> ATR search window [rad].</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_SetUserSpiral 0.4 0.2 </example>
        [COMF]
        public bool AUT_SetUserSpiral(AUT_SEARCH_SPIRAL spiralDim) =>
            AUT_SetUserSpiral(spiralDim.RangeHz, spiralDim.RangeV);

        /// <summary> Enabling the PowerSearch window and PowerSearch range.</summary>
        /// <remarks> This command enables / disables the predefined PowerSearch window including the predefined PowerSearch range limits, set by AUT_PS_SetRange.</remarks>
        /// <param name="enable">TRUE: Enables the user distance limits for PowerSearch. FALSE: Default range 0..400m.</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_PS_EnableRange </example>
        [COMF]
        public bool AUT_PS_EnableRange(bool enable) =>
            Call("%R1Q,9048:", enable, (resp) => Successful(resp.ReturnCode));

        /// <summary> Setting the PowerSearch range.</summary>
        /// <remarks> This command defines the PowerSearch distance range limits.<br/>These additional limits(additional to the PowerSearch window) will be used once the range checking is enabled (AUT_PS_EnableRange).</remarks>
        /// <param name="maxDist"> Minimal distance to prism (≥ 0m).</param>
        /// <param name="minDist"> Maximal distance to prism, where maxDist ≤ 400m, maxdist ≥ minDist + 10 </param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_PS_SetRange </example>
        [COMF]
        public bool AUT_PS_SetRange(long minDist, long maxDist) =>
            Call("%R1Q,9047:", minDist, maxDist, (resp) => Successful(resp.ReturnCode));

        /// <summary> Starting PowerSearch.</summary>
        /// <remarks> This command starts PowerSearch inside the given PowerSearch window, defined by AUT_SetSearchArea and optional by AUT_PS_SetRange.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_PS_SearchWindow </example>
        [COMF]
        public bool AUT_PS_SearchWindow() =>
            Call("%R1Q,9052:", (resp) => (resp.ReturnCode) switch
            {
                GRC.AUT_NO_WORKING_AREA => throw new LeicaException(resp.ReturnCode, "Working area not defined."),
                GRC.AUT_NO_TARGET => throw new LeicaException(resp.ReturnCode, "No Target found."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> Starting PowerSearch.</summary>
        /// <remarks> This command executes the 360º default PowerSearch and searches for the next target. A previously defined PowerSearch window(AUT_SetSearchArea) is not taken into account.Use AUT_PS_SearchWindow to do so.</remarks>
        /// <param name="direction"> Defines the searching direction (CLKW=1 or ACLKW=-1).</param>
        /// <param name="swing"> TRUE: Searching starts -10 gon to the given direction direction.This setting finds targets left of the telescope direction faster</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 AUT_PS_SearchNext </example>
        [COMF]
        public bool AUT_PS_SearchNext(long direction, bool swing) =>
            Call("%R1Q,9051:", direction, swing, (resp) => (resp.ReturnCode) switch
            {
                GRC.AUT_NO_TARGET => throw new LeicaException(resp.ReturnCode, "No Target found."),
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Invalid parameters."),
                _ => Successful(resp.ReturnCode)
            });

        #endregion AUTOMATION (AUT CONF)

        #region BASIC APPLICATIONS (BAP CONF)

        /// <summary> Getting the EDM type.</summary>
        /// <remarks> Gets the current EDM type for distance measurements (Reflector (IR) or Reflectorless (RL)).</remarks>
        /// <returns> Actual target type.</returns>
        /// <example> mod3 call 000001 BAP_GetTargetType </example>
        [COMF]
        public BAP_TARGET_TYPE? BAP_GetTargetType() =>
            Call("%R1Q,17022:", (resp) => Successful(resp.ReturnCode) && resp.Values.Length == 1 ? (BAP_TARGET_TYPE)resp.Values[0] : default);

        /// <summary> Setting the EDM type.</summary>
        /// <remarks> Sets the current EDM type for distance measurements (Reflector (IR) or Reflectorless (RL)).<br/>For each EDM type the last used EDM mode is remembered and activated if the EDM type is changed.<br/>If EDM type IR is selected the last used Automation mode is automatically activated.<br/>BAP_SetMeasPrg can also change the target type.<br/>EDM type RL is not available on all instrument types.</remarks>
        /// <param name="targetType"> Target type </param>
        /// <example> mod3 call 000001 BAP_SetTargetType </example>
        [COMF]
        public bool BAP_SetTargetType(BAP_TARGET_TYPE targetType) =>
            Call("%R1Q,17021:", targetType, (resp) => Successful(resp.ReturnCode));

        /// <summary> Getting the default prism type.</summary>
        /// <remarks> Gets the current prism type.</remarks>
        /// <returns> Actual prism type.</returns>
        /// <example> mod3 call 000001 BAP_GetPrismType </example>
        [COMF]
        public BAP_PRISMTYPE? BAP_GetPrismType() =>
            Call("%R1Q,17009:", (resp) => Successful(resp.ReturnCode) && resp.Values.Length == 1 ? (BAP_PRISMTYPE)resp.Values[0] : default);

        /// <summary> Setting the default prism type.</summary>
        /// <remarks> Sets the prism type for measurements with a reflector. It overwrites the prism constant, set by TMC_SetPrismCorr.</remarks>
        /// <param name="prismType"> Prism type </param>
        /// <example> mod3 call 000001 BAP_SetPrismType </example>
        [COMF]
        public bool BAP_SetPrismType(BAP_PRISMTYPE prismType) =>
            Call("%R1Q,17008:", prismType, (resp) => Successful(resp.ReturnCode));

        /// <summary> Getting the default or user prism type.</summary>
        /// <remarks> Gets the current prism type and name.</remarks>
        /// <example> mod3 call 000001 BAP_GetPrismType2 </example>
        [COMF]
        public KeyValuePair<BAP_PRISMTYPE, string>? BAP_GetPrismType2() =>
            Call("%R1Q,17031:", (resp) => Successful(resp.ReturnCode) && resp.Values.Length == 2
                ? new KeyValuePair<BAP_PRISMTYPE, string>((BAP_PRISMTYPE)resp.Values[0], resp.Values[1].ToString()) : default);

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
        public bool BAP_SetUserPrismDef(string prismName, double addConst, BAP_REFLTYPE reflType, string creator) =>
            Call("%R1Q,17023:", prismName, addConst, reflType, creator, (resp) => (resp.ReturnCode) switch
            {
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Invalid prism definition."),
                GRC.IVRESULT => throw new LeicaException(resp.ReturnCode, "Prism definition is not set."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> Getting the actual distance measurement program.</summary>
        /// <remarks> Gets the current distance measurement program.</remarks>
        /// <returns> Actual measurement program </returns>
        /// <example> mod3 call 000001 BAP_GetMeasPrg </example>
        [COMF]
        public BAP_USER_MEASPRG? BAP_GetMeasPrg() =>
            Call("%R1Q,17018:", (resp) => Successful(resp.ReturnCode) && resp.Values.Length == 1 ? (BAP_USER_MEASPRG)resp.Values[0] : default);

        /// <summary> Setting the distance measurement program.</summary>
        /// <remarks> Defines the distance measurement program i.e. for BAP_MeasDistanceAngle.<br/>RL EDM type programs are not available on all instrument types.<br/>Changing the measurement programs may change the EDM type as well(Reflector (IR) and Reflectorless (RL)).</remarks>
        /// <example> mod3 call 000001 BAP_SetMeasPrg </example>
        [COMF]
        public bool BAP_SetMeasPrg(BAP_USER_MEASPRG measPrg) =>
            Call("%R1Q,17019:", measPrg, (resp) => (resp.ReturnCode) switch
            {
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Measurement program is not available."),
                _ => Successful(resp.ReturnCode)
            });

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
                case GRC.TMC_ANGLE_NO_FULL_CORRECTION: throw new LeicaException(resp.ReturnCode, "Warning, only angle measurement valid, accuracy cannot be guaranteed.");
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
        public bool BAP_SearchTarget(bool dummy = false) =>
            Call("%R1Q,17020:", dummy, (resp) => (resp.ReturnCode) switch
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
            });

        /// <summary> Getting the current ATR low vis mode.</summary>
        /// <remarks> Gets the current low vis mode.</remarks>
        /// <example> mod3 call 000001 BAP_GetATRSetting </example>
        [COMF]
        public BAP_ATRSETTING? BAP_GetATRSetting() => CallGet<BAP_ATRSETTING>("%R1Q,17034:");

        /// <summary> Setting the current ATR low vis mode.</summary>
        /// <remarks> Sets the current low vis mode.</remarks>
        /// <param name="ATRSetting"> ATR low vis mode.</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 BAP_SetATRSetting </example>
        [COMF]
        public bool BAP_SetATRSetting(BAP_ATRSETTING ATRSetting) => CallSet("%R1Q,17035:", ATRSetting);

        /// <summary> Getting the reduced ATR field of view.</summary>
        /// <remarks> Get reduced ATR field of view mode.</remarks>
        /// <returns> ON: ATR uses reduced field of view(about 1/9).<br/>OFF: ATR uses full field of view.</returns>
        /// <example> mod3 call 000001 BAP_GetRedATRFov </example>
        [COMF]
        public ON_OFF_TYPE? BAP_GetRedATRFov() => CallGet<ON_OFF_TYPE>("%R1Q,17036:");

        /// <summary> Setting the reduced ATR field of view.</summary>
        /// <remarks> Set reduced ATR field of view mode.</remarks>
        /// <param name="redFov"> true/false.</param>
        /// <example> mod3 call 000001 BAP_SetRedATRFov </example>
        [COMF]
        public bool BAP_SetRedATRFov(ON_OFF_TYPE redFov) => CallSet("%R1Q,17037:", redFov);

        #endregion BASIC APPLICATIONS (BAP CONF)

        #region BASIC MAN MACHINE INTERFACE (BMM COMF)

        /// <summary> Outputing an alarm signal (triple beep).</summary>
        /// <remarks> This function produces a triple beep with the configured intensity and frequency, which cannot be changed. If there is a continuous signal active, it will be stopped before.</remarks>
        /// <example> mod3 call 000001 BMM_BeepAlarm </example>
        [COMF]
        public bool BMM_BeepAlarm() =>
            Call("%R1Q,11004:", (resp) => Successful(resp.ReturnCode));

        /// <summary> Outputing an alarm signal (single beep).</summary>
        /// <remarks> This function produces a single beep with the configured intensity and frequency, which cannot be changed. If a continuous signal is active, it will be stopped first.</remarks>
        /// <example> mod3 call 000001 BMM_BeepNormal </example>
        [COMF]
        public bool BMM_BeepNormal() =>
            Call("%R1Q,11003:", (resp) => Successful(resp.ReturnCode));

        /// <summary> Starting a continuous beep signal.</summary>
        /// <remarks> This function switches on the beep-signal with the intensity nIntens. If a continuous signal is active, it will be stopped first.Turn off the beeping device with IOS_BeepOff.</remarks>
        /// <param name="intens">Intensity of the beep-signal (volume) expressed as a percentage(0-100 %).</param>
        /// <example> mod3 call 000001 IOS_BeepOn </example>
        [COMF]
        public bool IOS_BeepOn(int intens = IOS_BEEP_STDINTENS) =>
            Call("%R1Q,20001:", intens, (resp) => Successful(resp.ReturnCode));

        /// <summary> Stopping an active beep signal.</summary>
        /// <remarks> This function switches off the beep-signal.</remarks>
        /// <example> mod3 call 000001 IOS_BeepOff </example>
        [COMF]
        public bool IOS_BeepOff() =>
            Call("%R1Q,20000:", (resp) => Successful(resp.ReturnCode));

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

        #region FILE TRANSFER (FTR COMF)

        /// <summary> Setup list.</summary>
        /// <remarks> This command sets up the device, file type and search path. It has to be called before command FTR_List can be used.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 FTR_SetupList </example>
        [COMF]
        public bool FTR_SetupList(long deviceType, long fileType, string searchPath) =>
            Call("%R1Q,23306:", deviceType, fileType, searchPath, (resp) => (resp.ReturnCode) switch
            {
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Device not available or can not get path."),
                GRC.NOTOK => throw new LeicaException(resp.ReturnCode, "Setup already done or FTR_AbortList() not called."),
                GRC.FTR_FILEACCESS => throw new LeicaException(resp.ReturnCode, "File access error."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> List file.</summary>
        /// <remarks> This command gets one single file entry. The command FTR_List has to be called first.</remarks>
        /// <param name="next"> True if first entry otherwise next entry.</param>
        /// <param name="last"> True if last entry.</param>
        /// <param name="DirInfo"> Info about file name, size and modification time and date. The entry is not valid if the file name is empty("").</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 FTR_List </example>
        [COMF]
        public bool FTR_List(bool next, out bool last, out FTR_DIRINFO dirInfo)
        {
            var resp = Call("%R1Q,23307:", next, (resp) => (resp.ReturnCode) switch
            {
                GRC.FTR_MISSINGSETUP => throw new LeicaException(resp.ReturnCode, "Missing setup."),
                GRC.FTR_INVALIDINPUT => throw new LeicaException(resp.ReturnCode, "First block is missing or last time was already last block."),
                _ => Successful(resp.ReturnCode) ? resp : resp
            });
            last = resp.Values[0].Equals(1L);
            dirInfo = new FTR_DIRINFO()
            {
                FileName = resp.Values[1].ToString(),
                FileSize = (long)resp.Values[2],
                ModTime = new FTR_MODTIME((long)resp.Values[3], (long)resp.Values[4], (long)resp.Values[5], (long)resp.Values[6]),
                ModDate = new FTR_MODDATE((long)resp.Values[7], (long)resp.Values[8], (long)resp.Values[9])
            };
            return true;
        }

        /// <summary> Abort list.</summary>
        /// <remarks> This command aborts or ends file list command.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 FTR_AbortList </example>
        [COMF]
        public bool FTR_AbortList() => CallSet("%R1Q,23308:");

        /// <summary> Setup download.</summary>
        /// <remarks> This command sets up the download of a file from the instrument. It has to be called before command FTR_Download can be used.</remarks>
        /// <param name="deviceType"> Device type.</param>
        /// <param name="fileType"> File type.</param>
        /// <param name="fileNameSrc">File name with extension. If file type is FTR_FILE_UNKNOWN additional file path is required.</param>
        /// <param name="blockSize"> Block size. Max value is FTR_MAX_BLOCKSIZE.</param>
        /// <returns> Number of blocks required to upload the file.</returns>
        /// <example> mod3 call 000001 FTR_SetupDownload </example>
        [COMF]
        public int? FTR_SetupDownload(FTR_DEVICETYPE deviceType, FTR_FILETYPE fileType, string fileNameSrc, int blockSize) =>
            Call("%R1Q,23303:", deviceType, fileType, fileNameSrc, blockSize, (resp) => (resp.ReturnCode) switch
            {
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Device not available or can not get path."),
                GRC.NOTOK => throw new LeicaException(resp.ReturnCode, "Setup already done or FTR_AbortDownload() not called."),
                GRC.FTR_INVALIDINPUT => throw new LeicaException(resp.ReturnCode, "Block size too big."),
                GRC.FTR_FILEACCESS => throw new LeicaException(resp.ReturnCode, "File access error."),
                _ => Successful(resp.ReturnCode) && resp.Values.Length == 1 ? (int)(long)resp.Values[0] : default
            });

        /// <summary> Download file.</summary>
        /// <remarks> This command gets one single block of data. The command FTR_SetupDownload has to be called first.<br/>
        /// <b>Note: </b> The maximum block number in C/VB is 65535/32767 therefore the file size is limited to 28MB/14MB. Visual Basic does not know data type unsigned integer.</remarks>
        /// <param name="blockNumber"> Blocknumber. The lock number starts with 1. If block number is 0 then the download process is aborted.</param>
        /// <returns> Block of data.</returns>
        /// <example> mod3 call 000001 FTR_Download </example>
        [COMF]
        public int FTR_Download(int blockNumber, out string block)
        {
            var resp = Call("%R1Q,23304:", blockNumber, (resp) => (resp.ReturnCode) switch
            {
                GRC.FTR_MISSINGSETUP => throw new LeicaException(resp.ReturnCode, "Missing setup."),
                GRC.FTR_INVALIDINPUT => throw new LeicaException(resp.ReturnCode, "Block size too big."),
                GRC.FTR_FILEACCESS => throw new LeicaException(resp.ReturnCode, "File access error."),
                _ => Successful(resp.ReturnCode) ? resp : resp
            });
            block = resp.Values[0].ToString();
            return (int)(long)resp.Values[1];
        }

        /// <summary> Abort download.</summary>
        /// <remarks> This command aborts or ends file download command.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 FTR_AbortDownload </example>
        [COMF]
        public bool FTR_AbortDownload() => CallSet("%R1Q,23305:");

        /// <summary> Delete file.</summary>
        /// <remarks> This command deletes one or more files. Wildcards may be used to delete multiple files. If deletion date is valid only files older than deletion date are deleted.</remarks>
        /// <param name="deviceType"> Device type.</param>
        /// <param name="fileType"> File type.</param>
        /// <param name="delDate"> Deletion date. Valid if ucMonth is not 0.</param>
        /// <returns> Number of deleted files.</returns>
        /// <example> mod3 call 000001 FTR_Delete </example>
        [COMF]
        public int FTR_Delete(FTR_DEVICETYPE deviceType, FTR_FILETYPE fileType, DateTime delDate, string fileName) =>
            Call("%R1Q,23309:", deviceType, fileType, delDate.Day, delDate.Month, delDate.Year, fileName, (resp) => (resp.ReturnCode) switch
            {
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Device not available or can not get path."),
                _ => Successful(resp.ReturnCode) && resp.Values.Length == 1 ? (int)(long)resp.Values[0] : default
            });

        #endregion FILE TRANSFER (FTR COMF)

        #region IMAGE PROCESSING (IMG COMF)

        /// <summary> Reading the actual image configuration.</summary>
        /// <param name="memType"> Memory device type </param>
        /// <returns> Number of deleted files.</returns>
        /// <example> mod3 call 000001 IMG_GetTccConfig </example>
        [COMF]
        public IMG_TCC_CONFIG? IMG_GetTccConfig(IMG_MEM_TYPE memType) =>
            Call("%R1Q,23400:", memType, (resp) => (resp.ReturnCode) switch
            {
                GRC.NA => throw new LeicaException(resp.ReturnCode, "Imaging license key not available."),
                GRC.FATAL => throw new LeicaException(resp.ReturnCode, "CF card is not available or configuration file does not exist."),
                GRC.IVVERSION => throw new LeicaException(resp.ReturnCode, "Configuration file version differs from that of system firmware."),
                _ => Successful(resp.ReturnCode) && resp.Values.Length == 1 ? new IMG_TCC_CONFIG
                {
                    ImageNumber = (long)resp.Values[0],
                    Quality = (long)resp.Values[1],
                    SubFunctNumber = (long)resp.Values[2],
                    FileNamePrefix = resp.Values[3].ToString()
                } : default
            });

        /// <summary> Setting the actual image configuration.</summary>
        /// <param name="memType"> Memory device type </param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 IMG_SetTccConfig </example>
        [COMF]
        public bool IMG_SetTccConfig(IMG_MEM_TYPE memType, long imageNumber, long quality, long subFunctNumber, string fileNamePrefix) =>
            Call("%R1Q,23401:", imageNumber, quality, subFunctNumber, fileNamePrefix, (resp) => (resp.ReturnCode) switch
            {
                GRC.NA => throw new LeicaException(resp.ReturnCode, "Imaging license key not available."),
                GRC.FATAL => throw new LeicaException(resp.ReturnCode, "CF card is not available full. Any parameter is out of range."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> Setting the actual image configuration.</summary>
        /// <param name="memType"> Memory device type </param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 IMG_SetTccConfig </example>
        [COMF]
        public bool IMG_SetTccConfig(IMG_MEM_TYPE memType, IMG_TCC_CONFIG parameters) =>
            IMG_SetTccConfig(memType, parameters.ImageNumber, parameters.Quality, parameters.SubFunctNumber, parameters.FileNamePrefix);

        /// <summary> Capture a telescopic image.</summary>
        /// <returns> Number of the currently captured image.</returns>
        /// <example> mod3 call 000001 IMG_TakeTccImage </example>
        [COMF]
        public int? IMG_TakeTccImage(IMG_MEM_TYPE memTypes) =>
            Call("%R1Q,23402:", memTypes, (resp) => (resp.ReturnCode) switch
            {
                GRC.NA => throw new LeicaException(resp.ReturnCode, "Imaging license key not available."),
                GRC.IVRESULT => throw new LeicaException(resp.ReturnCode, "Not supported by Telescope Firmware."),
                GRC.FATAL => throw new LeicaException(resp.ReturnCode, "CF card is not available full."),
                _ => Successful(resp.ReturnCode) && resp.Values.Length == 1 ? (int)(long)resp.Values[0] : default
            });

        #endregion IMAGE PROCESSING (IMG COMF)

        #region MOTORISATION (MOT COMF)

        /// <summary> Returning the condition of the LockIn control.</summary>
        /// <remarks> This function returns the current condition of the LockIn control (see subsystem AUT for further information). This command is valid for automated instruments only.</remarks>
        /// <returns> Lock information.</returns>
        /// <example> mod3 call 000001 MOT_ReadLockStatus </example>
        [COMF]
        public MOT_LOCK_STATUS? MOT_ReadLockStatus() =>
            Call("%R1Q,6021:", (resp) => (resp.ReturnCode) switch
            {
                GRC.NOT_IMPL => throw new LeicaException(resp.ReturnCode, "No motorisation available (no automated instrument)."),
                _ => Successful(resp.ReturnCode) && resp.Values.Length == 1 ? (MOT_LOCK_STATUS)resp.Values[0] : default
            });

        /// <summary> Starting the motor controller.</summary>
        /// <remarks> This command is used to enable remote or user interaction to the motor controller.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 MOT_StartController </example>
        [COMF]
        public bool MOT_StartController(MOT_MODE controlMode) =>
            Call("%R1Q,6001:", controlMode, (resp) => (resp.ReturnCode) switch
            {
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "The value of ControlMode is not MOT_OCONST."),
                GRC.NOT_IMPL => throw new LeicaException(resp.ReturnCode, "No motorization available (no automated instrument)."),
                GRC.MOT_BUSY => throw new LeicaException(resp.ReturnCode, "Subsystem is busy (e.g. controller already started)."),
                GRC.MOT_UNREADY => throw new LeicaException(resp.ReturnCode, "Subsystem is not initialised."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> Stopping the motor controller.</summary>
        /// <remarks> This command is used to stop movement and to stop the motor controller operation.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 MOT_StopController </example>
        [COMF]
        public bool MOT_StopController(MOT_STOPMODE stopMode) =>
            Call("%R1Q,6002:", stopMode, (resp) => (resp.ReturnCode) switch
            {
                GRC.MOT_NOT_BUSY => throw new LeicaException(resp.ReturnCode, "No movement in progress (e.g. stop without start)."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> Driving the instrument with a constant speed.</summary>
        /// <remarks> This command is used to set up the velocity of motorization. This function is valid only if MOT_StartController(MOT_OCONST) has been called previously.RefOmega[0] denotes the horizontal and RefOmega[1] denotes the vertical velocity setting.</remarks>
        /// <param name="hzSpeed"> The speed in horizontal and vertical direction in rad/s. The maximum speed is +/- 3.14 rad/s each for TM30 TS30 instruments and 0.79 rad/s each for TPS1200 instruments.</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 MOT_SetVelocity </example>
        [COMF]
        public bool MOT_SetVelocity(double hzSpeed, double vSpeed) =>
            Call("%R1Q,6004:", hzSpeed, vSpeed, (resp) => (resp.ReturnCode) switch
            {
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "RefOmega.adValue[HZ] and/or RefOmega.adValue[V] values are not within the boundaries."),
                GRC.MOT_NOT_CONFIG => throw new LeicaException(resp.ReturnCode, "System is not in state MOT_CONFIG or MOT_BUSY_OPEN_END (e.g. missing ‘start controller’)."),
                GRC.MOT_NOT_OCONST => throw new LeicaException(resp.ReturnCode, "Drive is not in mode MOT_OCONST (set by MOT_StartController)."),
                GRC.NOT_IMPL => throw new LeicaException(resp.ReturnCode, "No motorization available (no automated instrument)."),
                _ => Successful(resp.ReturnCode)
            });

        #endregion MOTORISATION (MOT COMF)

        #region SUPERVISOR (SUP COMF)

        /// <summary> Getting the power management configuration status.</summary>
        /// <remarks> The returned settings are power off configuration and timing.</remarks>
        /// <returns> AutoPower = Current activated shut down mechanism.<br/>Timeout = The timeout in ms. After this time the device switches in the mode defined by the value of AutoPower when no user activity(press a key, turn the device or communication via GeoCOM) occurs.</returns>
        /// <example> mod3 call 000001 SUP_GetConfig </example>
        [COMF]
        public bool SUP_GetConfig(out bool reserved, out SUP_AUTO_POWER autoPower, out long timeout)
        {
            var resp = Call("%R1Q,14001:", resp => resp);
            reserved = resp.Values[0].Equals(1L);
            autoPower = (SUP_AUTO_POWER)resp.Values[1];
            timeout = (long)resp.Values[2];
            return true;
        }

        /// <summary> Setting the power management configuration.</summary>
        /// <remarks> Set the auto power off mode to AUTO_POWER_DISABLED or AUTO_POWER_OFF and the corresponding timeout.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 SUP_SetConfig </example>
        [COMF]
        public bool SUP_SetConfig(bool reserved, SUP_AUTO_POWER autoPower, long timeout) =>
            Call("%R1Q,14002:", reserved, autoPower, timeout, (resp) => (resp.ReturnCode) switch
            {
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "Timeout parameter invalid."),
                _ => Successful(resp.ReturnCode)
            });

        #endregion SUPERVISOR (SUP COMF)

        #region THEODOLITE MEASUREMENT AND CALCULATION (TMC CONF)

        /// <summary> Getting the coordinates of a measured point.</summary>
        /// <remarks> This function queries an angle measurement and, in dependence of the selected Mode, an inclination measurement and calculates the co-ordinates of the measured point with an already measured distance.A distance measurement has to be started in advance.The WaitTime is a delay to wait for the distance measurement to finish. Single and tracking measurements are supported. Information about a missing distance measurement and other information about the quality of the result is returned in the return- code.</remarks>
        /// <param name="waitTime"> The delay to wait for the distance measurement to finish [ms].</param>
        /// <param name="mode"> Inclination sensor measurement mode.</param>
        /// <returns> Calculated Cartesian co-ordinates.</returns>
        /// <example> mod3 call 000001 TMC_GetCoordinate </example>
        [COMF]
        public TMC_COORDINATE? TMC_GetCoordinate(long waitTime, TMC_INCLINE_PRG mode) =>
            Call("%R1Q,2082:", waitTime, mode, (resp) => (resp.ReturnCode) switch
            {
                GRC.TMC_ACCURACY_GUARANTEE => throw new LeicaException(resp.ReturnCode, "Accuracy is not guaranteed, because the result is containing measurement data which accuracy could not be verified by the system. Co-ordinates are available."),
                GRC.TMC_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "The results are not corrected by all active sensors. Coordinates are available. In order to secure which correction is missing use the both functions TMC_IfDataAzeCorrError and TMC_IfDataIncCorrError."),
                GRC.TMC_ANGLE_OK => throw new LeicaException(resp.ReturnCode, "Angle values okay, but no valid distance. Co-ordinates are not available."),
                GRC.TMC_ANGLE_NO_ACC_GUARANTY => throw new LeicaException(resp.ReturnCode, "Only the angle measurement is valid but its accuracy cannot be guaranteed (the tilt measurement is not available)."),
                GRC.TMC_ANGLE_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "No distance data available but angle data are valid. The return code is equivalent to the GRC_TMC_NO_FULL_CORRECTION and relates to the angle data. Co-ordinates are not available. Perform a distance measurement first before you call this function."),
                GRC.TMC_DIST_ERROR => throw new LeicaException(resp.ReturnCode, "No measuring, because of missing target point, co-ordinates are not available. Aim target point and try it again."),
                GRC.TMC_DIST_PPM => throw new LeicaException(resp.ReturnCode, "No distance measurement respectively no distance data because of wrong EDM settings. Co-ordinates are not available."),
                GRC.TMC_ANGLE_ERROR => throw new LeicaException(resp.ReturnCode, "Angle or inclination measurement error. Check inclination modes in commands."),
                GRC.TMC_BUSY => throw new LeicaException(resp.ReturnCode, "TMC resource is locked respectively TMC task is busy. Repeat measurement."),
                GRC.ABORT => throw new LeicaException(resp.ReturnCode, "Measurement through customer aborted."),
                GRC.SHUT_DOWN => throw new LeicaException(resp.ReturnCode, "System power off through customer."),
                _ => Successful(resp.ReturnCode) && resp.Values.Length == 8 ? new TMC_COORDINATE
                {
                    E = double.Parse(resp.Values[0].ToString()),
                    N = double.Parse(resp.Values[1].ToString()),
                    H = double.Parse(resp.Values[2].ToString()),
                    CoordTime = (long)resp.Values[3],
                    E_Cont = double.Parse(resp.Values[4].ToString()),
                    N_Cont = double.Parse(resp.Values[5].ToString()),
                    H_Cont = double.Parse(resp.Values[6].ToString()),
                    CoordContTime = (long)resp.Values[7]
                } : default
            });

        /// <summary> Returning an angle and distance measurement.</summary>
        /// <remarks> This function returns the angles and distance measurement data. This command does not issue a new distance measurement.A distance measurement has to be started in advance.If a distance measurement is valid the function ignores WaitTime and returns the results.If no valid distance measurement is available and the distance measurement unit is not activated(by TMC_DoMeasure before the TMC_GetSimpleMea call) the angle measurement result is returned after the waittime.Information about distance measurement is returned in the return code.</remarks>
        /// <param name="waitTime"> The delay to wait for the distance measurement to finish [ms].</param>
        /// <param name="mode"> Inclination sensor measurement mode.</param>
        /// <returns> Result of the angle measurement [rad].<br/>Result of the distance measurement [m].</returns>
        /// <example> mod3 call 000001 TMC_GetSimpleMea </example>
        [COMF]
        public bool TMC_GetSimpleMea(long waitTime, TMC_INCLINE_PRG mode, out TMC_HZ_V_ANG onlyAngle, out double slopeDistance)
        {
            var resp = Call("%R1Q,2108:", waitTime, mode, (resp) => (resp.ReturnCode) switch
            {
                GRC.TMC_ACCURACY_GUARANTEE => throw new LeicaException(resp.ReturnCode, "Accuracy is not guaranteed because the result consists of data which accuracy could not be verified by the system. Angle and distance data are available."),
                GRC.TMC_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "The results are not corrected by all active sensors. Angle and distance data are available. In order to secure which correction is missing use the both functions TMC_IfDataAzeCorrError and TMC_IfDataIncCorrError. This message is to be considered as a warning."),
                GRC.TMC_ANGLE_OK => throw new LeicaException(resp.ReturnCode, "Angle values okay, but no valid distance. Perform a distance measurement previously."),
                GRC.TMC_ANGLE_NO_ACC_GUARANTY => throw new LeicaException(resp.ReturnCode, "Only the angle measurement is valid but its accuracy cannot be guaranteed (the tilt measurement is not available)."),
                GRC.TMC_ANGLE_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "No distance data available but angle data are valid. The return code is equivalent to the GRC_TMC_NO_FULL_CORRECTION and relates to the angle data. Co-ordinates are not available. Perform a distance measurement first before you call this function."),
                GRC.TMC_DIST_ERROR => throw new LeicaException(resp.ReturnCode, "No measuring, because of missing target point, co-ordinates are not available. Aim target point and try it again."),
                GRC.TMC_DIST_PPM => throw new LeicaException(resp.ReturnCode, "No distance measurement respectively no distance data because of wrong EDM settings. Angle data are available but distance data are not available."),
                GRC.TMC_ANGLE_ERROR => throw new LeicaException(resp.ReturnCode, "Angle or inclination measurement error. Check inclination modes in commands."),
                GRC.TMC_BUSY => throw new LeicaException(resp.ReturnCode, "TMC resource is locked respectively TMC task is busy. Repeat measurement."),
                GRC.ABORT => throw new LeicaException(resp.ReturnCode, "Measurement through customer aborted."),
                GRC.SHUT_DOWN => throw new LeicaException(resp.ReturnCode, "System power off through customer."),
                _ => Successful(resp.ReturnCode) ? resp : resp
            });
            onlyAngle = new TMC_HZ_V_ANG() { Hz = double.Parse(resp.Values[0].ToString()), V = double.Parse(resp.Values[1].ToString()) };
            slopeDistance = double.Parse(resp.Values[2].ToString());
            return true;
        }

        /// <summary> Returning a complete angle measurement.</summary>
        /// <remarks> This function carries out an angle measurement and, in dependence of configuration, inclination measurement and returns the results.As shown the result is very comprehensive. For simple angle measurements use TMC_GetAngle5 or TMC_GetSimpleMea instead.<br/>Information about measurement is returned in the return code.</remarks>
        /// <param name="mode"> Inclination sensor measurement mode.</param>
        /// <returns> Result of the angle measurement.</returns>
        /// <example> mod3 call 000001 TMC_GetAngle1 </example>
        [COMF]
        public TMC_ANGLE? TMC_GetAngle1(TMC_INCLINE_PRG mode) =>
            Call("%R1Q,2003:", mode, (resp) => (resp.ReturnCode) switch
            {
                GRC.TMC_ACCURACY_GUARANTEE => throw new LeicaException(resp.ReturnCode, "Accuracy is not guaranteed because the result consists of data which accuracy could not be verified by the system. Angle and distance data are available."),
                GRC.TMC_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "The results are not corrected by all active sensors. Angle and distance data are available. In order to secure which correction is missing use the both functions TMC_IfDataAzeCorrError and TMC_IfDataIncCorrError. This message is to be considered as a warning."),
                GRC.TMC_ANGLE_OK => throw new LeicaException(resp.ReturnCode, "Angle values okay, but no valid distance. Perform a distance measurement previously."),
                GRC.TMC_ANGLE_NO_ACC_GUARANTY => throw new LeicaException(resp.ReturnCode, "Only the angle measurement is valid but its accuracy cannot be guaranteed (the tilt measurement is not available)."),
                GRC.TMC_ANGLE_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "No distance data available but angle data are valid. The return code is equivalent to the GRC_TMC_NO_FULL_CORRECTION and relates to the angle data. Co-ordinates are not available. Perform a distance measurement first before you call this function."),
                GRC.TMC_DIST_ERROR => throw new LeicaException(resp.ReturnCode, "No measuring, because of missing target point, co-ordinates are not available. Aim target point and try it again."),
                GRC.TMC_DIST_PPM => throw new LeicaException(resp.ReturnCode, "No distance measurement respectively no distance data because of wrong EDM settings. Angle data are available but distance data are not available."),
                GRC.TMC_ANGLE_ERROR => throw new LeicaException(resp.ReturnCode, "Angle or inclination measurement error. Check inclination modes in commands."),
                GRC.TMC_BUSY => throw new LeicaException(resp.ReturnCode, "TMC resource is locked respectively TMC task is busy. Repeat measurement."),
                GRC.ABORT => throw new LeicaException(resp.ReturnCode, "Measurement through customer aborted."),
                GRC.SHUT_DOWN => throw new LeicaException(resp.ReturnCode, "System power off through customer."),
                _ => Successful(resp.ReturnCode) && resp.Values.Length == 9 ? new TMC_ANGLE()
                {
                    Hz = double.Parse(resp.Values[0].ToString()),
                    V = double.Parse(resp.Values[1].ToString()),
                    AngleAccuracy = double.Parse(resp.Values[2].ToString()),
                    AngleTime = DateTime.FromFileTimeUtc((long)resp.Values[3]),
                    Incline = new TMC_INCLINE()
                    {
                        CrossIncline = double.Parse(resp.Values[4].ToString()),
                        LengthIncline = double.Parse(resp.Values[5].ToString()),
                        AccuracyIncline = double.Parse(resp.Values[6].ToString()),
                        InclineTime = DateTime.FromFileTimeUtc((long)resp.Values[7])
                    },
                    Face = (TMC_FACE)resp.Values[8]
                } : default
            });

        /// <summary> Returning a simple angle measurement.</summary>
        /// <remarks> This function carries out an angle measurement and returns the results. In contrast to the function TMC_GetAngle1 this function returns only the values of the angle. For simple angle measurements use TMC_GetSimpleMea instead.<br/>Information about measurement is returned in the return code.</remarks>
        /// <param name="mode"> Inclination sensor measurement mode.</param>
        /// <returns> Result of the angle measurement.</returns>
        /// <example> mod3 call 000001 TMC_GetAngle5 </example>
        [COMF]
        public TMC_HZ_V_ANG? TMC_GetAngle5(TMC_INCLINE_PRG mode) =>
            Call("%R1Q,2107:", mode, (resp) => (resp.ReturnCode) switch
            {
                GRC.TMC_ACCURACY_GUARANTEE => throw new LeicaException(resp.ReturnCode, "Accuracy is not guaranteed because the result consists of data which accuracy could not be verified by the system. Angle and distance data are available."),
                GRC.TMC_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "The results are not corrected by all active sensors. Angle and distance data are available. In order to secure which correction is missing use the both functions TMC_IfDataAzeCorrError and TMC_IfDataIncCorrError. This message is to be considered as a warning."),
                GRC.TMC_ANGLE_OK => throw new LeicaException(resp.ReturnCode, "Angle values okay, but no valid distance. Perform a distance measurement previously."),
                GRC.TMC_ANGLE_NO_ACC_GUARANTY => throw new LeicaException(resp.ReturnCode, "Only the angle measurement is valid but its accuracy cannot be guaranteed (the tilt measurement is not available)."),
                GRC.TMC_ANGLE_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "No distance data available but angle data are valid. The return code is equivalent to the GRC_TMC_NO_FULL_CORRECTION and relates to the angle data. Co-ordinates are not available. Perform a distance measurement first before you call this function."),
                GRC.TMC_DIST_ERROR => throw new LeicaException(resp.ReturnCode, "No measuring, because of missing target point, co-ordinates are not available. Aim target point and try it again."),
                GRC.TMC_DIST_PPM => throw new LeicaException(resp.ReturnCode, "No distance measurement respectively no distance data because of wrong EDM settings. Angle data are available but distance data are not available."),
                GRC.TMC_ANGLE_ERROR => throw new LeicaException(resp.ReturnCode, "Angle or inclination measurement error. Check inclination modes in commands."),
                GRC.TMC_BUSY => throw new LeicaException(resp.ReturnCode, "TMC resource is locked respectively TMC task is busy. Repeat measurement."),
                GRC.ABORT => throw new LeicaException(resp.ReturnCode, "Measurement through customer aborted."),
                GRC.SHUT_DOWN => throw new LeicaException(resp.ReturnCode, "System power off through customer."),
                _ => Successful(resp.ReturnCode) && resp.Values.Length == 2 ? new TMC_HZ_V_ANG()
                {
                    Hz = double.Parse(resp.Values[0].ToString()),
                    V = double.Parse(resp.Values[1].ToString())
                } : default
            });

        /// <summary> Returning a slope distance and hz-angle, v-angle.</summary>
        /// <remarks> The function starts an EDM Tracking measurement and waits until a distance is measured. Then it returns the angle and the slope-distance, but no co-ordinates.If no distance can be measured, it returns the angle values(hz, v) and the corresponding return-code.<br/>In order to abort the current measuring program use the function TMC_DoMeasure.</remarks>
        /// <param name="mode"> Inclination sensor measurement mode.</param>
        /// <returns> <b>OnlyAngle</b> measured Hz- and V- angle.<br/><b>SlopeDistance</b> measured slope-distance.</returns>
        /// <example> mod3 call 000001 TMC_QuickDist </example>
        [COMF]
        public bool TMC_QuickDist(out TMC_HZ_V_ANG onlyAngle, out double slopeDistance)
        {
            var resp = Call("%R1Q,2117:", (resp) => (resp.ReturnCode) switch
             {
                 GRC.TMC_ACCURACY_GUARANTEE => throw new LeicaException(resp.ReturnCode, "Accuracy is not guaranteed because the result consists of data which accuracy could not be verified by the system. Angle and distance data are available."),
                 GRC.TMC_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "The results are not corrected by all active sensors. Angle and distance data are available. In order to secure which correction is missing use the both functions TMC_IfDataAzeCorrError and TMC_IfDataIncCorrError. This message is to be considered as a warning."),
                 GRC.TMC_ANGLE_OK => throw new LeicaException(resp.ReturnCode, "Angle values okay, but no valid distance. Perform a distance measurement previously."),
                 GRC.TMC_ANGLE_NO_ACC_GUARANTY => throw new LeicaException(resp.ReturnCode, "Only the angle measurement is valid but its accuracy cannot be guaranteed (the tilt measurement is not available)."),
                 GRC.TMC_ANGLE_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "No distance data available but angle data are valid. The return code is equivalent to the GRC_TMC_NO_FULL_CORRECTION and relates to the angle data. Co-ordinates are not available. Perform a distance measurement first before you call this function."),
                 GRC.TMC_DIST_ERROR => throw new LeicaException(resp.ReturnCode, "No measuring, because of missing target point, co-ordinates are not available. Aim target point and try it again."),
                 GRC.TMC_DIST_PPM => throw new LeicaException(resp.ReturnCode, "No distance measurement respectively no distance data because of wrong EDM settings. Angle data are available but distance data are not available."),
                 GRC.TMC_ANGLE_ERROR => throw new LeicaException(resp.ReturnCode, "Angle or inclination measurement error. Check inclination modes in commands."),
                 GRC.TMC_BUSY => throw new LeicaException(resp.ReturnCode, "TMC resource is locked respectively TMC task is busy. Repeat measurement."),
                 GRC.ABORT => throw new LeicaException(resp.ReturnCode, "Measurement through customer aborted."),
                 GRC.SHUT_DOWN => throw new LeicaException(resp.ReturnCode, "System power off through customer."),
                 _ => Successful(resp.ReturnCode) ? resp : resp
             });
            onlyAngle = new TMC_HZ_V_ANG()
            {
                Hz = double.Parse(resp.Values[0].ToString()),
                V = double.Parse(resp.Values[1].ToString())
            };
            slopeDistance = double.Parse(resp.Values[2].ToString());
            return true;
        }

        /// <summary> Returning an angle, inclination and distance measurement.</summary>
        /// <remarks> This function returns angle, inclination and distance measurement data including accuracy and distance measurement time.This command does not issue a new distance measurement.A distance measurement has to be started in advance.If a distance measurement is valid the function ignores WaitTime and returns the results. If no valid distance measurement is available and the distance measurement unit is not activated (by TMC_DoMeasure before the TMC_GetFullMeas call) the angle measurement result is returned after the waiting time.Information about distance measurement is returned in the return code.</remarks>
        /// <param name="waitTime"> The delay to wait for the distance measurement to finish [ms].</param>
        /// <param name="mode"> Inclination sensor measurement mode.</param>
        /// <returns> <b>OnlyAngle</b> measured Hz- and V- angle.<br/><b>SlopeDistance</b> measured slope-distance.</returns>
        /// <example> mod3 call 000001 TMC_GetFullMeas </example>
        [COMF]
        public bool TMC_GetFullMeas(long waitTime, TMC_INCLINE_PRG mode,
            out double hzAngle, out double vAngle, out double accuracyAngle, out double crossIncl, out double lengthIncl, out double accuracyIncl, out double slopeDist, out double distTime)
        {
            var resp = Call("%R1Q,2167:", waitTime, mode, (resp) => (resp.ReturnCode) switch
            {
                GRC.TMC_ACCURACY_GUARANTEE => throw new LeicaException(resp.ReturnCode, "Accuracy is not guaranteed because the result consists of data which accuracy could not be verified by the system. Angle and distance data are available."),
                GRC.TMC_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "The results are not corrected by all active sensors. Angle and distance data are available. In order to secure which correction is missing use the both functions TMC_IfDataAzeCorrError and TMC_IfDataIncCorrError. This message is to be considered as a warning."),
                GRC.TMC_ANGLE_OK => throw new LeicaException(resp.ReturnCode, "Angle values okay, but no valid distance. Perform a distance measurement previously."),
                GRC.TMC_ANGLE_NO_ACC_GUARANTY => throw new LeicaException(resp.ReturnCode, "Only the angle measurement is valid but its accuracy cannot be guaranteed (the tilt measurement is not available)."),
                GRC.TMC_ANGLE_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "No distance data available but angle data are valid. The return code is equivalent to the GRC_TMC_NO_FULL_CORRECTION and relates to the angle data. Co-ordinates are not available. Perform a distance measurement first before you call this function."),
                GRC.TMC_DIST_ERROR => throw new LeicaException(resp.ReturnCode, "No measuring, because of missing target point, co-ordinates are not available. Aim target point and try it again."),
                GRC.TMC_DIST_PPM => throw new LeicaException(resp.ReturnCode, "No distance measurement respectively no distance data because of wrong EDM settings. Angle data are available but distance data are not available."),
                GRC.TMC_ANGLE_ERROR => throw new LeicaException(resp.ReturnCode, "Angle or inclination measurement error. Check inclination modes in commands."),
                GRC.TMC_BUSY => throw new LeicaException(resp.ReturnCode, "TMC resource is locked respectively TMC task is busy. Repeat measurement."),
                GRC.ABORT => throw new LeicaException(resp.ReturnCode, "Measurement through customer aborted."),
                GRC.SHUT_DOWN => throw new LeicaException(resp.ReturnCode, "System power off through customer."),
                _ => Successful(resp.ReturnCode) ? resp : resp
            });
            hzAngle = double.Parse(resp.Values[0].ToString());
            vAngle = double.Parse(resp.Values[1].ToString());
            accuracyAngle = double.Parse(resp.Values[2].ToString());
            crossIncl = double.Parse(resp.Values[3].ToString());
            lengthIncl = double.Parse(resp.Values[4].ToString());
            accuracyIncl = double.Parse(resp.Values[5].ToString());
            slopeDist = double.Parse(resp.Values[6].ToString());
            distTime = double.Parse(resp.Values[7].ToString());
            return true;
        }

        #endregion THEODOLITE MEASUREMENT AND CALCULATION (TMC CONF)

        #region MEASUREMENT CONTROL FUNCTIONS (TMC CONF)

        /// <summary> Carrying out a distance measurement.</summary>
        /// <remarks> This function carries out a distance measurement according to the TMC measurement mode like single distance, tracking,... . Please note that this command does not output any values (distances). In order to get the values you have to use other measurement functions such as TMC_GetCoordinate, TMC_GetSimpleMea, TMC_GetFullMeas or else TMC_GetAngle.<br/>The result of the distance measurement is kept in the instrument and is valid to the next TMC_DoMeasure command where a new distance is requested or the distance is clear by the measurement program TMC_CLEAR.<br/>
        /// <b>Note:</b> If you perform a distance measurement with the measure program TMC_DEF_DIST, the distance sensor will work with the set EDM mode, see TMC_SetEdmMode.</remarks>
        /// <param name="command"> TMC measurement mode.</param>
        /// <param name="mode"> Inclination sensor measurement mode.</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 TMC_DoMeasure </example>
        [COMF]
        public bool TMC_DoMeasure(TMC_MEASURE_PRG command, TMC_INCLINE_PRG mode) =>
            Call("%R1Q,2008:", command, mode, (resp) => Successful(resp.ReturnCode));

        /// <summary> Inputing a slope distance and height offset.</summary>
        /// <remarks> This function is used to input manually measured slope distance and height offset for a following measurement. Additionally an inclination measurement and an angle measurement are carried out to determine the co-ordinates of target.The V-angle is corrected to π/2 or 3⋅π/2 in dependence of the instrument’s face because of the manual input.<br/>After this command the previous measured distance is cleared</remarks>
        /// <param name="slopeDistance"> Slope distance [m].</param>
        /// <param name="hgtOffset"> Height offset [m].</param>
        /// <param name="mode"> Inclination sensor measurement mode.</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 TMC_SetHandDist </example>
        [COMF]
        public bool TMC_SetHandDist(double slopeDistance, double hgtOffset, TMC_INCLINE_PRG mode) =>
            Call("%R1Q,2019:", slopeDistance, hgtOffset, mode, (resp) => (resp.ReturnCode) switch
            {
                GRC.TMC_ACCURACY_GUARANTEE => throw new LeicaException(resp.ReturnCode, "Accuracy is not guaranteed, because the result are consist of measuring data which accuracy could not be verified by the system. Co-ordinates are available."),
                GRC.TMC_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "The results are not corrected by all active sensors. Coordinates are available. In order to secure which correction is missing use the both functions TMC_IfDataAzeCorrError and TMC_IfDataIncCorrError."),
                GRC.TMC_ANGLE_OK => throw new LeicaException(resp.ReturnCode, "Angle values okay, but no valid distance. Co-ordinates are not available."),
                GRC.TMC_ANGLE_NO_ACC_GUARANTY => throw new LeicaException(resp.ReturnCode, "Only the angle measurement is valid but its accuracy cannot be guaranteed (the tilt measurement is not available)."),
                GRC.TMC_ANGLE_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "No distance data available but angle data are valid. The return code is equivalent to the GRC_TMC_NO_FULL_CORRECTION and relates to the angle data. Co-ordinates are not available. Perform a distance measurement first before you call this function."),
                GRC.TMC_DIST_ERROR => throw new LeicaException(resp.ReturnCode, "No measuring, because of missing target point, co-ordinates are not available. Aim target point and try it again."),
                GRC.TMC_DIST_PPM => throw new LeicaException(resp.ReturnCode, "No distance measurement respectively no distance data because of wrong EDM settings. Co-ordinates are not available."),
                GRC.TMC_ANGLE_ERROR => throw new LeicaException(resp.ReturnCode, "Angle or inclination measurement error. Check inclination modes in commands."),
                GRC.TMC_BUSY => throw new LeicaException(resp.ReturnCode, "TMC resource is locked respectively TMC task is busy. Repeat measurement."),
                GRC.ABORT => throw new LeicaException(resp.ReturnCode, "Measurement through customer aborted."),
                GRC.SHUT_DOWN => throw new LeicaException(resp.ReturnCode, "System power off through customer."),
                _ => Successful(resp.ReturnCode)
            });

        #endregion MEASUREMENT CONTROL FUNCTIONS (TMC CONF)

        #region DATA SETUP FUNCTIONS

        /// <summary> Returning the current reflector height.</summary>
        /// <remarks> This function returns the current reflector height.</remarks>
        /// <returns> Current reflector height [m].</returns>
        /// <example> mod3 call 000001 TMC_GetHeight </example>
        [COMF]
        public double TMC_GetHeight() => CallGet<double>("%R1Q,2011:");

        /// <summary> Setting a new reflector height.</summary>
        /// <remarks> This function sets a new reflector height.</remarks>
        /// <param name="height"> New reflector height [m].</param>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 TMC_SetHeight </example>
        [COMF]
        public bool TMC_SetHeight(double height) =>
            Call("%R1Q,2012:", height, (resp) => (resp.ReturnCode) switch
            {
                GRC.TMC_BUSY => throw new LeicaException(resp.ReturnCode, "TMC resource is locked respectively TMC task is busy. The reflector height is not set. Repeat measurement."),
                GRC.IVPARAM => throw new LeicaException(resp.ReturnCode, "A reflector height less than 10m or greater than 100m is entered. Invalid parameter."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> Getting the atmospheric correction parameters.</summary>
        /// <remarks> This function is used to get the parameters for the atmospheric correction.</remarks>
        /// <returns> Atmospheric Correction Data.</returns>
        /// <example> mod3 call 000001 TMC_GetAtmCorr </example>
        [COMF]
        public TMC_ATMOS_TEMPERATURE? TMC_GetAtmCorr() => Call("%R1Q,2029:",
            resp => Successful(resp.ReturnCode) && resp.Values.Length == 4 ? new TMC_ATMOS_TEMPERATURE
            {
                Lambda = double.Parse(resp.Values[0].ToString()),
                Pressure = double.Parse(resp.Values[0].ToString()),
                DryTemperature = double.Parse(resp.Values[0].ToString()),
                WetTemperature = double.Parse(resp.Values[0].ToString())
            } : default);

        /// <summary> Setting the atmospheric correction parameters.</summary>
        /// <remarks> This function is used to set the parameters for the atmospheric correction.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 TMC_SetAtmCorr </example>
        [COMF]
        public bool TMC_SetAtmCorr(TMC_ATMOS_TEMPERATURE atm) =>
            Call("%R1Q,2028:", atm.Lambda, atm.Pressure, atm.DryTemperature, atm.WetTemperature, (resp) => Successful(resp.ReturnCode));

        /// <summary> Orientating the instrument in hz-direction.</summary>
        /// <remarks> This function is used to orientate the instrument in Hz direction. It is a combination of an angle measurement to get the Hz offset and afterwards setting the angle Hz offset in order to orientates onto a target.Before the new orientation can be set an existing distance must be cleared(use TMC_DoMeasure with the command = TMC_CLEAR).</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 TMC_SetOrientation </example>
        [COMF]
        public bool TMC_SetOrientation(double hzOrientation) =>
            Call("%R1Q,2113:", hzOrientation, (resp) => (resp.ReturnCode) switch
            {
                GRC.TMC_ACCURACY_GUARANTEE => throw new LeicaException(resp.ReturnCode, "Accuracy is not guaranteed, because the result are consist of measuring data which accuracy could not be verified by the system. Co-ordinates are available."),
                GRC.TMC_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "The results are not corrected by all active sensors. Coordinates are available. In order to secure which correction is missing use the both functions TMC_IfDataAzeCorrError and TMC_IfDataIncCorrError."),
                GRC.TMC_ANGLE_OK => throw new LeicaException(resp.ReturnCode, "Angle values okay, but no valid distance. Co-ordinates are not available."),
                GRC.TMC_ANGLE_NO_ACC_GUARANTY => throw new LeicaException(resp.ReturnCode, "Only the angle measurement is valid but its accuracy cannot be guaranteed (the tilt measurement is not available)."),
                GRC.TMC_ANGLE_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "No distance data available but angle data are valid. The return code is equivalent to the GRC_TMC_NO_FULL_CORRECTION and relates to the angle data. Co-ordinates are not available. Perform a distance measurement first before you call this function."),
                GRC.TMC_DIST_ERROR => throw new LeicaException(resp.ReturnCode, "No measuring, because of missing target point, co-ordinates are not available. Aim target point and try it again."),
                GRC.TMC_DIST_PPM => throw new LeicaException(resp.ReturnCode, "No distance measurement respectively no distance data because of wrong EDM settings. Co-ordinates are not available."),
                GRC.TMC_ANGLE_ERROR => throw new LeicaException(resp.ReturnCode, "Angle or inclination measurement error. Check inclination modes in commands."),
                GRC.TMC_BUSY => throw new LeicaException(resp.ReturnCode, "TMC resource is locked respectively TMC task is busy. Repeat measurement."),
                GRC.ABORT => throw new LeicaException(resp.ReturnCode, "Measurement through customer aborted."),
                GRC.SHUT_DOWN => throw new LeicaException(resp.ReturnCode, "System power off through customer."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> Getting the prism constant.</summary>
        /// <remarks> This function is used to get the prism constant.</remarks>
        /// <returns> Prism constant [m].</returns>
        /// <example> mod3 call 000001 TMC_GetPrismCorr </example>
        [COMF]
        public double TMC_GetPrismCorr() => CallGet<double>("%R1Q,2023:");

        /// <summary> Getting the refraction coefficient.</summary>
        /// <remarks> This function is used to get the refraction coefficient for correction of measured height difference.</remarks>
        /// <returns> Refraction control data.</returns>
        /// <example> mod3 call 000001 TMC_GetRefractiveCorr </example>
        [COMF]
        public TMC_REFRACTION? TMC_GetRefractiveCorr() => Call("%R1Q,2031:",
            resp => Successful(resp.ReturnCode) && resp.Values.Length == 3 ? new TMC_REFRACTION
            {
                RefOn = resp.Values[0].Equals(1L) ? ON_OFF_TYPE.ON : ON_OFF_TYPE.OFF,
                EarthRadius = double.Parse(resp.Values[1].ToString()),
                RefractiveScale = double.Parse(resp.Values[2].ToString())
            } : default);

        /// <summary> Setting the refraction coefficient.</summary>
        /// <remarks> This function is used to set the refraction distortion coefficient for correction of measured height difference.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 TMC_SetRefractiveCorr </example>
        [COMF]
        public bool TMC_SetRefractiveCorr(TMC_REFRACTION refractive) =>
            Call("%R1Q,2030:", refractive.RefOn, refractive.EarthRadius, refractive.RefractiveScale, (resp) => (resp.ReturnCode) switch
            {
                GRC.TMC_BUSY => throw new LeicaException(resp.ReturnCode, "TMC resource is locked respectively TMC task is busy. The refraction distortion factor is not set. Repeat measurement."),
                GRC.TMC_NO_FULL_CORRECTION => throw new LeicaException(resp.ReturnCode, "Wrong values entered."),
                GRC.IVRESULT => throw new LeicaException(resp.ReturnCode, "Wrong values entered."),
                GRC.SET_SETINCOMPLETE => throw new LeicaException(resp.ReturnCode, "Invalid number of parameters."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> Getting the refraction model.</summary>
        /// <remarks> This function is used to get the current refraction model. Note that changing the refraction method is not indicated on the instrument’s interface.</remarks>
        /// <returns> Refraction data:<br/>Method = 1 means method 1 (for the rest of the world).<br/>Method = 2 means method 2 (for Australia).</returns>
        /// <example> mod3 call 000001 TMC_GetRefractiveMethod </example>
        [COMF]
        public int? TMC_GetRefractiveMethod() => CallGet<int>("%R1Q,2091:");

        /// <summary> Setting the refraction model.</summary>
        /// <remarks> This function is used to set the refraction model.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 TMC_SetRefractiveMethod </example>
        [COMF]
        public bool TMC_SetRefractiveMethod(int method) =>
            Call("%R1Q,2090:", method, (resp) => (resp.ReturnCode) switch
            {
                GRC.TMC_BUSY => throw new LeicaException(resp.ReturnCode, "TMC resource is locked respectively TMC task is busy. The refraction distortion factor is not set. Repeat measurement."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> Getting the station coordinates of the instrument.</summary>
        /// <remarks> This function is used to get the station coordinates of the instrument.</remarks>
        /// <returns> Instrument station co-ordinates [m].</returns>
        /// <example> mod3 call 000001 TMC_GetStation </example>
        [COMF]
        public TMC_STATION? TMC_GetStation() => Call("%R1Q,2009:",
            resp => Successful(resp.ReturnCode) && resp.Values.Length == 3 ? new TMC_STATION
            {
                E0 = double.Parse(resp.Values[0].ToString()),
                N0 = double.Parse(resp.Values[1].ToString()),
                H0 = double.Parse(resp.Values[2].ToString()),
                Hi = double.Parse(resp.Values[3].ToString())
            } : default);

        /// <summary> Setting the station coordinates of the instrument.</summary>
        /// <remarks> This function is used to set the station coordinates of the instrument.</remarks>
        /// <returns> Execution successful.</returns>
        /// <example> mod3 call 000001 TMC_SetStation </example>
        [COMF]
        public bool TMC_SetStation(TMC_STATION station) =>
            Call("%R1Q,2010:", station.E0, station.N0, station.H0, station.Hi, (resp) => (resp.ReturnCode) switch
            {
                GRC.TMC_BUSY => throw new LeicaException(resp.ReturnCode, "TMC resource is locked respectively TMC task is busy. The refraction distortion factor is not set. Repeat measurement."),
                _ => Successful(resp.ReturnCode)
            });

        /// <summary> Getting the atmospheric ppm correction factor.</summary>
        /// <remarks> This function retrieves the atmospheric ppm value.</remarks>
        /// <returns> Atmospheric ppm correction factor.</returns>
        /// <example> mod3 call 000001 TMC_GetAtmPpm </example>
        [COMF]
        public double? TMC_GetAtmPpm() => CallGet<double>("%R1Q,2151:");

        /// <summary> Setting the atmospheric ppm correction factor.</summary>
        /// <remarks> This function is used to set the atmospheric ppm value.</remarks>
        /// <returns> Atmospheric ppm correction factor.</returns>
        /// <example> mod3 call 000001 TMC_SetAtmPpm </example>
        [COMF]
        public bool TMC_SetAtmPpm(double ppmA) => CallSet("%R1Q,2148:", ppmA);

        /// <summary> Getting the geometric ppm correction factor.</summary>
        /// <remarks> This function is used to get the station coordinates of the instrument.</remarks>
        /// <param name="geomUseAutomatic"> Current state of the Geometric ppm calculation switch (automatic or manual).</param>
        /// <param name="scaleFactorCentralMeridian"> Scale factor on central meridian.</param>
        /// <param name="offsetCentralMeridian"> Offset from central meridian [m].</param>
        /// <param name="heightReductionPPM"> ppm value due to height above reference.</param>
        /// <param name="individualPPM"> Individual ppm value.</param>
        /// <returns> Instrument station co-ordinates [m].</returns>
        /// <example> mod3 call 000001 TMC_GetGeoPpm </example>
        [COMF]
        public bool TMC_GetGeoPpm(out int geomUseAutomatic, double scaleFactorCentralMeridian, double offsetCentralMeridian, double heightReductionPPM, double individualPPM)
        {
            var resp = Call("%R1Q,2154:", resp => resp);
            geomUseAutomatic = (int)(long)resp.Values[0];
            scaleFactorCentralMeridian = double.Parse(resp.Values[1].ToString());
            offsetCentralMeridian = double.Parse(resp.Values[2].ToString());
            heightReductionPPM = double.Parse(resp.Values[3].ToString());
            individualPPM = double.Parse(resp.Values[4].ToString());
            return resp.Values.Length == 5;
        }

        #endregion DATA SETUP FUNCTIONS

        #region CLIENT SPECIFIC GEOCOM FUNCTIONS
        /* The following functions are not applicable to the ASCII protocol, because these functions influence the behaviour of the client application only. */

        /// <summary> [SAMPLE] </summary>
        public string DownloadFile()
        {
            var numOfBlocks = FTR_SetupDownload(FTR_DEVICETYPE.FTR_DEVICE_PCPARD, FTR_FILETYPE.FTR_FILE_IMAGES, "image000.jpg", FTR_BLOCK.FTR_MAX_BLOCKSIZE);
            string file = string.Empty;
            for (int blockNumber = 1; blockNumber <= numOfBlocks; blockNumber++)
            {
                var cnt = FTR_Download(blockNumber, out var block);
                if (cnt != 0)
                {
                    file += block;
                    continue;
                }
                break;
            }
            return file;
        }

        #endregion CLIENT SPECIFIC GEOCOM FUNCTIONS

        #region Nested types

        T? CallExperimental<T>(string funcName, params object[] args)
        {
            if (_api.Functions.Any(f => f.Name.Equals(funcName, StringComparison.OrdinalIgnoreCase)))
            {
                var extfunc = _api.Functions.First(f => f.Name.Equals(funcName, StringComparison.OrdinalIgnoreCase));
                var resp = Request(RequestString(extfunc.Call, args));
                if (extfunc.Return.Any(e => e.Code == resp.ReturnCode))
                {
                    var rc = extfunc.Return.First(e => e.Code == resp.ReturnCode);
                    throw new LeicaException(rc.Code, rc.Text);
                }
                if (resp.Values.Length == 1)
                    return (T)resp.Values[0];
            }
            return default;
        }

        internal struct GrcFunctionCollection
        {
            public GrcFunction[] Functions;
            public GrcReturn[] Return;
        }

        internal struct GrcFunction
        {
            public string Name;
            public string Call;
            public GrcReturn[] Return;
        }

        internal struct GrcReturn
        {
            public GRC Code;
            public string Text;
        }

        #endregion Nested types
    }
}
