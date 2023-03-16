//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: DbException –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System;
    using System.Collections.Generic;
    #endregion Using

    public class DbException : Exception
    {
        public const int NO_INSTANCE = 1;
        public const int NO_DATABASE = 2;

        static readonly Dictionary<int, string> _msgs = new Dictionary<int, string>()
        {
            {NO_INSTANCE, "Не запущен сервер базы данных." },
            {NO_DATABASE, "Отсутвует база данных." }
        };

        public int ErrorCode { get; }

        public DbException(int errorCode, Exception innerException = null)
            : base(_msgs.ContainsKey(errorCode) ? _msgs[errorCode] : innerException?.Message ?? "Неизвестная ошибка!", innerException)
        {
            ErrorCode = errorCode;
        }
    }

    public class DbNotFoundException : DbException
    {
        public DbNotFoundException(int errorCode, Exception innerException = null)
            : base(errorCode, innerException)
        {
        }
    }
}
