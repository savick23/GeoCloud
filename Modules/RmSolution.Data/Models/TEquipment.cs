//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TEquipment – Справочник оборудования.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    [Table("equipments")]
    public class TEquipment : TCatalogRow
    {
        [Column("model nvarchar(128) NULL")]
        public string? Model { get; set; }
        [Column("serial nvarchar(32) NULL")]
        public string? Serial { get; set; }
    }
}
