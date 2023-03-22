//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TObject –
//--------------------------------------------------------------------------------------------------
#pragma warning disable CS8618
namespace RmSolution.Runtime
{
    #region Using
    using System;
    #endregion Using

    public class TEntity
    {
        /// <summary> Уникальный 64-разрядный идентификатор в Системе.</summary>
        public long Id { get; set; }
    }

    public class TAttribute : TEntity
    {
        public string Code { get; set; }
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

    public class TObject : TEntity
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public bool IsView { get; }
        public Type Type { get; set; }

        public TAttributeCollection Attributes { get; } = new TAttributeCollection();

        public override string ToString() =>
            $"{Source}";
    }

    public class TObjectCollection : List<TObject>
    {
    }
}
#pragma warning restore CS8618
