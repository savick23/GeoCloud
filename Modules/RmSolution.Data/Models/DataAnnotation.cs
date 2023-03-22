//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: Annotations –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAnnotations
{
    #region Using
    using System;
    #endregion Using

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableAttribute : Attribute
    {
        public string Name { get; }
        public bool IsView { get; }

        public TableAttribute(string name, bool isView = false)
        {
            Name = name;
            IsView = isView;
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

    /// <summary> Для параметров модуля в том числе.</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        public string? Name { get; set; }
        public bool Nullable { get; set; }
        public int Length { get; set; }

        public ColumnAttribute(string? name = null)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TObjectAttribute : Attribute
    {
        public long Type { get; }

        public TObjectAttribute(long type)
        {
            Type = type;
        }
    }
}
