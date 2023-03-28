//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: SmartMetadata – Метаданные.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Server
{
    #region Using
    using System.Reflection;
    using RmSolution.Runtime;
    using RmSolution.DataAnnotations;
    using System;
    using RmSolution.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Dynamic;
    using System.Text;
    using Microsoft.Extensions.Logging;
    #endregion Using

    internal class SmartMetadata : IMetadata
    {
        #region Declarations

        ILogger _logger;
        IDatabase _db;

        #endregion Declarations

        #region Properties

        /// <summary> Минимальная необходимая версия базы данных.</summary>
        public static readonly Version DbVersionRequirements = Version.Parse("3.0.0.2");
        public string DatabaseName => _db.DatabaseName ?? "RMGEO01";
        public TObjectCollection Entities { get; } = new();
        /// <summary> Настройки, параметры конфигурации.</summary>
        public dynamic Settings;

        #endregion Properties

        #region Constuctors, Initialization

        public SmartMetadata(ILogger logger, IDatabase connection)
        {
            _logger = logger;
            _db = connection;
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
                    if (!isnewdb) ((IDatabaseFactory)_db).UpdateDatabase(Entities, (msg) => _logger.LogInformation(msg));
                    Settings = new TSettingsWrapper(_db.Query<TSettings>());
                    break;
                }
                catch (TDbNotFoundException)
                {
                    if (Entities.Count == 0) LoadMetadata(null);
                    if (attempt-- > 0)
                    {
                        _logger.LogWarning(string.Format(TEXT.CreateDatabaseTitle, DatabaseName));
                        ((IDatabaseFactory)_db).CreateDatabase(Entities, (msg) => _logger.LogInformation(msg));
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
            var tobjs = db?.Query<TObject>();
            var tattrs = db?.Query<TAttribute>();
            foreach (var mdtype in GetTypes<TableAttribute>())
            {
                var props = mdtype.GetProperties(BindingFlags.Instance | BindingFlags.Public).OrderBy(d => d.MetadataToken);
                var info = (TableAttribute?)mdtype.GetCustomAttribute(typeof(TableAttribute));
                var dboi = tobjs?.FirstOrDefault(o => o.Code == info?.Source?.ToUpper());
                var attrs = tattrs?.Where(a => a.Parent == dboi?.Id).ToList();
                var obj = new TObject()
                {
                    Id = dboi?.Id ?? 0,
                    Parent = info.IsSystem ? TType.System : TType.Catalog,
                    Name = info?.Name ?? throw new Exception("Не указано наименование объекта конфигурации."),
                    Source = info?.Source ?? throw new Exception("Не указан источник метаданных (таблица)."),
                    Ordinal = info?.Ordinal ?? int.MaxValue,
                    Type = mdtype
                };
                Entities.Add(obj);
                foreach (var pi in props)
                    if (pi.IsDefined(typeof(ColumnAttribute)))
                    {
                        var ai = (ColumnAttribute?)pi.GetCustomAttributes(typeof(ColumnAttribute)).First();
                        var dbai = attrs?.FirstOrDefault(a => a.Name == ai.Name);
                        obj.Attributes.Add(new TAttribute()
                        {
                            Id = dbai?.Id ?? 0,
                            Code = pi.Name,
                            Name = ai.Name,
                            Type = dbai?.Type ?? 0,
                            CType = pi.PropertyType,
                            Source = ai.Definition,
                            IsKey = ai.IsKey,
                            Visible  = ai.Visible,
                            PrimaryKey = ((PrimaryKeyAttribute?)pi.GetCustomAttributes(typeof(PrimaryKeyAttribute)).FirstOrDefault())?.Columns,
                            Indexes = ((IndexAttribute?)pi.GetCustomAttributes(typeof(IndexAttribute)).FirstOrDefault())?.Columns,
                            DefaultValue = pi.PropertyType == typeof(DateTime) ? TBaseRow.DATETIMEEMPTY
                                : ai.Default == null ? pi.GetValue(Activator.CreateInstance(mdtype)) : ai.Default
                        });
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

        public IEnumerable<object>? GetData(string id)
        {
            var mdtype = Entities.FirstOrDefault(e => e.Code == id || e.Name == id || e.Source == id);
            if (mdtype != null)
            {
                string alias = "a";
                string ajoin = "a";
                var stmt_select = new StringBuilder("SELECT ");
                var stmt_from = new StringBuilder(" FROM ").Append(mdtype.TableName).Append(' ').Append(alias);
                stmt_select.Append(string.Join(",", mdtype.Attributes.Select(ai =>
                {
                    if (ai.CType == typeof(TRefType) && ai.Type > 0 && Entities.FirstOrDefault(e => e.Id == ai.Type) is TObject refobj)
                    {
                        ajoin = ((char)(ajoin[0] + 1)).ToString();
                        stmt_from.Append(" LEFT JOIN ").Append(refobj.TableName).Append(' ').Append(ajoin).Append(" ON ").Append(ajoin).Append('.').Append("id=").Append("a." + ai.Field);
                        return string.Concat("cast(", alias, ".", ai.Field, " as nvarchar(32))+';'+", ajoin, ".\"name\" \"", ai.Field[1..^1], '"');
                    }
                    return string.Concat(alias, ".", ai.Field);
                }
                )));
                return _db.Query(mdtype.Type, stmt_select.Append(stmt_from).ToString());
            }
            return null;
        }

        public object? UpdateData(object? item)
        {
            if (item != null) _db.Update(item);
            return item;
        }

        #endregion IMetadata implementation
    }
}
