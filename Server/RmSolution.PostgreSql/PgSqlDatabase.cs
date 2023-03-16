// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: PgSqlDatabase –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System.Data;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;
    using Npgsql;
    using RmSolution.Runtime;
    #endregion Using

    public sealed class PgSqlDatabase : DatabaseFactorySuite
    {
        #region Declarations

        const int SQLERR_FAILED = -2147467259;

        string _connstr;

        #endregion Declarations

        #region Properties

        public override string DefaultScheme => "public";
        public override string Version => Scalar("SELECT VERSION()")?.ToString();

        #endregion Properties

        public PgSqlDatabase(string connectionString) : base(connectionString)
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
                if (ex.ErrorCode == SQLERR_FAILED && ex.InnerException is SocketException)
                    throw new TDbException(TDbException.NO_INSTANCE, ex);
                else if (ex.ErrorCode == SQLERR_FAILED && ex is PostgresException sql && sql.SqlState == "3D000")
                    throw new TDbNotFoundException(TDbException.NO_DATABASE, ex);
                else
                    throw;
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
                throw new TDbException(ex.SqlState, stmt, ex);
            }
            return ds.Tables.Count > 0 ? ds.Tables[0] : null;
        }

        public override void Exec(string statement, params object[] args)
        {
            try
            {
                lock (SyncRoot)
                {
                    if (args.Length > 0) statement = string.Format(statement, args);
                    using var cmd = new NpgsqlCommand(statement, (NpgsqlConnection)_conn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (NpgsqlException ex)
            {
                throw new TDbException(ex.SqlState, string.Format(statement, args), ex);
            }
        }

        #endregion IDatabase implementation

        #region IDatabaseFactory implementation

        public override void CreateDatabase()
        {
            var dbname = DatabaseName;
            var newdb = new PgSqlDatabase(Regex.Replace(_connstr, @"DATABASE=.*?[;$]", "Database=postgres;", RegexOptions.IgnoreCase));
            try
            {
                newdb.Open();
                if (!newdb.Query("SELECT datname FROM pg_database").Rows.Cast<DataRow>().Any(r => r[0].ToString().ToUpper() == dbname))
                {
                    // TEMPLATE='template0' - если указать другой или опустить получим ошибку: new collation (Russian_Russia.1251) is incompatible with the collation of the template database (English_United States.1252)
                    newdb.Exec(@$"CREATE DATABASE {LQ}{dbname}{RQ} WITH OWNER=postgres ENCODING='UTF-8' LC_COLLATE='ru_RU.UTF-8' LC_CTYPE='ru_RU.UTF-8' TABLESPACE=pg_default CONNECTION LIMIT=-1 TEMPLATE='template0';");
                    newdb.Close();

                    Thread.Sleep(5000); // задержка на инициализацию БД
                    newdb = new PgSqlDatabase(_connstr);
                    newdb.Open();

                    CreateEnvironment(newdb);
                }
            }
            finally
            {
                newdb.Close();
            }
        }

        #endregion IDatabaseFactory implementation
    }
}
