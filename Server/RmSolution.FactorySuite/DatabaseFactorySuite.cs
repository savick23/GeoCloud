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
    using System.Globalization;
    using System.Text;
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
            { typeof(TRefType), "bigint" },
            { typeof(TRefType?), "bigint" },
            { typeof(Int32), "int" },
            { typeof(Int32?), "int" },
            { typeof(Int64), "bigint" },
            { typeof(Int64?), "bigint" },
            { typeof(DateTime), "datetime" },
            { typeof(DateTime?), "datetime" },
            { typeof(string), "nvarchar({0})" }
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

        static DatabaseFactorySuite()
        {
            SqlMapper.AddTypeHandler(new TRefTypeTypeConverter());
        }

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
            var src = ((TObject?)typeof(T).GetCustomAttributes(typeof(TObject), true).FirstOrDefault())?.Source;
            if (src != null)
                return _conn.Query<T>("SELECT * FROM " + SchemaTableName(src));

            return default;
        }

        public IEnumerable<dynamic>? Query(Type type)
        {
            var src = ((TObject?)type.GetCustomAttributes(typeof(TObject), true).FirstOrDefault())?.Source;
            if (src != null)
                return _conn.Query("SELECT * FROM " + SchemaTableName(src));

            return default;
        }

        public IEnumerable<object>? Query(Type type, string statement, params object[] args) =>
            _conn.Query(type, string.Format(statement, args));

        public async Task<IEnumerable<dynamic>?> QueryAsync(Type type)
        {
            var src = ((TObject?)type.GetCustomAttributes(typeof(TObject), true).FirstOrDefault())?.Source;
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

        public object? Insert(object item)
        {
            var tab = item.GetDefinition();
            if (tab != null)
            {
                var stmt = new StringBuilder("INSERT " + tab.TableName + " (");
                var vals = new StringBuilder(") VALUES (");
                string comma = string.Empty;
                foreach (var col in item.GetAttributes())
                {
                    stmt.Append(comma).Append(LQ).Append(col.Key.ToLower()).Append(RQ);
                    vals.Append(comma).Append(GetSqlValue(item, col.Key));
                    comma = ",";
                }
                Exec(string.Concat(stmt, vals.Append(')')));
                return item;
            }
            return null;
        }

        public virtual object? Update(object item)
        {
            var tab = item.GetDefinition();
            if (tab != null)
            {
                var stmt = new StringBuilder("UPDATE " + tab.TableName + " SET ");
                string comma = string.Empty;
                foreach (var col in item.GetAttributes().Where(a => !a.Value.IsKey)) {
                    stmt.Append(comma).Append(LQ).Append(col.Key.ToLower()).Append(RQ).Append('=').Append(GetSqlValue(item, col.Key));
                    comma = ",";
                }
                var key = item.GetAttributes().FirstOrDefault(a => a.Value.IsKey);
                stmt.Append(" WHERE ").Append(LQ).Append(key.Key.ToLower()).Append(RQ).Append('=').Append(GetSqlValue(item, key.Key));

                Exec(stmt.ToString());
                return item;
            }
            return null;
        }

#pragma warning disable CS8603

        static string GetSqlValue(object? value)
        {
            if (value == null) return "NULL";
            return value switch
            {
                string => string.Concat("'", value.ToString(), "'"),
                bool => (bool)value ? "1" : "0",
                DateTime => string.Concat("'", ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fff"), "'"),
                TRefType => ((TRefType)value).Value?.ToString() ?? "NULL",
                float => ((float)value).ToString(CultureInfo.InvariantCulture),
                double => ((double)value).ToString(CultureInfo.InvariantCulture),
                decimal => ((decimal)value).ToString(CultureInfo.InvariantCulture),
                byte[] => string.Concat("0x", string.Concat(((byte[])value).Select(n => n.ToString("x2")))),
                _ => value.ToString()
            };
        }

        static string GetSqlValue(object obj, string propertyName) =>
            GetSqlValue(obj.GetType().GetProperty(propertyName)?.GetValue(obj));

#pragma warning restore CS8603

        #endregion IDatabase implementation

        #region Nested types

        class TRefTypeTypeConverter : SqlMapper.TypeHandler<TRefType>
        {
            public override void SetValue(IDbDataParameter parameter, TRefType value) =>
                parameter.Value = value.ToString();

            public override TRefType Parse(object value) => new TRefType(
                long.Parse(Regex.Match(value.ToString(), "\\d+").Value),
                Regex.Match(value.ToString(), @"(?<=\d+;).*?(?=$)").Value);
        }

        #endregion Nested types
    }
}
