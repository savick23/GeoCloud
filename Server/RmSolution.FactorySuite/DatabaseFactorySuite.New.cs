//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: DatabaseFactorySuite – IDatabaseFactory implementation
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System;
    using System.Data;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using RmSolution.DataAnnotations;
    using RmSolution.Runtime;
    #endregion Using

    public partial class DatabaseFactorySuite
    {
        public void UpdateDatabase(IMetadata metadata, Action<string> message)
        {
            try
            {
                Open();
                CreateEnvironment(this, metadata.Entities, message);
            }
            finally
            {
                Close();
            }
        }

        /// <summary> Создать системные таблицы на основании атрибутов классов.</summary>
        protected void CreateEnvironment(IDatabase db, TObjectCollection entities, Action<string> message)
        {
            var tables = db.Tables();
            entities.ForEach(oi =>
            {
                if (!tables.Any(t => t.Name == (oi.Source.Contains('.') ? oi.Source : string.Concat(db.DefaultScheme, '.', oi.Source))) && !oi.IsView)
                {
                    var tdefn = new TableDefinition(oi.Source);
                    foreach (var ai in oi.Attributes)
                    {
                        tdefn.Columns.Add(ai.Source ?? BuildColumnDefn(ai));
                        if (ai.PrimaryKey != null) tdefn.Constraints.Add(string.Concat("PRIMARY KEY ", string.Join(",", ai.PrimaryKey)));
                        if (ai.Indexes != null) tdefn.Constraints.Add(string.Concat("INDEX ", string.Join(",", ai.Indexes)));
                    }
                    CreateTable(db, tdefn, message);
                }
            });
        }

        /// <summary> Построение описания поля на основании метаданных свойства .NET.</summary>
        string BuildColumnDefn(TColumn ai)
        {
            var finded = _typemapping.TryGetValue(ai.CType, out string? type);
            type = finded ? string.Format(type, ai.Length) : "int";
            return string.Join(" ", ai.Code.ToLower(), type,
                ai.IsKey ? "PRIMARY KEY" : ai.CType.IsValueType && !ai.CType.AssemblyQualifiedName.Contains("System.Nullable") || !ai.Nullable ? "NOT NULL" : "NULL");
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
                StringBuilder stmt = new("CREATE TABLE " + tableName);
                string comma = " (\r\n";
                defn.Columns.ForEach(col =>
                {
                    stmt.Append(comma);
                    ColumnDefinitionAdapter(stmt,
                        Regex.Matches(col, @"JSON\([\w\.]*\)|NOT NULL|NULL|PRIMARY KEY|[\w_(,)]+").Cast<Match>().Select(m => m.Value).ToArray());

                    comma = ",\r\n";
                });
                defn.Constraints.Where(a => a.StartsWith("PRIMARY KEY")).ToList().ForEach(defn =>
                {
                    stmt.Append(",\r\nPRIMARY KEY ");
                    IndexDefinition(stmt, defn.Substring(12).Trim().Split(new char[] { ',' }));
                });
                stmt.AppendLine(");\r\n");
                defn.Constraints.Where(a => a.StartsWith("INDEX")).ToList().ForEach(defn =>
                {
                    IndexDefinitionAdapter(stmt, tableName, defn.Substring(6).Trim().Split(new char[] { ',' }));
                });
                db.Exec(stmt.ToString());
                message(string.Format(TEXT.DbTableCreated, tableName));
            }
            else message(string.Format(TEXT.DbTableExists, tableName));
        }

        protected virtual void ColumnDefinitionAdapter(StringBuilder stmt, string[] definition)
        {
            for (int i = 0; i < definition.Length; i++)
            {
                var defn = definition[i];
                if (i == 0)
                {
                    stmt.Append(LQ);
                    stmt.Append(defn);
                    stmt.Append(RQ);
                }
                else if (defn.StartsWith("JSON"))
                {
                }
                else
                {
                    stmt.Append(' ');
                    stmt.Append(defn.Replace("(0)", "(MAX)"));
                }
            }
        }

        protected virtual void IndexDefinitionAdapter(StringBuilder stmt, string tableName, string[] columns)
        {
            var indexName = string.Join("_", columns.Select(f => Regex.Match(f, @"^\w+").Value.ToUpper()));
            stmt.Append($"CREATE NONCLUSTERED INDEX idx_{Regex.Match(tableName, @"(?<=\[).*?(?=\])").Value}_{indexName} ON {tableName} ");
            IndexDefinition(stmt, columns);
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

        #region Database initialization

        /// <summary> Инициализировать БД данными из файлов инициализации dbinit.smx.</summary>
        protected void InitDatabase(IDatabase db, TObjectCollection entities, Action<string> logger)
        {
            // Загрузим пользовательские объекты конфигурации -->
            var init_db_filename = Path.Combine(CONFIG, INITFILE);
            if (File.Exists(init_db_filename))
            {
                var db_tables = db.Tables();
                using var db_init = new ArchiveReader(init_db_filename);
                var init_files = XDocument.Parse(db_init.ReadAllText("[Content_Types].xml"))?
                    .Root?.Elements().Where(e => e.Attribute("Type")?.Value == "rmgeo")
                    .First().Attribute("PartNames")?.Value;

                init_files?
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(name => new
                    {
                        path = Path.GetDirectoryName(name),
                        mask = name.Split(new char[] { '\\' }).Last() + ".xml"
                    })
                    .SelectMany(p => db_init.GetFiles(p.path, p.mask))
                    .ToList()
                    .ForEach(filename =>
                     {
                         var sect = XDocument.Parse(db_init.ReadAllText(filename))?.Root?.Elements().FirstOrDefault();
                         if (sect != null)
                             LoadInitConfig(db, entities, db_tables, sect);
                     });
            }
        }

        void LoadInitConfig(IDatabase db, TObjectCollection entities, List<DbTable> tables, XElement items)
        {
            foreach (var item in items.Elements())
            {
                if (item.Name == "item")
                    CreateObjectFrom(db, entities, tables, item);
                else
                    LoadInitConfig(db, entities, tables, item);
            }
        }

        /// <summary> Создаём структуру и физические объекты в БД при первом запуске и отсутствии БД.</summary>
        void CreateObjectFrom(IDatabase db, TObjectCollection entities, List<DbTable> tables, XElement item)
        {
            var src = item.Attribute(WellKnownAttributes.Source.ToLower())?.Value;
            if (src != null)
            {
                var entity = entities.FirstOrDefault(e => e.Source == src);
                if (!src.Contains('.')) src = string.Concat(DefaultScheme, ".", src);
                var table = tables.FirstOrDefault(t => t.Name == src);
                Dictionary<string, object?>? entity_defaults = null;
                if (entity != null)
                {
                    object entity_inst = Activator.CreateInstance(entity.CType);
                    entity_defaults = entity.CType.GetProperties()
                        .Where(p => table.Columns.Any(c => c.Name == p.Name.ToLower()))
                        .ToDictionary(k => k.Name.ToLower(), v => v.GetCustomAttribute<TColumn>()?.DefaultValue ?? v.GetValue(entity_inst));
                }
                if (src != "config.objects")
                {
                    var record = new TObject();
                    typeof(TObject).GetProperties().Where(p => item.Attribute(p.Name.ToLower()) != null).ToList()
                        .ForEach(p => p.SetValue(record, InitGetValue(p.PropertyType, item.Attribute(p.Name.ToLower())?.Value)));

                    db.InsertOrUpdate(record);
                }
                var stmt = new StringBuilder();
                foreach (var sect in item.Elements())
                {
                    if (sect.Name == "attributes")
                    {
                        foreach (var attr in sect.Elements())
                        {
                            var record = new TColumn()
                            {
                                Parent = long.Parse(sect.Parent.Attribute(WellKnownAttributes.Id).Value)
                            };
                            typeof(TColumn).GetProperties().Where(p => attr.Attribute(p.Name.ToLower()) != null).ToList()
                                .ForEach(p => p.SetValue(record, InitGetValue(p.PropertyType, attr.Attribute(p.Name.ToLower())?.Value)));

                            db.InsertOrUpdate(record);
                        }
                        var attrs = new TColumn();

                    }
                    else if (sect.Name == "data")
                    {
                        foreach (var row in GetRows(sect, new List<XElement>()))
                        {
                            stmt.Append("INSERT INTO " + SchemaTableName(src) + " (");
                            StringBuilder sqlvals = new();
                            string comma = string.Empty;
                            foreach (var col in table.Columns)
                            {
                                var key = col.Name.ToLower();
                                if (row.Attribute(col.Name)?.Value is string val)
                                {
                                    stmt.Append(comma).Append(LQ).Append(col.Name).Append(RQ);
                                    sqlvals.Append(comma).Append(InitGetValue(col.Type, val));
                                    comma = ",";
                                }
                                else if (entity_defaults != null && entity_defaults.TryGetValue(key, out var valdef) && valdef != null)
                                {
                                    stmt.Append(comma).Append(LQ).Append(col.Name).Append(RQ);
                                    sqlvals.Append(comma).Append(InitGetValue(col.Type, valdef is DateTime dt && dt.Year < 1970 ? new DateTime(1970, 1, 1) : valdef));
                                    comma = ",";
                                }
                            }
                            stmt.Append(") VALUES (").Append(sqlvals).Append(");\n");
                        }
                    }
                }
                db.Exec(stmt.ToString());
            }
        }

        List<XElement> GetRows(XElement xe, List<XElement> result)
        {
            foreach (var e in xe.Elements())
                if (e.Name.LocalName == "row")
                    result.Add(e);
                else
                    GetRows(e, result);

            return result;
        }

        static string InitGetValue(string type, object? value)
        {
            if (value == null)
                return "NULL"; 

            if (value.GetType().IsEnum)
                return ((int)value).ToString();

            var val = value.ToString() ?? string.Empty;
            switch (type.ToUpper())
            {
                case "SMALLINT":
                case "INT":
                case "BIGINT":
                case "REAL":
                case "FLOAT":
                case "DOUBLE":
                case "DECIMAL":
                case "NUMERIC":
                    return val;

                case "BIT":
                    return val.ToLower() == "true" || val == "1" ? "1" : "0";

                case "DATETIME":
                    return string.Concat("'", DateTime.Parse(val).ToString("yyyy-MM-ddTHH:mm:ss.fff"), "'");

                default:
                    return string.Concat("'", val, "'");
            }
        }

        static object? InitGetValue(Type type, string? value)
        {
            if (value == null) return null;
            if (type.IsEnum) return Enum.TryParse(type, value, true, out var eval) ? eval : 0;
            if (type == typeof(int)) return int.Parse(value);
            if (type == typeof(long)) return long.Parse(value);
            if (type == typeof(float)) return float.Parse(value);
            if (type == typeof(double)) return double.Parse(value);
            if (type == typeof(decimal)) return decimal.Parse(value);
            if (type == typeof(DateTime)) return DateTime.Parse(value);
            if (type == typeof(bool)) return value.ToString().ToLower() == "true" || value == "1";
            return value.ToString();
        }

        #endregion Database initialization
    }
}
