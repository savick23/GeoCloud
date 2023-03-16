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
        [Column("modified datetime NOT NULL")]
        public DateTime Modified { get; set; }
    }
}
