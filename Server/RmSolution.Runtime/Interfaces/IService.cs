//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». Smart System Platform 3.1. Все права защищены.
// Описание: IService –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    /// <summary> Внутренний процес.</summary>
    public interface IMicroService
    {
        /// <summary> Идентификатор модуля.</summary>
        long Id { get; set; }
        /// <summary> Идентификатор запущенного процесса.</summary>
        int ProcessId { get; set; }
        /// <summary> Текущий статус выполнения.</summary>
        RuntimeStatus Status { get; }
    }

    /// <summary> Признак автозапуска модуля при старте.</summary>
    public interface IStartup
    { }
}