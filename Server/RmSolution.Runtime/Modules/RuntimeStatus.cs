//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». Smart System Platform 3.1. Все права защищены.
// Описание: RuntimeStatus – Статусы выполнения служб и модулей.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    #region Using
    using System.ComponentModel;
    #endregion Using

    [Flags]
    public enum RuntimeStatus
    {
        [Description("Не установлен")]
        None = 0,
        [Description("Остановлена")]
        Stopped = 1,
        [Description("Попытка остановки")]
        StopPending = 2,
        [Description("Выполняется")]
        Running = 4,
        [Description("Попытка запуска")]
        StartPending = 8,
        [Description("Приостановлена")]
        Pause = 16,
        [Description("Сервисный режим")]
        Service = 32,
        [Description("Ошибка выполнения")]
        Failed = 64,

        Loop = Running | Failed | Service
    }
}