//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TEquipment – Справочник оборудования.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using RmSolution.DataAnnotations;
    #endregion Using

    [Table("equiptypes")]
    public class TEquipmentType : TCatalogRow
    {
    }

    [Table("equipments")]
    public class TEquipment : TCatalogGroupTreeRow
    {
        [Column]
        public long Type { get; set; }
        [Column("model nvarchar(128) NULL")]
        public string? Model { get; set; }
        [Column("serial nvarchar(32) NULL")]
        public string? Serial { get; set; }
    }
}
