// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: MsSqlDatabase –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System.Data;
    using System.Threading;
    using Microsoft.Data.SqlClient;
    #endregion Using

    public sealed class MsSqlDatabase : DatabaseFactorySuite
    {
        #region Declarations

        string _connstr;

        #endregion Declarations

        #region Properties

        public override string DefaultScheme => "dbo";
        public override string Version => Scalar("SELECT @@VERSION")?.ToString();

        #endregion Properties

        public MsSqlDatabase(string connectionString)
        {
            _connstr = connectionString;
        }

        #region IDatabase implementation

        public override IDatabase Open()
        {
            _conn = new SqlConnection(string.Concat(_connstr, string.IsNullOrWhiteSpace(ApplicationName) ? string.Empty : "Application Name=" + ApplicationName));
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

        public override DataTable Query(string statement, params object[] args) =>
            Query(CommandBehavior.Default, statement, args);

        #endregion IDatabase implementation

        #region Private methods

        DataTable Query(CommandBehavior behavior, string statement, params object[] args)
        {
            DataTable res = new DataTable();
            SqlTransaction tran = null;
            lock (SyncRoot)
                try
                {
                    tran = ((SqlConnection)_conn).BeginTransaction();
                    try
                    {
                        res = QueryTran(tran, behavior, statement, args);
                        tran.Commit();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number != 15002) // Code cannot be executed within a transaction
                            throw;

                        tran.Rollback();
                        res = QueryTran(tran, behavior, statement, args);
                    }
                }
                catch (SqlException ex)
                {

                    if (tran != null)
                        try
                        {
                            tran.Rollback();
                        }
                        catch { }

                    res.Dispose();
                    if (ex.Number < 60000)
                    {
                        throw new Exception(string.Format(statement, args), ex);
                    }
                    else throw;
                }
            return res;
        }

        DataTable QueryTran(SqlTransaction tran, CommandBehavior behavior, string statement, params object[] args)
        {
            var res = new DataTable();
            using var cmd = new SqlCommand(string.Format(statement, args), (SqlConnection)_conn, tran);
            using var rd = cmd.ExecuteReader(behavior);
            res.Load(rd);
            return res;
        }

        #endregion Private methods
    }
}
