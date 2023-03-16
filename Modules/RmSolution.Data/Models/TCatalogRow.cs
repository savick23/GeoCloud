//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TCatalogRow – Базовый клас записей (DataRow) справочников.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    public class TBaseRow
    {
        [Column("id bigint PRIMARY KEY")]
        public long Id { get; set; }
        [Column("state int NOT NULL")]
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
        [Column("creator bigint NOT NULL")]
        public long Creator { get; set; }
        [Column("created datetime NOT NULL")]
        public DateTime Created { get; set; }
        [Column("modifier bigint NULL")]
        public long Modifier { get; set; }
        [Column("modified datetime NULL")]
        public DateTime Modified { get; set; }
    }

    public class TCatalogTreeRow : TCatalogRow
    {
        [Column("parent bigint NOT NULL")]
        public long Parent { get; set; }
    }

    public class TCatalogGroupRow : TCatalogRow
    {
        [Column("group bigint NOT NULL")]
        public long Group { get; set; }
    }

    public class TCatalogGroupTreeRow : TCatalogTreeRow
    {
        [Column("group bigint NOT NULL")]
        public long Group { get; set; }
    }
}
