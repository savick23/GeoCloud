//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: Annotations –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAnnotations
{
    #region Using
    using System;
    using System.Xml.Linq;
    #endregion Using

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableAttribute : Attribute
    {
        public string Name { get; }
        public string Source { get; }
        public bool IsView { get; }

        public TableAttribute(string name, string source, bool isView = false)
        {
            Name = name;
            Source = source;
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
        public string Name { get; set; }
        public string? Definition { get; set; }
        public bool Nullable { get; set; }
        public int Length { get; set; }
        public string? Type { get; set; }
        public bool Visible { get; set; } = true;

        public ColumnAttribute(string name, string? definition = null)
        {
            Name = name;
            Definition = definition;
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
