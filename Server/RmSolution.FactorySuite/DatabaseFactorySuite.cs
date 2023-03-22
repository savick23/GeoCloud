//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: DatabaseFactorySuite –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Text.RegularExpressions;
    using Dapper;
    using RmSolution.DataAnnotations;
    using RmSolution.Runtime;
    #endregion Using

    public abstract partial class DatabaseFactorySuite : IDatabase, IDatabaseFactory, IDisposable
    {
        #region Declarations

        const string INITFILE = "dbinit.smx";
#if DEBUG
        protected static readonly string CONFIG = Path.Combine(Path.GetDirectoryName(Environment.CommandLine), "config");
#else
        protected static readonly string CONFIG = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "config");
#endif
        protected DbConnection? _conn;

        static Dictionary<Type, string> _typemapping = new() {
            { typeof(Int32), "int NOT NULL" },
            { typeof(Int32?), "int NULL" },
            { typeof(Int64), "bigint NOT NULL" },
            { typeof(Int64?), "bigint NULL" },
            { typeof(DateTime), "datetime NOT NULL" },
            { typeof(DateTime?), "datetime NULL" }
        };

        /// <summary> Исключаем одновременный доступ (запрос) к данным базы данных.</summary>
        protected object SyncRoot = new();

        #endregion Declarations

        #region Properties

        public string? ApplicationName { get; set; } = "rmgeo";
        public virtual string? DefaultScheme { get; }
        public virtual string? Version { get; }
        public string? DatabaseName { get; }
        public virtual string LQ => "\"";
        public virtual string RQ => "\"";

        #endregion Properties

        public DatabaseFactorySuite(string connectionString)
        {
            DatabaseName = Regex.Match(connectionString, @"(?<=INITIAL CATALOG=|Database=).*?(?=;|$)", RegexOptions.IgnoreCase).Value;
        }

        #region IDatabase implementation

        public virtual IDatabase Open() => throw new NotImplementedException();
        public virtual DataTable Query(string statement, params object[] args) => throw new NotImplementedException();
        public virtual void Exec(string statement, params object[] args) => throw new NotImplementedException();
        public virtual IEnumerable<string> Schemata() => throw new NotImplementedException();
        public virtual IEnumerable<string> Tables() => throw new NotImplementedException();

        /// <summary> Создать базу данных с кодовой страницой UTF8.</summary>
        public virtual void CreateDatabase(TObjectCollection entities, Action<string> message) => throw new NotImplementedException();

        public virtual void Close()
        {
            _conn?.Close();
        }

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }

        public virtual IEnumerable<T> Query<T>(string statement, params object[] args) =>
            _conn.Query<T>(string.Format(statement, args));

        public virtual IEnumerable<T>? Query<T>()
        {
            var src = ((TableAttribute?)typeof(T).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault())?.Source;
            if (src != null)
                return _conn.Query<T>("SELECT * FROM " + SchemaTableName(src));

            return default;
        }

        public IEnumerable<dynamic>? Query(Type type)
        {
            var src = ((TableAttribute?)type.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault())?.Source;
            if (src != null)
                return _conn.Query("SELECT * FROM " + SchemaTableName(src));

            return default;
        }

        public async Task<IEnumerable<dynamic>?> QueryAsync(Type type)
        {
            var src = ((TableAttribute?)type.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault())?.Source;
            if (src != null)
                return await _conn.QueryAsync("SELECT * FROM " + SchemaTableName(src));

            return default;
        }

        public virtual object Scalar(string statement, params object[] args)
        {
            var tbl = Query(statement, args);         
            return tbl?.Rows.Count > 0 ? tbl.Rows[0][0] : null;
        }

        public virtual T Scalar<T>(string statement, params object[] args) =>
            Scalar(statement, args) is T res ? res : default;

        #endregion IDatabase implementation
    }
}
