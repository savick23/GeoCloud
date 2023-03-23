//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TCatalogRow – Базовый клас записей (DataRow) базы данных.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using RmSolution.DataAnnotations;
    using System.Text.Json.Serialization;
    #endregion Using

    public class TBaseRow
    {
        internal static readonly DateTime DATETIMEEMPTY = new(1970, 1, 1); // UnixTime minimum

        [Column("Идентификатор", "id bigint PRIMARY KEY", Visible = false)]
        public long Id { get; set; }
        [Column("Состояние", Visible = false)]
        public int State { get; set; }
    }

    public class TCatalogRow : TBaseRow
    {
        [Column("Код", "code nvarchar(8) NOT NULL")]
        public string? Code { get; set; }
        [Column("Наименование", "name nvarchar(64) NOT NULL")]
        public string? Name { get; set; }
        [Column("Описание", "descript nvarchar(1024) NULL")]
        public string? Descript { get; set; }
        [Column("Создал")]
        public long Creator { get; set; } = TUser.ADMINISTRATOR;
        [Column("Создано")]
        public DateTime Created { get; set; } = DateTime.Now;
        [Column("Изменил")]
        public long? Modifier { get; set; }
        [Column("Изменено")]
        public DateTime? Modified { get; set; }
    }

    public class TCatalogTreeRow : TCatalogRow
    {
        [Column("Родитель")]
        public long Parent { get; set; }
    }

    public class TCatalogGroupRow : TCatalogRow
    {
        [Column("Группа")]
        public long Group { get; set; }
    }

    public class TCatalogGroupTreeRow : TCatalogTreeRow
    {
        [Column("Группа")]
        public long Group { get; set; }
    }
}
