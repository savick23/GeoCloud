//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: DatabaseFactorySuite –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System;
    using System.Data.Common;
    #endregion Using

    public abstract class DatabaseFactorySuite : IDatabase
    {
        #region Declarations

        protected DbConnection _conn;

        #endregion Declarations

        #region Properties

        public string ApplicationName { get; set; } = "rmgeo";
        public virtual string DefaultScheme { get; }
        public virtual string Version { get; }

        #endregion Properties

        #region IDatabase implementation

        public virtual IDatabase Open() => throw new NotImplementedException();

        #endregion IDatabase implementation
    }

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
    }
}
