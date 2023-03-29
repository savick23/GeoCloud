//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TEquipment – Справочник оборудования.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using RmSolution.DataAnnotations;
    #endregion Using

    [TObject("Типы оборудования", "equiptypes", Ordinal = 102)]
    public class TEquipmentType : TCatalogRow
    {
    }

    [TObject("Оборудование", "equipments", Ordinal = 100)]
    public class TEquipment : TCatalogGroupTreeRow
    {
        [TAttribute("Тип", Binding = "equiptypes")]
        public TRefType Type { get; set; }
        [TAttribute("Модель", Length = 128, Nullable = true)]
        public string? Model { get; set; }
        [TAttribute("Серийный номер", Length = 32, Nullable = true)]
        public string? Serial { get; set; }
    }
}
