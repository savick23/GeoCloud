﻿//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: SmartMetadata – Метаданные.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    #region Using
    using System;
    using System.Reflection;
    using System.Data;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using RmSolution.DataAnnotations;
    using RmSolution.Data;
    #endregion Using

    internal class SmartMetadata : IMetadata
    {
        #region Declarations

        DatabaseConnectionHandler _dbconnection;
        ILogger _logger;
        IDatabase _db;

        #endregion Declarations

        #region Properties

        /// <summary> Минимальная необходимая версия базы данных.</summary>
        public static readonly Version DbVersionRequirements = Version.Parse("3.0.0.3");
        public string DatabaseName => _db.DatabaseName ?? "RMGEO01";
        public TObjectCollection Entities { get; } = new();
        /// <summary> Настройки, параметры конфигурации.</summary>
        public dynamic Settings;

        #endregion Properties

        #region Constuctors, Initialization

        public SmartMetadata(ILogger logger, DatabaseConnectionHandler dbconnection)
        {
            _dbconnection = dbconnection;
            _logger = logger;
            _db = _dbconnection();
        }

        public void Open()
        {
            int attempt = 1;
            bool isnewdb = false;
            while (true)
                try
                {
                    _db.Open();
                    LoadMetadata(_db);
                    if (!isnewdb) ((IDatabaseFactory)_db).UpdateDatabase(this, (msg) => _logger.LogInformation(msg));
                    Settings = new TSettingsWrapper(_db.Query<TSettings>());
                    break;
                }
                catch (TDbNotFoundException)
                {
                    if (Entities.Count == 0) LoadMetadata(null);
                    if (attempt-- > 0)
                    {
                        _logger.LogWarning(string.Format(TEXT.CreateDatabaseTitle, DatabaseName));
                        ((IDatabaseFactory)_db).CreateDatabase(this, (msg) => _logger.LogInformation(msg));
                        Entities.Clear();
                        isnewdb = true;
                        _logger.LogInformation(string.Format(TEXT.CreateDatabaseSuccessfully, DatabaseName));
                        continue;
                    }
                    _logger.LogError(string.Format(TEXT.CreateDatabaseFailed, DatabaseName));
                    throw;
                }
                catch (Exception ex)
                {
                    throw new Exception("Подключение к база данных \"" + DatabaseName + "\" не установлено! " + ex.Message);
                }
        }

        void LoadMetadata(IDatabase? db)
        {
            var db_objects = db?.Query<TObject>();
            var db_columns = db?.Query<TColumn>();
            foreach (var mdtype in GetTypes<TObject>())
            {
                var model = mdtype.GetCustomAttribute<TObject>();
                var obj = db_objects?.FirstOrDefault(o => o.Source == model?.Source) ?? model;
                if (obj != null)
                {
                    obj.CType = mdtype;
                    var attrs = db_columns?.Where(a => a.Parent == obj?.Id).ToList();
                    foreach (var pi in mdtype.GetProperties(BindingFlags.Instance | BindingFlags.Public).OrderBy(d => d.MetadataToken)
                        .Where(d => d.IsDefined(typeof(TColumn))))
                    {
                        var ai = attrs?.FirstOrDefault(a => a.Source?.Equals(pi.Name, StringComparison.OrdinalIgnoreCase) ?? false)
                            ?? (TColumn?)pi.GetCustomAttributes(typeof(TColumn)).First();

                        ai.Code ??= pi.Name;
                        ai.CType = pi.PropertyType;
                        ai.PrimaryKey = ((PrimaryKeyAttribute?)pi.GetCustomAttributes(typeof(PrimaryKeyAttribute)).FirstOrDefault())?.Columns;
                        ai.Indexes = ((IndexAttribute?)pi.GetCustomAttributes(typeof(IndexAttribute)).FirstOrDefault())?.Columns;
                        ai.Nullable |= ai.CType.AssemblyQualifiedName.Contains("System.Nullable");
                        ai.DefaultValue = pi.PropertyType == typeof(DateTime) ? TItemBase.DATETIMEEMPTY
                                : ai.DefaultValue == null ? pi.GetValue(Activator.CreateInstance(mdtype)) : ai.DefaultValue;

                        obj.Attributes.Add(ai);
                    }
                    Entities.Add(obj);
                }
            }
        }

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

        #endregion Constuctors, Initialization

        #region IMetadata implementation

        public TObject? GetObject(long id)
        {
            id &= TType.TypeMask;
            return Entities.FirstOrDefault(oi => oi.Id == id);
        }

        public TObject? GetObject(string id) =>
            Entities.FirstOrDefault(oi => oi.Code == id || oi.Name == id || oi.Source == id);

        public IEnumerable<T>? GetData<T>() => UseDatabase(db => db.Query<T>());

        public async Task<IEnumerable<object>?> GetDataAsync(string id) => await UseDatabaseAsync(db =>
        {
            var obj = GetObject(id);
            if (obj != null)
            {
                string alias = "a";
                string ajoin = "a";
                var stmt_select = new StringBuilder("SELECT ");
                var stmt_from = new StringBuilder(" FROM ").Append(obj.TableName).Append(' ').Append(alias);
                stmt_select.Append(string.Join(",", obj.Attributes.Select(ai =>
                {
                    if (ai.CType == typeof(TRefType) && ai.Type > 0 && Entities.FirstOrDefault(e => e.Id == ai.Type) is TObject refobj)
                    {
                        ajoin = ((char)(ajoin[0] + 1)).ToString();
                        stmt_from.Append(" LEFT JOIN ").Append(refobj.TableName).Append(' ').Append(ajoin).Append(" ON ").Append(ajoin).Append('.').Append("id=").Append("a." + ai.Field);
                        return string.Concat("cast(", alias, '.', ai.Field, " as char(19))+", ajoin, '.', obj.Attributes.ViewField, ' ', ai.Field);
                    }
                    return string.Concat(alias, '.', ai.Field);
                }
                )));
                return db.Query(obj.CType, stmt_select.Append(stmt_from).ToString());
            }
            return null;
        });

        public async Task<DataTable?> GetDataTableAsync(string id) => await UseDatabaseAsync(db =>
        {
            var obj = GetObject(id);
            if (obj != null)
            {
                string alias = "a";
                string ajoin = "a";
                var stmt_select = new StringBuilder("SELECT ");
                var stmt_from = new StringBuilder(" FROM ").Append(obj.TableName).Append(' ').Append(alias);
                stmt_select.Append(string.Join(",", obj.Attributes.Select(ai =>
                {
                    if (ai.CType == typeof(TRefType) && ai.Type > 0 && Entities.FirstOrDefault(e => e.Id == ai.Type) is TObject refobj)
                    {
                        ajoin = ((char)(ajoin[0] + 1)).ToString();
                        stmt_from.Append(" LEFT JOIN ").Append(refobj.TableName).Append(' ').Append(ajoin).Append(" ON ").Append(ajoin).Append('.').Append("id=").Append("a." + ai.Field);
                        return string.Concat(alias, '.', ai.Field, ',', ajoin, '.', obj.Attributes.ViewField, ' ', ai.DisplayField);
                    }
                    return string.Concat(alias, '.', ai.Field);
                }
                )));
                return db.Query(stmt_select.Append(stmt_from).ToString());
            }
            return null;
        });

        public async Task<DataTable?> GetReferenceData(long id) => await UseDatabaseAsync(db =>
        {
            var obj = GetObject(id);
            if (obj != null)
            {
                return db.Query(new StringBuilder("SELECT ")
                    .Append(obj.Attributes.IdField.Field).Append(',').Append(obj.Attributes.ViewField)
                    .Append(" FROM ").Append(obj.TableName).ToString());
            }
            return null;
        });

        public object? UpdateData(object? item)
        {
            if (item != null) _db.Update(item);
            return item;
        }

        public object? NewItem(object? id)
        {
            TObject? obj;
            if (id is long objid && (obj = Entities[objid]) != null)
            {
                var newitem = (TItemBase)Activator.CreateInstance(obj.CType);
                newitem.Id = obj.Id + TType.NewId;
                if (obj.AutoInc == TCodeAutoInc.Common && obj.Attributes.CodeField is TColumn fcode)
                {
                    var code = _dbconnection().Open().Scalar<string>("SELECT max(code) FROM " + obj.TableName);
                    if (code == null)
                        code = "1".PadLeft(fcode.Length, '0');
                    else if (long.TryParse(code, out var num))
                        code = (++num).ToString().PadLeft(code.Length, '0');

                    obj.CType.GetProperty(fcode.Source, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)?.SetValue(newitem, code);
                }
                return newitem;
            }
            throw new Exception("Не найден объект конфигурации " + (id ?? "(null)"));
        }

        #endregion IMetadata implementation

        #region Database connection

        /// <summary> Используется новое подключение к БД, после выполнения которое закрывается.</summary>
        T UseDatabase<T>(Func<IDatabase, T> operation)
        {
            var db = _dbconnection();
            try
            {
                db.Open();
                return operation.Invoke(db);
            }
            finally
            {
                db.Close();
            }
        }

        /// <summary> Используется новое подключение к БД, после выполнения которое закрывается.</summary>
        async Task<T> UseDatabaseAsync<T>(Func<IDatabase, T> operation) => await Task.Run(() =>
        {
            var db = _dbconnection();
            try
            {
                db.Open();
                return operation.Invoke(db);
            }
            finally
            {
                db.Close();
            }
        });

        #endregion Database connection
    }
}
