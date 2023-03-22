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

        [Column("id bigint PRIMARY KEY")]
        public long Id { get; set; }
        [Column]
        public int State { get; set; }
    }

    public class TCatalogRow : TBaseRow
    {
        [Column("code nvarchar(8) NOT NULL")]
        public string? Code { get; set; }
        [Column("name nvarchar(64) NOT NULL")]
        public string? Name { get; set; }
        [Column("descript nvarchar(1024) NULL")]
        public string? Descript { get; set; }
        [Column]
        public long Creator { get; set; } = TUser.ADMINISTRATOR;
        [Column]
        public DateTime Created { get; set; } = DateTime.Now;
        [Column]
        public long? Modifier { get; set; }
        [Column]
        public DateTime? Modified { get; set; }
    }

    public class TCatalogTreeRow : TCatalogRow
    {
        [Column]
        public long Parent { get; set; }
    }

    public class TCatalogGroupRow : TCatalogRow
    {
        [Column]
        public long Group { get; set; }
    }

    public class TCatalogGroupTreeRow : TCatalogTreeRow
    {
        [Column]
        public long Group { get; set; }
    }
}
