//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: IRuntime – Главный интерфейс среды выполнения сервера приложений.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    public interface IRuntime
    {
        string GetWorkDirectory();

        /// <summary> Наименование сервера приложений.</summary>
        string Name { get; set; }
        /// <summary> Версия модуля.</summary>
        Version Version { get; }

        void Send(TMessage m);
        void Send(int type, long lparam);
        void Send(int type, long lparam, long hparam);
        void Send(int type, long lparam, long hparam, object data);
        /// <summary> Отправить сообщение с задержкой DELAY миллисекунд.</summary>
        int Send(int type, long lparam, long hparam, object data, long delay);
        /// <summary> Отправить сообщение с задержкой DELAY миллисекунд и периодом.</summary>
        int Send(int type, long lparam, long hparam, object data, long delay, long period);
        int Send(int type, long lparam, long hparam, object data, DateTime dateTime);

        /// <summary> Удалить задание.</summary>
        bool RemoveTask(int id);
        /// <summary> Сбросить задание.</summary>
        bool ResetTask(int id);
        bool Modify(int id, TMessage m, long delay);
        bool Modify(int id, TMessage m, long delay, long period);
        bool Modify(int id, TMessage m, DateTime dateTime);

        /// <summary> Отправить сообщение с задержкой DELAY миллисекунд.</summary>
        int Send(TMessage m, long delay);
        int Send(TMessage m, long delay, long period);
        int Send(TMessage m, DateTime dateTime);
    }
}