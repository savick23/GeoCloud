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
        static string BuildColumnDefn(TAttribute ai)
        {
            var finded = _typemapping.TryGetValue(ai.Type, out string? type);
            return string.Join(" ", ai.Code.ToLower(),
                finded ? type : "int",
                finded ? string.Empty : ai.Type.IsValueType && !ai.Type.AssemblyQualifiedName.Contains("System.Nullable") ? "NOT NULL" : "NULL");
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
                    var stmt = new StringBuilder();
                    foreach (var sect in item.Elements())
                    {
                        if (sect.Name == "data")
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
                                        sqlvals.Append(comma).Append(InitGetValue(ai.Type, col.Value));
                                        comma = ",";
                                    }
                                }
                                foreach (var ai in attrs)
                                    if (ai.Type.IsValueType && !ai.Type.AssemblyQualifiedName.Contains("System.Nullable"))
                                    {
                                        stmt.Append(comma).Append(ai.Field);
                                        sqlvals.Append(comma).Append(InitGetValue(ai.Type, ai.DefaultValue));
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
