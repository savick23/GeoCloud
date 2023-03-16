//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: DbException –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    #endregion Using

    public class TDbException : DbException
    {
        public const int NO_INSTANCE = 1;
        public const int NO_DATABASE = 2;

        public override string SqlState { get; }

        static readonly Dictionary<int, string> _msgs = new Dictionary<int, string>()
        {
            {NO_INSTANCE, "Не запущен сервер базы данных." },
            {NO_DATABASE, "Отсутвует база данных." }
        };

        public TDbException(int errorCode, Exception innerException = null)
            : base(_msgs.ContainsKey(errorCode) ? _msgs[errorCode] : innerException?.Message ?? "Неизвестная ошибка!", innerException)
        {
            HResult = errorCode;
        }

        public TDbException(string sqlState, string statement, Exception exception)
          : base($"{statement}{FullMessage(exception)}", exception)
        {
            SqlState = sqlState;
        }

        static string FullMessage(Exception ex) => $"\r\n{ex.Message}{(ex.InnerException != null ? $"{FullMessage(ex.InnerException)}" : "")}";
    }

    public class TDbNotFoundException : TDbException
    {
        public TDbNotFoundException(int errorCode, Exception innerException = null)
            : base(errorCode, innerException)
        {
        }
    }
}
