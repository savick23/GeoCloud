// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: MsSqlDatabase –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System.Data.Common;
    using Microsoft.Data.SqlClient;
    #endregion Using

    public sealed class MsSqlDatabase : DatabaseFactorySuite
    {
        #region Declarations

        string _connstr;

        #endregion Declarations

        #region Properties

        public override string DefaultScheme => "dbo";

        #endregion Properties

        public MsSqlDatabase(string connectionString)
        {
            _connstr = connectionString;
        }

        #region IDatabase implementation

        public override IDatabase Open()
        {
         //   _conn = new SqlConnection(string.Concat(_connstr, string.IsNullOrWhiteSpace(ApplicationName) ? string.Empty : ";Application Name=" + ApplicationName));
            _conn = new SqlConnection(_connstr);
            try
            {
                _conn.Open();
            }
            catch (SqlException ex)
            {
                //if (ex.ErrorCode == SQLERR_FAILED && ex.Number == SQLERR_NO_INSTANCE)
                //    throw new DbException(DbException.NO_INSTANCE, ex);
                //else if (ex.ErrorCode == SQLERR_FAILED && ex.Number == SQLERR_NO_DATABASE)
                //    throw new DbNotFoundException(DbException.NO_DATABASE, ex);
                //else
                //    throw;
            }
      //      ((SqlConnection)_conn).InfoMessage += new SqlInfoMessageEventHandler(OnInfoMessage);
            return this;
        }

        #endregion IDatabase implementation
    }
}
