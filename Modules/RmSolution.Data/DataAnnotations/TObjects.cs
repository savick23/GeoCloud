//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TObjects – Классы объектов конфигурации (метаданных).
//--------------------------------------------------------------------------------------------------
#pragma warning disable CS8618
namespace RmSolution.DataAnnotations
{
    #region Using
    using System;
    using RmSolution.Data;
    #endregion Using

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [TObject("Объекты конфигурации", "config.objects", Ordinal = 1)]
    public sealed class TObject : TEntity
    {
        #region Properties

        public bool IsView { get; }
        public Type CType { get; set; }
        [TColumn("Автонумерация")]
        public TCodeAutoInc AutoInc { get; set; }
        /// <summary> Различные флаги объекта конфигурации.</summary>
        [TColumn("Флаги")]
        public TObjectFlags Flags { get; set; }
        /// <summary> [Flag] Признак системного объекта.</summary>
        public bool IsSystem => (Flags & TObjectFlags.System) > 0;

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

        #endregion Properties

        public TObject()
        {
        }

        public TObject(string name, string source, bool isView = false)
        {
            Name = name;
            Source = source;
            IsView = isView;
        }

        public TObjectDto ToDto() => new()
        {
            Id = Id,
            Code = Code,
            Name = Name,
            Source = Source,
            Type = CType.AssemblyQualifiedName ?? CType.Name,
            Attributes = Attributes.Select(a => new TAttributeDto()
            {
                Name = a.Name,
                Field = a.Source ?? a.Code,
                Type = a.Type,
                Visible = a.Visible,
                IsReference = a.IsReference
            }).ToArray()
        };

        public override string ToString() =>
            $"{Source}";
    }

    public sealed class TObjectCollection : List<TObject>
    {
        public TObject? this[long id] =>
            this.FirstOrDefault(oi => oi.Id == id);

        public TObject? this[string id] =>
            this.FirstOrDefault(oi => oi.Code == id || oi.Name == id || oi.Source == id);
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

    [Flags]
    public enum TObjectFlags
    {
        None = 0,
        System = 1
    }

    public enum TCodeAutoInc
    {
        None = 0,
        /// <summary> Во всем справочнике.</summary>
        /// <remarks> В процессе формирования нового кода для элемента справочника будет сформирован код, уникальный во всем справочнике.</remarks>
        Common = 1,
        /// <summary> В пределах подчинения (группы).</summary>
        /// <remarks> В процессе формирования нового кода для элемента справочника будет сформирован код, уникальный в пределах иерархии элемента (элементы, имеющие одного и того же родителя будут иметь различные коды, элементы, имеющие разных родителей могут иметь одинаковые коды).</remarks>
        Parent = 2,
        /// <summary> В пределах подчинения владельцу.</summary>
        /// <remarks> В процессе формирования нового кода для элемента справочника будет сформирован код, уникальный в пределах подчинения (элементы, имеющие одного и того же владельца будут иметь различные коды; элементы, имеющие различных владельцев могут иметь одинаковые коды).</remarks>
        Owner = 4
    }
}
#pragma warning restore CS8618
