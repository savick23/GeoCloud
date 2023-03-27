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
    #endregion Using

    internal class SmartMetadata : IMetadata
    {
        #region Declarations

        IDatabase _db;

        #endregion Declarations

        #region Properties

        /// <summary> Минимальная необходимая версия базы данных.</summary>
        public static readonly Version DbVersionRequirements = Version.Parse("3.0.0.1");
        public string DatabaseName => _db.DatabaseName ?? "RMGEO01";
        public TObjectCollection Entities { get; } = new();
        /// <summary> Настройки, параметры конфигурации.</summary>
        public dynamic Settings;

        #endregion Properties

        #region Constuctors, Initialization

        public SmartMetadata(IDatabase connection)
        {
            _db = connection;
        }

        public void Open()
        {
            LoadMetadata();
            _db.Open();
            Settings = new TSettingsWrapper(_db.Query<TSettings>());
        }

        void LoadMetadata()
        {
            foreach (var mdtype in GetTypes<TableAttribute>())
            {
                var props = mdtype.GetProperties(BindingFlags.Instance | BindingFlags.Public).OrderBy(d => d.MetadataToken);
                var info = (TableAttribute?)mdtype.GetCustomAttribute(typeof(TableAttribute));
                var obj = new TObject()
                {
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
                        obj.Attributes.Add(new TAttribute()
                        {
                            Code = pi.Name,
                            Name = ai.Name,
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

        public object? GetData(string id)
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
                    if (ai.CType == typeof(TRefType))
                    {
                        ajoin = ((char)(ajoin[0] + 1)).ToString();
                        stmt_from.Append(" LEFT JOIN ").Append("equiptypes").Append(' ').Append(ajoin).Append(" ON ").Append(ajoin).Append('.').Append("id=").Append("a." + ai.Field);
                        return string.Concat(alias, ".", ai.Field, ",", ajoin, ".\"name\" \"", ai.Field[1..^1], "_view\"");
                    }
                    return string.Concat(alias, ".", ai.Field);
                })));
                var stmt = stmt_select.Append(stmt_from).ToString();
            }
            return null;
        }

        #endregion IMetadata implementation
    }
}
