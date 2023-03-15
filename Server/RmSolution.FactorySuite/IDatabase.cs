﻿//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: IDatabase –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System.Data;
    #endregion Using

    public interface IDatabase
    {
        /// <summary> Наименование приложения.</summary>
        string ApplicationName { get; set; }
        /// <summary> Схема по-умолчанию.</summary>
        string DefaultScheme { get; }
        /// <summary> Возвращает версию базы данных.</summary>
        string Version { get; }

        /// <summary> Открыть соединение с базой данных.</summary>
        IDatabase Open();
        /// <summary> Закрыть соединение с базой данных.</summary>
        void Close();
        /// <summary> Возвращает таблицу из базы данных.</summary>
        DataTable Query(string statement, params object[] args);
        /// <summary> Возвращает список указанного типа с маппингом из базы данных.</summary>
        IEnumerable<T> Query<T>(string statement, params object[] args);
        /// <summary> Возвращает единственное значение из БД. Первую колонку первой записи.</summary>
        object Scalar(string statement, params object[] args);
        /// <summary> Возвращает единственное значение из БД указанного типа. Первую колонку первой записи.</summary>
        T Scalar<T>(string statement, params object[] args);
    }
}
