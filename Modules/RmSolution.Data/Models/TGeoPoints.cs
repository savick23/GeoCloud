//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TGeoPoints – Точки измерения.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using RmSolution.DataAnnotations;
    #endregion Using

    [Table("Проекты измерений", "geoprojects", Ordinal = 110)]
    public class TGeoProjects : TCatalogRow
    {
        [Column("Местоположение", "location nvarchar(128) NOT NULL")]
        public string Location { get; set; }
    }

    [Table("Группы измерений", "geogroups", Ordinal = 112)]
    public class TGeoGroups : TCatalogRow
    {
        [Column("Проект", Type = "geoprojects")]
        public TRefType Project { get; set; }
    }

    [Table("Точки измерения", "geopoints", Ordinal = 114)]
    public class TGeoPoints : TCatalogRow
    {
        [Column("Группа", Type = "geogroups")]
        public TRefType Group { get; set; }
    }
}
