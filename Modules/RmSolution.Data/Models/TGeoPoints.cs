//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TGeoPoints – Точки измерения.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using RmSolution.DataAnnotations;
    #endregion Using

    [TObject("Проекты измерений", "geoprojects", Ordinal = 110)]
    public class TGeoProjects : TCatalogRow
    {
        [TColumn("Местоположение", Length = 128)]
        public string Location { get; set; }
    }

    [TObject("Группы измерений", "geogroups", Ordinal = 112)]
    public class TGeoGroups : TCatalogRow
    {
        [TColumn("Проект", Binding = "geoprojects")]
        public TRefType Project { get; set; }
    }

    [TObject("Точки измерения", "geopoints", Ordinal = 114)]
    public class TGeoPoints : TCatalogRow
    {
        [TColumn("Группа", Binding = "geogroups")]
        public TRefType Group { get; set; }
    }
}
