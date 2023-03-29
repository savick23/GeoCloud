//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: DatabaseFactorySuite – IDatabaseFactory implementation
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System;
    using System.Data;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using RmSolution.DataAnnotations;
    using RmSolution.Runtime;
    #endregion Using

    public partial class DatabaseFactorySuite
    {
        public void UpdateDatabase(TObjectCollection entities, Action<string> message)
        {
            try
            {
                Open();
                CreateEnvironment(this, entities, message);
            }
            finally
            {
                Close();
            }
        }

        /// <summary> Создать системные таблицы на основании атрибутов классов.</summary>
        protected void CreateEnvironment(IDatabase db, TObjectCollection entities, Action<string> message) =>
            entities.ForEach(oi =>
            {
                if (!oi.IsView)
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

        /// <summary> Построение описания поля на основании метаданных свойства .NET.</summary>
        string BuildColumnDefn(TAttributeAttribute ai)
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
        protected void InitDatabase(IDatabase db, TObjectCollection entities, Action<string> message)
        {
            // Загрузим пользовательские объекты конфигурации -->
            var init_db_filename = Path.Combine(CONFIG, INITFILE);
            if (File.Exists(init_db_filename))
            {
                using var init_db = new ArchiveReader(init_db_filename);
                var init_files = XDocument.Parse(init_db.ReadAllText("[Content_Types].xml"))?
                    .Root?.Elements().Where(e => e.Attribute("Type")?.Value == "rmgeo")
                    .First().Attribute("PartNames")?.Value;

                init_files?
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(name => new
                    {
                        path = Path.GetDirectoryName(name),
                        mask = name.Split(new char[] { '\\' }).Last() + ".xml"
                    })
                    .SelectMany(p => init_db.GetFiles(p.path, p.mask))
                    .ToList()
                    .ForEach(filename =>
                     {
                         var sect = XDocument.Parse(init_db.ReadAllText(filename))?.Root?.Elements().FirstOrDefault();
                         if (sect != null)
                             LoadInitConfig(db, entities, sect, sect.Name.LocalName);
                     });
            }
        }

        void LoadInitConfig(IDatabase db, TObjectCollection entities, XElement items, string section)
        {
            foreach (var item in items.Elements())
            {
                if (item.Name == "item")
                    CreateObjectFrom(db, entities, item, section);
                else
                    LoadInitConfig(db, entities, item, section);
            }
        }

        /// <summary> Создаём структуру и физические объекты в БД.</summary>
        void CreateObjectFrom(IDatabase db, TObjectCollection entities, XElement item, string section)
        {
            var src = item.Attribute(WellKnownAttributes.Source.ToLower())?.Value;
            if (src != null)
            {
                var oi = entities.FirstOrDefault(oi => oi.Source.Equals(src));
                if (oi != null)
                {
                    db.Exec($"INSERT INTO config.\"objects\" (\"id\",\"parent\",\"code\",\"name\",\"descript\") VALUES ({item.Attribute("id")?.Value},{TType.Catalog},{GetSqlValue(item.Attribute("code")?.Value)},{GetSqlValue(item.Attribute("name")?.Value)},{GetSqlValue(item.Attribute("descript")?.Value)})");
                    var stmt = new StringBuilder();
                    foreach (var sect in item.Elements())
                    {
                        if (sect.Name == "attributes")
                        {
                            foreach (var attr in sect.Elements())
                                db.Exec($"INSERT INTO config.\"attributes\" (\"id\",\"parent\",\"code\",\"name\",\"type\") VALUES ({attr.Attribute("id")?.Value},{sect.Parent?.Attribute("id")?.Value},{GetSqlValue(attr.Attribute("code")?.Value)},{GetSqlValue(attr.Attribute("name")?.Value)},{GetSqlValue(attr.Attribute("type")?.Value ?? "0")})");
                        }    
                        else if (sect.Name == "data")
                        {
                            foreach (var row in sect.Elements())
                            {
                                stmt.Append("INSERT INTO " + SchemaTableName(src) + " (");
                                StringBuilder sqlvals = new();
                                string comma = string.Empty;
                                var attrs = (TAttributeCollection)oi.Attributes.Clone();
                                foreach (var col in row.Attributes())
                                {
                                    if (oi.Attributes.TryGetAttribute(col.Name.LocalName, out var ai))
                                    {
                                        attrs.Remove(ai);
                                        stmt.Append(comma).Append(ai.Field);
                                        sqlvals.Append(comma).Append(InitGetValue(ai.CType, col.Value));
                                        comma = ",";
                                    }
                                }
                                foreach (var ai in attrs)
                                    if (ai.CType.IsValueType || !ai.CType.AssemblyQualifiedName.Contains("System.Nullable"))
                                    {
                                        stmt.Append(comma).Append(ai.Field);
                                        sqlvals.Append(comma).Append(InitGetValue(ai.CType, ai.DefaultValue));
                                        comma = ",";
                                    }

                                stmt.Append(") VALUES (").Append(sqlvals).Append(");\n");
                            }
                        }
                    }
                    db.Exec(stmt.ToString());
                }
            }
        }

        static string InitGetValue(Type type, object? value)
        {
            if (value == null)
                return "NULL";

            var val = value.ToString();
            if (type == typeof(short) || type == typeof(int) || type == typeof(long) || type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                return val;

            if (type == typeof(bool))
                return val.ToLower() == "true" || val.ToLower() == "1" ? "1" : "0";

            if (type == typeof(DateTime))
                return "'" + DateTime.Parse(val).ToString("yyyy-MM-ddTHH:mm:ss.fff") + "'";

            return "'" + val + "'";
        }

        #endregion Database initialization
    }
}
