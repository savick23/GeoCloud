//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TAttribute –
//--------------------------------------------------------------------------------------------------
#pragma warning disable CS8618
namespace RmSolution.Runtime
{
    #region Using
    using System;
    using RmSolution.DataAnnotations;
    #endregion Using

    [Table("Реквизиты объекта конфигурации", "config.attributes", Ordinal = 2, IsSystem = true)]
    public class TAttribute : TEntity
    {
        /// <summary> Признак первичного ключа.</summary>
        public bool IsKey { get; set; }
        public Type Type { get; set; }

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

        public override string ToString() =>
            $"{Code} {Type.Name}";
    }

    public class TAttributeCollection : List<TAttribute>, ICloneable
    {
        public bool TryGetAttribute(string name, out TAttribute? attribute)
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
}
#pragma warning restore CS8618
