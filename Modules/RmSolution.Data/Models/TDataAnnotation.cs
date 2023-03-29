//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: DataAnnotation –
//--------------------------------------------------------------------------------------------------
#pragma warning disable CS8618
namespace RmSolution.DataAnnotations
{
    #region Using
    using System;
    using RmSolution.DataAnnotations;
    #endregion Using

    public abstract class TEntityAttribute : Attribute
    {
        /// <summary> Уникальный 64-разрядный идентификатор в Системе.</summary>
        [TAttribute("Идентификатор", IsKey = true)]
        public long Id { get; set; }
        /// <summary> Родитель, тип.</summary>
        /// <remarks> 1 - конфигурация; 2 - системный; 3 - перечисление; 4 - справочник; </remarks>
        [TAttribute("Код")]
        public long Parent { get; set; }
        /// <summary> Код объекта конфигурации.</summary>
        [TAttribute("Код", Length = 32)]
        public string Code { get; set; }
        /// <summary> Наимменование объекта конфигурации.</summary>
        [TAttribute("Наименование", Length = 64)]
        public string Name { get; set; }
        /// <summary> Описание объекта конфигурации.</summary>
        [TAttribute("Описание", Length = 1024)]
        public string? Descript { get; set; }
    }

    [TObject("Объекты конфигурации", "config.objects", Ordinal = 1, IsSystem = true)]
    public sealed class TObjectAttribute : TEntityAttribute
    {
        public string Source { get; set; }
        public bool IsView { get; }
        public int Ordinal { get; set; }
        public Type Type { get; set; }
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

        public TAttributeCollection2 Attributes { get; } = new TAttributeCollection2();

        public TObjectAttribute(string name, string source, bool isView = false)
        {
            Name = name;
            Source = source;
            IsView = isView;
        }

        public override string ToString() =>
            $"{Source}";
    }

    [TObject("Реквизиты объекта конфигурации", "config.attributes", Ordinal = 2, IsSystem = true)]
    public sealed class TAttributeAttribute : TEntityAttribute
    {
        /// <summary> Тип данных.</summary>
        [TAttribute("Тип")]
        public long Type { get; set; }
        [TAttribute("Длинна")]
        public int Length { get; set; }
        /// <summary> Признак первичного ключа.</summary>
        public bool IsKey { get; set; }
        /// <summary> Встроенный тип C#.</summary>
        public Type CType { get; set; }

        /// <summary> Definition </summary>
        public string? Source { get; set; }
        /// <summary> Definition </summary>
        public string[]? PrimaryKey { get; set; }
        /// <summary> Definition </summary>
        public string[]? Indexes { get; set; }
        /// <summary> Наименование поля БД.</summary>
        public string Field => string.Concat('"', Code.ToLower(), '"');
        /// <summary> Значение поля по умолчанию.</summary>
        public object? DefaultValue { get; set; }
        /// <summary> Видимость поля по умолчанию в клиенте.</summary>
        public bool Visible { get; set; }

        public TAttributeAttribute(string name)
        {
            Name = name;
        }

        public override string ToString() =>
            $"{Code} {CType.Name}";
    }

    public class TAttributeCollection2 : List<TAttributeAttribute>, ICloneable
    {
        public bool TryGetAttribute(string name, out TAttributeAttribute? attribute)
        {
            attribute = this.FirstOrDefault(ai => ai.Code.ToUpper() == name.ToUpper());
            return attribute != null;
        }

        public object Clone()
        {
            var res = new TAttributeCollection2();
            res.AddRange(this);
            return res;
        }
    }
}
#pragma warning restore CS8618
