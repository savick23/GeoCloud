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
    using System.Text.RegularExpressions;
    using RmSolution.Runtime;
    #endregion Using

    public abstract class DatabaseFactorySuite : IDatabase, IDatabaseFactory, IDisposable
    {
        #region Declarations

        protected DbConnection _conn;

        /// <summary> Исключаем одновременный доступ (запрос) к данным базы данных.</summary>
        protected object SyncRoot = new();

        #endregion Declarations

        #region Properties

        public string ApplicationName { get; set; } = "rmgeo";
        public virtual string DefaultScheme { get; }
        public virtual string Version { get; }
        public string DatabaseName { get; }
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
        /// <summary> Создать базу данных с кодовой страницой UTF8.</summary>
        public virtual void CreateDatabase() => throw new NotImplementedException();

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

        /// <summary> Создать системные таблицы на основании атрибутов классов.</summary>
        protected void CreateEnvironment(IDatabase db)
        {
            GetTypes<TableAttribute>().ForEach(t =>
            {
                var meta = (TableAttribute)t.GetCustomAttributes(typeof(TableAttribute), false)[0];
                if (!meta.IsView)
                {
                    var tdefn = new TableDefinition(meta.Name);
                    foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public).OrderBy(d => d.MetadataToken))
                        if (p.IsDefined(typeof(ColumnAttribute)))
                        {
                            tdefn.Columns.Add(((ColumnAttribute)p.GetCustomAttributes(typeof(ColumnAttribute)).First()).Name);

                            //if (!_typeNumeric.Contains(p.Name) &&
                            //    (p.PropertyType == typeof(int) || p.PropertyType == typeof(long) || p.PropertyType == typeof(decimal) || p.PropertyType == typeof(float) || p.PropertyType == typeof(double)))
                            //    _typeNumeric.Add(p.Name);
                        }
                }
            });
        }

        /// <summary> Возвращает типы с указанным аттрибутом.</summary>
        static List<Type> GetTypes<T>() where T : Attribute
        {
            var tkn = Assembly.GetEntryAssembly().GetName().GetPublicKeyToken();
            return Assembly.GetEntryAssembly()
                .GetReferencedAssemblies()
                .Where(a => a.GetPublicKeyToken()?.Where((n, i) => tkn.Length > 0 && tkn[i] == n).Count() == tkn.Length)
                .SelectMany(a => Assembly.Load(a).GetTypes())
                .Concat(Assembly.GetEntryAssembly().GetTypes())
                .Where(t => t.IsDefined(typeof(T), false)).ToList();
        }

        #endregion IDatabaseFactory implementation
    }
}
