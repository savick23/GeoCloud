//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TEquipment – Справочник оборудования.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using RmSolution.DataAnnotations;
    #endregion Using

    [Table("Типы оборудования", "equiptypes")]
    public class TEquipmentType : TCatalogRow
    {
    }

    [Table("Оборудование", "equipments")]
    public class TEquipment : TCatalogGroupTreeRow
    {
        [Column(Type = "equiptypes")]
        public long Type { get; set; }
        [Column("model nvarchar(128) NULL")]
        public string? Model { get; set; }
        [Column("serial nvarchar(32) NULL")]
        public string? Serial { get; set; }
    }
}
