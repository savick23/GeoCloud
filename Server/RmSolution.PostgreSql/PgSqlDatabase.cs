// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: PgSqlDatabase –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System.Data;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.Extensions.Logging;
    using Npgsql;
    using RmSolution.DataAnnotations;
    using RmSolution.Runtime;
    #endregion Using

    public sealed class PgSqlDatabase : DatabaseFactorySuite
    {
        #region Declarations

        const int SQLERR_FAILED = -2147467259;

        static readonly Dictionary<string, string> _pgtypes = new Dictionary<string, string>()
        {
            { @"bit", "boolean" },
            { @"float(4)", "real" },
            { @"float(8)", "double precision"},
            { @"tinyint", "smallint" },
            { @"\btimestamp\b", "bytea" },
            { @"rowversion", "bigint" },
            { @"datetime", "timestamp(3) without time zone" },
            { @"nvarchar\(max\)", "text" },
            { @"nvarchar", "varchar" },
            { @"varbinary\(\w+\)", "bytea" },
            { @"varbinary", "bytea" }
        };

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

        public override void CreateDatabase(IMetadata metadata, Action<string> message)
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

                    CreateEnvironment(newdb, metadata.Entities, message);
                    InitDatabase(newdb, metadata.Entities, message);
                }
            }
            finally
            {
                newdb.Close();
            }
        }

        #endregion IDatabaseFactory implementation

        #region Database objects

        public override List<string> Schemata() =>
            Query("SELECT schema_name FROM INFORMATION_SCHEMA.SCHEMATA ORDER BY 1")
                .Rows.Cast<DataRow>().Select(r => r[0]?.ToString() ?? string.Empty).ToList();

        public override List<DbTable> Tables() =>
            Query("SELECT table_schema,table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_schema NOT IN('pg_catalog','information_schema') AND table_type='BASE TABLE' ORDER BY 1")
                .Rows.Cast<DataRow>().Select(r => new DbTable(string.Concat(r[0], ".", r[1]),
                    new List<DbColumn>())).ToList();

        protected override void ColumnDefinitionAdapter(StringBuilder sqlcmd, string[] definition)
        {
            for (int i = 0; i < definition.Length; i++)
            {
                var defn = definition[i];
                if (i == 0)
                {
                    sqlcmd.Append(LQ);
                    sqlcmd.Append(defn);
                    sqlcmd.Append(RQ);
                }
                else if (i == 1)
                {
                    sqlcmd.Append(' ');
                    _pgtypes.ToList().ForEach(a => defn = Regex.Replace(defn, a.Key, a.Value).Replace("(0)", string.Empty));
                    sqlcmd.Append(defn);
                }
                else if (defn.StartsWith("IDENTITY"))
                {
                    var prms = Regex.Matches(defn, @"\d+");
                    sqlcmd.Append(" GENERATED BY DEFAULT AS IDENTITY (START WITH ");
                    sqlcmd.Append(prms.Count > 0 ? prms[0].Value : "1");
                    sqlcmd.Append(" INCREMENT BY ");
                    sqlcmd.Append(prms.Count > 1 ? prms[1].Value : "1");
                    sqlcmd.Append(')');
                }
                else if (defn.StartsWith("JSON"))
                {
                    sqlcmd.Append(" GENERATED ALWAYS AS ((\"")
                        .Append(Regex.Match(defn, @"(?<=JSON\()\w+").Value)
                        .Append("\"::json->>'")
                        .Append(Regex.Match(defn, @"\w+", RegexOptions.RightToLeft).Value)
                        .Append("')::").Append(definition[1].Contains("varchar") ? "text" : definition[1]).Append(") STORED");
                }
                else
                {
                    sqlcmd.Append(' ');
                    sqlcmd.Append(defn);
                }
            }
        }

        protected override void IndexDefinitionAdapter(StringBuilder sqlcmd, string tableName, string[] columns)
        {
            var indexName = string.Join("_", columns.Select(f => Regex.Match(f, @"^\w+").Value.ToUpper()));
            sqlcmd.Append($"CREATE INDEX IDX_{Regex.Match(tableName, @"(?<="").*?(?="")").Value.ToUpper()}_{indexName} ON {tableName} ");
            IndexDefinition(sqlcmd, columns);
        }

        #endregion Database objects
    }
}
