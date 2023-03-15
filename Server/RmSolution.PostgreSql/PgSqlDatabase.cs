// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: PgSqlDatabase –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System.Data;
    using Npgsql;
    using RmSolution.Runtime;
    #endregion Using

    public sealed class PgSqlDatabase : DatabaseFactorySuite
    {
        #region Declarations

        string _connstr;

        #endregion Declarations

        #region Properties

        public override string DefaultScheme => "public";
        public override string Version => Scalar("SELECT VERSION()")?.ToString();

        #endregion Properties

        public PgSqlDatabase(string connectionString)
        {
            _connstr = connectionString;
        }

        #region IDatabase implementation

        public override IDatabase Open()
        {
            _conn = new NpgsqlConnection(string.Concat(_connstr, string.IsNullOrWhiteSpace(ApplicationName) ? string.Empty : "Application Name=" + ApplicationName));
            try
            {
                _conn.Open();
            }
            catch (NpgsqlException ex)
            {
                //if (ex.ErrorCode == SQLERR_FAILED && ex.InnerException is SocketException)
                //    throw new DbException(DbException.NO_INSTANCE, ex);
                //else if (ex.ErrorCode == SQLERR_FAILED && ex is PostgresException sql && sql.SqlState == "3D000")
                //    throw new DbNotFoundException(DbException.NO_DATABASE, ex);
                //else
                //    throw;
            }
            return this;
        }

        public override DataTable Query(string statement, params object[] args)
        {
            var ds = new DataSet();
            var stmt = args.Length == 0 ? statement : string.Format(statement, args);
            try
            {
                lock (SyncRoot)
                {
                    using var cmd = new NpgsqlDataAdapter(stmt, (NpgsqlConnection)_conn);
                    cmd.Fill(ds);
                }
            }
            catch (NpgsqlException ex)
            {
                //throw new TDatabaseException(ex.SqlState, stmt, ex);
            }
            return ds.Tables.Count > 0 ? ds.Tables[0] : null;
        }

        #endregion IDatabase implementation
    }
}
