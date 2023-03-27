//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: IDatabase –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    #region Using
    using System.Data;
    using Microsoft.Extensions.Logging;
    #endregion Using

    public interface IDatabase
    {
        /// <summary> Наименование приложения.</summary>
        string? ApplicationName { get; set; }
        /// <summary> Схема по-умолчанию.</summary>
        string? DefaultScheme { get; }
        /// <summary> Возвращает наименование базы данных.</summary>
        string? DatabaseName { get; }
        /// <summary> Возвращает версию базы данных.</summary>
        string? Version { get; }

        /// <summary> Открыть соединение с базой данных.</summary>
        IDatabase Open();
        /// <summary> Закрыть соединение с базой данных.</summary>
        void Close();
        /// <summary> Возвращает таблицу из базы данных.</summary>
        DataTable Query(string statement, params object[] args);
        /// <summary> Возвращает список указанного типа с маппингом из базы данных.</summary>
        IEnumerable<T> Query<T>(string statement, params object[] args);
        /// <summary> Возвращает список указанного типа с маппингом из базы данных.</summary>
        IEnumerable<T>? Query<T>();
        /// <summary> Возвращает список указанного типа с маппингом из базы данных.</summary>
        IEnumerable<dynamic>? Query(Type type);
        /// <summary> Возвращает список указанного типа с маппингом из базы данных.</summary>
        IEnumerable<object>? Query(Type type, string statement, params object[] args);
        /// <summary> Возвращает список указанного типа с маппингом из базы данных.</summary>
        Task<IEnumerable<dynamic>?> QueryAsync(Type type);
        /// <summary> Возвращает единственное значение из БД. Первую колонку первой записи.</summary>
        object Scalar(string statement, params object[] args);
        /// <summary> Возвращает единственное значение из БД указанного типа. Первую колонку первой записи.</summary>
        T Scalar<T>(string statement, params object[] args);
        /// <summary> Выполнить инструкцию базы данных.</summary>
        void Exec(string statement, params object[] args);
        /// <summary> Обновить данные в БД.</summary>
        void Update(object item);

        /// <summary> Возвращает список схем данных в текущей базе данных.</summary>
        IEnumerable<string> Schemata();
        /// <summary> Возвращает список таблиц в текущей базе данных.</summary>
        IEnumerable<string> Tables();
    }

    public interface IDatabaseFactory // For isolation
    {
        /// <summary> Создаёт новую базу данных.</summary>
        void CreateDatabase(TObjectCollection entities, Action<string> message);
        /// <summary> Временно.</summary>
        void UpdateDatabase(TObjectCollection entities, Action<string> message);
    }
}
