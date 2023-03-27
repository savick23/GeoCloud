//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TObject –
//--------------------------------------------------------------------------------------------------
#pragma warning disable CS8618
namespace RmSolution.Runtime
{
    #region Using
    using System;
    using RmSolution.DataAnnotations;
    #endregion Using

    /// <summary> Базовая сущность объекта конфигурации.</summary>
    public class TEntity
    {
        /// <summary> Уникальный 64-разрядный идентификатор в Системе.</summary>
        [Column("Идентификатор", "id bigint PRIMARY KEY", IsKey = true)]
        public long Id { get; set; }
        /// <summary> Родитель, тип.</summary>
        /// <remarks> 1 - конфигурация; 2 - системный; 3 - перечисление; 4 - справочник; </remarks>
        [Column("Код", "parent bigint NOT NULL")]
        public long Parent { get; set; }
        /// <summary> Код объекта конфигурации.</summary>
        [Column("Код", "code nvarchar(32) NOT NULL")]
        public string Code { get; set; }
        /// <summary> Наимменование объекта конфигурации.</summary>
        [Column("Наименование", "name nvarchar(64) NOT NULL")]
        public string Name { get; set; }
        /// <summary> Описание объекта конфигурации.</summary>
        [Column("Описание", "descript nvarchar(1024) NULL")]
        public string? Descript { get; set; }
    }

    [Table("Объекты конфигурации", "config.objects", Ordinal = 1, IsSystem = true)]
    public class TObject : TEntity
    {
        public string Source { get; set; }
        public bool IsView { get; }
        public int Ordinal { get; set; }
        public Type Type { get; set; }

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

        public override string ToString() =>
            $"{Source}";
    }

    public class TObjectCollection : List<TObject>
    {
    }
}
#pragma warning restore CS8618
