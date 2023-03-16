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
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.Extensions.Logging;
    using RmSolution.Runtime;
    #endregion Using

    public abstract class DatabaseFactorySuite : IDatabase, IDatabaseFactory, IDisposable
    {
        #region Declarations

        protected DbConnection? _conn;

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
        public virtual IEnumerable<T> Query<T>(string statement, params object[] args) => throw new NotImplementedException();
        public virtual void Exec(string statement, params object[] args) => throw new NotImplementedException();
        public virtual IEnumerable<string> Schemata() => throw new NotImplementedException();
        public virtual IEnumerable<string> Tables() => throw new NotImplementedException();

        /// <summary> Создать базу данных с кодовой страницой UTF8.</summary>
        public virtual void CreateDatabase(Action<string> message) => throw new NotImplementedException();

        public virtual void Close()
        {
            _conn?.Close();
        }

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }

        public virtual object Scalar(string statement, params object[] args)
        {
            var tbl = Query(statement, args);         
            return tbl?.Rows.Count > 0 ? tbl.Rows[0][0] : null;
        }

        public virtual T Scalar<T>(string statement, params object[] args) =>
            Scalar(statement, args) is T res ? res : default;

        #endregion IDatabase implementation

        #region IDatabaseFactory implementation

        /// <summary> Возвращает типы с указанным аттрибутом.</summary>
        static List<Type> GetTypes<T>() where T : Attribute
        {
            var entry = Assembly.GetEntryAssembly();
            if (entry != null)
            {
                var tkn = entry.GetName().GetPublicKeyToken();
                if (tkn != null)
                    return entry.GetReferencedAssemblies()
                        .Where(a => a.GetPublicKeyToken()?.Where((n, i) => tkn.Length > 0 && tkn[i] == n).Count() == tkn.Length)
                        .SelectMany(a => Assembly.Load(a).GetTypes())
                        .Concat(entry.GetTypes())
                        .Where(t => t.IsDefined(typeof(T), false)).ToList();
            }
            return new List<Type>();
        }

        /// <summary> Создать системные таблицы на основании атрибутов классов.</summary>
        protected void CreateEnvironment(IDatabase db, Action<string> message)
        {
            GetTypes<TableAttribute>().ForEach(t =>
            {
                var meta = (TableAttribute)t.GetCustomAttributes(typeof(TableAttribute), false)[0];
                if (!meta.IsView)
                {
                    var tdefn = new TableDefinition(meta.Name);
                    foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public).OrderBy(d => d.MetadataToken))
                        if (p.IsDefined(typeof(ColumnAttribute)))
                            tdefn.Columns.Add(((ColumnAttribute)p.GetCustomAttributes(typeof(ColumnAttribute)).First()).Name);

                    t.GetCustomAttributes(typeof(PrimaryKeyAttribute), false).ToList()
                        .ForEach(idx => tdefn.Constraints.Add(string.Concat("PRIMARY KEY ", string.Join(",", ((PrimaryKeyAttribute)idx).Columns))));

                    t.GetCustomAttributes(typeof(IndexAttribute), false).ToList()
                        .ForEach(idx => tdefn.Constraints.Add(string.Concat("INDEX ", string.Join(",", ((IndexAttribute)idx).Columns))));

                    CreateTable(db, tdefn, message);
                }
            });
        }

        /// <summary> Получает полное имя таблицы со схемой.</summary>
        string SchemaTableName(string tableName)
        {
            var names = tableName.Split(new char[] { '.' });
            return names.Length == 1
                ? string.Concat(DefaultScheme, ".", LQ, names[0], RQ)
                : string.Concat(names[0], ".", LQ, names[1], RQ);
        }

        /// <summary> Создать физическую таблицу в базе данных, связанные с объектом конфигурации.</summary>
        void CreateTable(IDatabase db, TableDefinition defn, Action<string> message)
        {
            if (defn.Name.Contains('.')) // указана схема
            {
                var schema = defn.Name.Split(new char[] { '.' })[0];
                if (!db.Schemata().Any(s => s == schema))
                {
                    db.Exec($"CREATE SCHEMA {LQ}{schema}{RQ};");
                    message(string.Format(TEXT.DbSchemaCreated, schema));
                }
            }
            var tableName = SchemaTableName(defn.Name);
            if (!db.Tables().Any(tname => tname.Equals(tableName.Replace(LQ, string.Empty))))
            {
                StringBuilder sqlcmd = new StringBuilder("CREATE TABLE " + tableName);
                string comma = " (\r\n";
                defn.Columns.ForEach(col =>
                {
                    sqlcmd.Append(comma);
                    ColumnDefinitionAdapter(sqlcmd,
                        Regex.Matches(col, @"JSON\([\w\.]*\)|NOT NULL|NULL|PRIMARY KEY|[\w_(,)]+").Cast<Match>().Select(m => m.Value).ToArray());

                    comma = ",\r\n";
                });
                defn.Constraints.Where(a => a.StartsWith("PRIMARY KEY")).ToList().ForEach(defn =>
                {
                    sqlcmd.Append(",\r\nPRIMARY KEY ");
                    IndexDefinition(sqlcmd, defn.Substring(12).Trim().Split(new char[] { ',' }));
                });
                sqlcmd.AppendLine(");\r\n");
                defn.Constraints.Where(a => a.StartsWith("INDEX")).ToList().ForEach(defn =>
                {
                    IndexDefinitionAdapter(sqlcmd, tableName, defn.Substring(6).Trim().Split(new char[] { ',' }));
                });
                db.Exec(sqlcmd.ToString());
                message(string.Format(TEXT.DbTableCreated, tableName));
            }
            else message(string.Format(TEXT.DbTableExists, tableName));
        }

        protected virtual void ColumnDefinitionAdapter(StringBuilder sqlcmd, string[] definition)
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
                else if (defn.StartsWith("JSON"))
                {
                }
                else
                {
                    sqlcmd.Append(' ');
                    sqlcmd.Append(defn.Replace("(0)", "(MAX)"));
                }
            }
        }

        protected virtual void IndexDefinitionAdapter(StringBuilder sqlcmd, string tableName, string[] columns)
        {
            var indexName = string.Join("_", columns.Select(f => Regex.Match(f, @"^\w+").Value.ToUpper()));
            sqlcmd.Append($"CREATE NONCLUSTERED INDEX idx_{Regex.Match(tableName, @"(?<=\[).*?(?=\])").Value}_{indexName} ON {tableName} ");
            IndexDefinition(sqlcmd, columns);
        }

        protected void IndexDefinition(StringBuilder sqlcmd, string[] columns)
        {
            string comma = "(";
            columns.ToList()
                .ForEach(fld =>
                {
                    var f = fld.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    sqlcmd.AppendFormat("{0}{1}{2}{3} {4}", comma, LQ, f[0], RQ, f.Length > 1 ? f[1] : string.Empty);
                    comma = ",";
                });

            sqlcmd.AppendLine(")");
        }

        #endregion IDatabaseFactory implementation
    }
}
