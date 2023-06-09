﻿//--------------------------------------------------------------------------------------------------
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
        public virtual List<string> Schemata() => throw new NotImplementedException();
        public virtual List<DbTable> Tables() => throw new NotImplementedException();

        /// <summary> Создать базу данных с кодовой страницой UTF8.</summary>
        public virtual void CreateDatabase(IMetadata metadata, Action<string> message) => throw new NotImplementedException();

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
            var obj = item.GetDefinition();
            if (obj != null && item is TItemBase xitem)
            {
                if ((xitem.Id & TType.RecordMask) == TType.NewId)
                {
                    xitem.Id = Scalar<long>("SELECT max(id) from " + obj.TableName) + TType.TypeIterator;
                }
                Exec(BuildInsertCommand(obj, item));
                return item;
            }
            return null;
        }

        public virtual object? Update(object item)
        {
            var obj = item.GetDefinition();
            if (obj != null)
            {
                Exec(BuildUpdateCommand(obj, item));
                return item;
            }
            return null;
        }

        public object? InsertOrUpdate(object item)
        {
            var obj = item.GetDefinition();
            if (obj != null)
            {
                var pkey = item.GetAttributes().First(a => a.Value.IsKey);
                Exec(string.Concat("IF EXISTS(SELECT 1 FROM " + obj.TableName + " WHERE \"" + pkey.Key + "\"=" + GetSqlValue(item, pkey.Key) + ") ",
                    BuildUpdateCommand(obj, item), " ELSE ", BuildInsertCommand(obj, item)));

                return item;
            }
            return null;
        }

        string BuildInsertCommand(TObject obj, object item)
        {
            // DECLARE @newid bigint=(SELECT max(id)+0x1000000000000 from "equiptypes"); SELECT @newid;
            var stmt = new StringBuilder("INSERT " + obj.TableName + " (");
            var vals = new StringBuilder(") VALUES (");
            string comma = string.Empty;
            foreach (var col in item.GetAttributes())
            {
                stmt.Append(comma).Append(LQ).Append(col.Key.ToLower()).Append(RQ);
                vals.Append(comma).Append(GetSqlValue(item, col.Key));
                comma = ",";
            }
            return string.Concat(stmt, vals.Append(')'));
        }

        string BuildUpdateCommand(TObject obj, object item)
        {
            var stmt = new StringBuilder("UPDATE " + obj.TableName + " SET ");
            string comma = string.Empty;
            foreach (var col in item.GetAttributes().Where(a => !a.Value.IsKey))
            {
                stmt.Append(comma).Append(LQ).Append(col.Key.ToLower()).Append(RQ).Append('=').Append(GetSqlValue(item, col.Key));
                comma = ",";
            }
            var key = item.GetAttributes().FirstOrDefault(a => a.Value.IsKey);
            stmt.Append(" WHERE ").Append(LQ).Append(key.Key.ToLower()).Append(RQ).Append('=').Append(GetSqlValue(item, key.Key));

            return stmt.ToString();
        }

#pragma warning disable CS8603

        static string GetSqlValue(object? value)
        {
            if (value == null)
                return "NULL";

            if (value.GetType().IsEnum)
                return ((int)value).ToString();

            return value switch
            {
                string => string.Concat("'", value.ToString(), "'"),
                bool => (bool)value ? "1" : "0",
                DateTime => string.Concat("'", ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fff"), "'"),
                TRefType => ((TRefType)value).Id?.ToString() ?? "NULL",
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

            public override TRefType Parse(object value) =>
                value is long val
                ? new TRefType(val, null)
                : value == null
                ? TRefType.Empty
                : new TRefType(long.Parse(((string)value)[0..19]), ((string)value)[19..]);
        }

        #endregion Nested types
    }
}
