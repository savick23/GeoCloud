﻿//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TEquipment – Справочник оборудования.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using RmSolution.DataAnnotations;
    #endregion Using

    [Table("Типы оборудования", "equiptypes", Ordinal = 102)]
    public class TEquipmentType : TCatalogRow
    {
    }

    [Table("Оборудование", "equipments", Ordinal = 100)]
    public class TEquipment : TCatalogGroupTreeRow
    {
        [Column("Тип", Type = "equiptypes")]
        public TRefType Type { get; set; }
        [Column("Модель", "model nvarchar(128) NULL")]
        public string? Model { get; set; }
        [Column("Серийный номер", "serial nvarchar(32) NULL")]
        public string? Serial { get; set; }
    }
}
