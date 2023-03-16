// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: MsSqlDatabase –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System.Data;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Xml.Linq;
    using Microsoft.Data.SqlClient;
    using RmSolution.Runtime;
    #endregion Using

    public sealed class MsSqlDatabase : DatabaseFactorySuite
    {
        #region Declarations

        const int SQLERR_FAILED = -2146232060;
        const int SQLERR_NO_INSTANCE = 2;
        const int SQLERR_NO_DATABASE = 4060;
        const int SQLERR_NO_PROCESS_PIPE = 233;

        string _connstr;

        #endregion Declarations

        #region Properties

        public override string DefaultScheme => "dbo";
        public override string Version => Scalar("SELECT @@VERSION")?.ToString().Split(new char[] { '\n' })[0].Trim();

        #endregion Properties

        public MsSqlDatabase(string connectionString) : base(connectionString)
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
                CreateEnvironment(this, (msg) => Console.WriteLine(msg));
            }
            catch (SqlException ex)
            {
                if (ex.ErrorCode == SQLERR_FAILED && ex.Number == SQLERR_NO_INSTANCE)
                    throw new TDbException(TDbException.NO_INSTANCE, ex);
                else if (ex.ErrorCode == SQLERR_FAILED && (ex.Number == SQLERR_NO_DATABASE || ex.Number == SQLERR_NO_PROCESS_PIPE))
                    throw new TDbNotFoundException(TDbException.NO_DATABASE, ex);
                else
                    throw;
            }
      //      ((SqlConnection)_conn).InfoMessage += new SqlInfoMessageEventHandler(OnInfoMessage);
            return this;
        }

        public override DataTable Query(string statement, params object[] args) =>
            Query(CommandBehavior.Default, statement, args);

        public override void Exec(string statement, params object[] args) =>
            Exec(CommandBehavior.Default, statement, args);

        #endregion IDatabase implementation

        #region IDatabaseFactory implementation

        public override void CreateDatabase(Action<string> message)
        {
            var dbname = DatabaseName;
            var newdb = new MsSqlDatabase(Regex.Replace(_connstr, @"INITIAL CATALOG=.*?[;$]", "Initial Catalog=master;", RegexOptions.IgnoreCase));
            try
            {
                newdb.Open();
                if (!newdb.Query("EXEC sp_databases").Rows.Cast<DataRow>().Any(r => r[0].ToString().ToUpper() == dbname))
                {
                    newdb.Exec(@$"CREATE DATABASE {LQ}{dbname}{RQ} COLLATE Cyrillic_General_100_CI_AS_SC_UTF8; ALTER DATABASE [{dbname}] SET RECOVERY SIMPLE;");
                    newdb.Close();
                    message($"Создана база данных {dbname}.");

                    Thread.Sleep(5000); // задержка на инициализацию БД
                    newdb = new MsSqlDatabase(_connstr);
                    newdb.Open();

                    CreateEnvironment(newdb, message);
                }
            }
            finally
            {
                newdb.Close();
            }
        }

        #endregion IDatabaseFactory implementation

        #region Database objects

        public override IEnumerable<string> Schemata() =>
            Query("SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA ORDER BY 1")
                .Rows.Cast<DataRow>().Select(r => r[0]?.ToString() ?? string.Empty).ToList();

        public override IEnumerable<string> Tables() =>
            Query("SELECT TABLE_SCHEMA+'.'+TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY 1")
                .Rows.Cast<DataRow>().Select(r => r[0]?.ToString() ?? string.Empty).ToList();

        #endregion Database objects

        #region Private methods

        DataTable Query(CommandBehavior behavior, string statement, params object[] args)
        {
            DataTable res = new();
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
                        throw new TDbException(ex.SqlState, string.Format(statement, args), ex);
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

        void Exec(CommandBehavior behavior, string statement, params object[] args)
        {
            lock (SyncRoot)
            {
                using var cmd = new SqlCommand(args.Length > 0 ? string.Format(statement, args) : statement, (SqlConnection)_conn);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    throw new TDbException(ex.SqlState, string.Format(statement, args), ex);
                }
            }
        }

        #endregion Private methods
    }
}
