//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». Smart System Platform 3.1. Все права защищены.
// Описание: IModule – Интерфейс модуля, службы сервера прмложений.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    public interface IModule : IDisposable
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

        /// <summary> Перечень обрабатываемых сообщений. Подписка. Инициализация.</summary>
        int[] Subscribe { get; set; }

        /// <summary> Асинхроный процесс выполнения модуля.</summary>
        void ProcessMessage(ref TMessage m);

        void Start();
        void Stop();
        void Kill();

        /// <summary> Последняя ошибка.</summary>
        Exception LastError { get; set; }
    }

    /// <summary> Признак автозапуска модуля при старте.</summary>
    public interface IStartup
    { }
}