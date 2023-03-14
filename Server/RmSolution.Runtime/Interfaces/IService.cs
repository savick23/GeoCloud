//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». Smart System Platform 3.1. Все права защищены.
// Описание: IService –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    /// <summary> Внутренний процес.</summary>
    public interface IService
    {
        /// <summary> Идентификатор модуля.</summary>
        long Id { get; set; }
        /// <summary> Наименование модуля (процесса).</summary>
        string Name { get; set; }
        /// <summary> Идентификатор запущенного процесса.</summary>
        int ProcessId { get; set; }
        /// <summary> Текущий статус выполнения.</summary>
        RuntimeStatus Status { get; }
        /// <summary> Версия модуля.</summary>
        Version Version { get; }
    }

    /// <summary> Признак автозапуска модуля при старте.</summary>
    public interface IStartup
    { }
}