//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TMessage –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    #region Using
    using System;
    #endregion Using

    /// <summary> Системное сообщение.</summary>
    [System.Diagnostics.DebuggerDisplay("Msg={MSG.ToString(Msg)}, {LParam}, {HParam}, {Data}")]
    public struct TMessage
    {
        public static TMessage Empty = new TMessage(0);

        /// <summary> Тип сообщения </summary>
        public int Msg;
        /// <summary> Параметр 1 </summary>
        public long LParam;
        /// <summary> Параметр 2 </summary>
        public long HParam;
        /// <summary> Возврат - результат </summary>
        public long Result;
        /// <summary> Произвольные данные </summary>
        public object Data;

        public TMessage(int msg)
            : this(msg, 0, 0, null) { }

        public TMessage(int msg, object data)
            : this(msg, 0, 0, data) { }

        public TMessage(int msg, long lparam, object data)
            : this(msg, lparam, 0, data) { }

        public TMessage(int msg, long lparam, long hparam, long result, object data)
            : this(msg, lparam, hparam, data) => Result = result;

        public TMessage(int msg, long lparam, long hparam, object data)
        {
            Msg = msg;
            LParam = lparam;
            HParam = hparam;
            Result = 0;
            Data = data;
        }
    }

    /// <summary> Константы. Сообщения системной очереди сообщений.</summary>
    public static class MSG
    {
        /// <summary> Выполняется после окончания запуска всех служб.</summary>
        public const int StartServer = 0x0001;
        /// <summary> Выполняется при остановке объектового сервере.</summary>
        public const int StopServer = 0x0002;
        /// <summary> Установить модуль/службу в Систему.</summary>
        public const int InstallModule = 0x0003;
        /// <summary> Удалить модуль/службу из Системы.</summary>
        public const int UninstallModule = 0x0004;
        /// <summary> Выполняется после запуска главного процеса выполнения.</summary>
        public const int StartRuntime = 0x0009;

        /// <summary> LParam = Id процесса.</summary>
        public const int ErrorMessage = 0x000c;
        /// <summary> LParam = Id процесса.</summary>
        public const int CriticalMessage = 0x000d;
        /// <summary> LParam = Id процесса.</summary>
        public const int WarningMessage = 0x000e;
        /// <summary> LParam = Id процесса.</summary>
        public const int InformMessage = 0x000f;

        /// <summary> Вывод в окно терминала по протоколу <em>Telnet</em>. HParam = ИД session.</summary>
        /// <remarks> LParam = ИД процесса (модуля); HParam = ИД терминальной сессии; Data = text/int - 0x484F4C44 HOLD, 0x46524545 FREE.</remarks>
        public const int Terminal = 0x0014;

        /// <summary> Все сообщения.</summary>
        public const int All = 0x55555555;
    }
}