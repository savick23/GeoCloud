//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: Классы метаданных.
//--------------------------------------------------------------------------------------------------
#pragma warning disable CS8618
namespace RmSolution.DataAnnotations
{
    #region Using
    using System;
    #endregion Using

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public abstract class TEntity : Attribute
    {
        /// <summary> Уникальный 64-разрядный идентификатор в Системе.</summary>
        [TColumn("Идентификатор", IsKey = true)]
        public long Id { get; set; }
        /// <summary> Родитель, тип.</summary>
        [TColumn("Родитель")]
        public long Parent { get; set; }
        /// <summary> Родитель, тип.</summary>
        /// <remarks> TType </remarks>
        [TColumn("Тип")]
        public long Type { get; set; }
        /// <summary> Уникальный код объекта конфигурации.</summary>
        [TColumn("Код", Length = 32)]
        public string Code { get; set; }
        /// <summary> Наимменование объекта конфигурации.</summary>
        [TColumn("Наименование", Length = 64)]
        public string Name { get; set; }
        /// <summary> Описание объекта конфигурации.</summary>
        [TColumn("Описание", Length = 1024, Nullable = true)]
        public string? Description { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [TObject("Объекты конфигурации", "config.objects", Ordinal = 1, IsSystem = true)]
    public sealed class TObject : TEntity
    {
        [TColumn("Источник", Length = 64, Nullable = true)]
        public string? Source { get; set; }
        public bool IsView { get; }
        [TColumn("Порядок")]
        public int Ordinal { get; set; } = int.MaxValue;
        public Type CType { get; set; }
        /// <summary> Признак системного объекта.</summary>
        public bool IsSystem { get; set; }

        /// <summary> Полное имя таблицы в базе данных.</summary>
        public string TableName
        {
            get
            {
                var t = Source.Split(new char[] { '.' });
                return t.Length == 1 ? string.Concat('"', t[0], '"') : string.Concat(t[0], ".\"", t[1], '"');
            }
        }

        public TAttributeCollection Attributes { get; } = new TAttributeCollection();

        public TObject()
        {
        }

        public TObject(string name, string source, bool isView = false)
        {
            Name = name;
            Source = source;
            IsView = isView;
        }

        public override string ToString() =>
            $"{Source}";
    }

    public sealed class TObjectCollection : List<TObject>
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [TObject("Реквизиты объекта конфигурации", "config.attributes", Ordinal = 2, IsSystem = true)]
    public sealed class TColumn : TEntity
    {
        [TColumn("Длинна")]
        public int Length { get; set; }
        /// <summary> Признак первичного ключа.</summary>
        public bool IsKey { get; set; }
        /// <summary> Признак возможности пустых значений.</summary>
        public bool Nullable { get; set; }
        /// <summary> Встроенный тип C#.</summary>
        public Type CType { get; set; }

        /// <summary> Definition </summary>
        public string? Source { get; set; }
        /// <summary> Definition </summary>
        public string[]? PrimaryKey { get; set; }
        /// <summary> Definition </summary>
        public string[]? Indexes { get; set; }
        /// <summary> Ссылочная таблица.</summary>
        public string? Binding { get; set; }
        /// <summary> Наименование поля БД.</summary>
        public string Field => string.Concat('"', Code.ToLower(), '"');
        /// <summary> Значение поля по умолчанию.</summary>
        public object? DefaultValue { get; set; }
        /// <summary> Видимость поля по умолчанию в клиенте.</summary>
        [TColumn("Видимость по умолчанию")]
        public bool Visible { get; set; } = true;

        public TColumn()
        {
        }

        public TColumn(string name)
        {
            Name = name;
        }

        public override string ToString() =>
            $"{Code} {CType.Name}";
    }

    public sealed class TAttributeCollection : List<TColumn>, ICloneable
    {
        public bool TryGetAttribute(string name, out TColumn? attribute)
        {
            attribute = this.FirstOrDefault(ai => ai.Code.ToUpper() == name.ToUpper());
            return attribute != null;
        }

        public object Clone()
        {
            var res = new TAttributeCollection();
            res.AddRange(this);
            return res;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class PrimaryKeyAttribute : Attribute
    {
        public string[] Columns { get; }

        public PrimaryKeyAttribute(params string[] columns)
        {
            Columns = columns;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IndexAttribute : Attribute
    {
        public string[] Columns { get; }

        public IndexAttribute(params string[] columns)
        {
            Columns = columns;
        }
    }
}
#pragma warning restore CS8618
