//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TEquipment – Справочник оборудования.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using RmSolution.DataAnnotations;
    #endregion Using

    [TObject("Единицы измерений", "engunits", Ordinal = 100)]
    public class TEngUnit : TCatalogGroupRow
    {
        [TColumn("Условное обозначение", Length = 8, Nullable = true)]
        public string? Symbol { get; set; }
    }

    [TObject("Типы оборудования", "equiptypes", Ordinal = 101)]
    public class TEquipmentType : TCatalogRow
    {
    }

    [TObject("Оборудование", "equipments", Ordinal = 102)]
    public class TEquipment : TCatalogGroupTreeRow
    {
        [TColumn("Тип", Binding = "equiptypes")]
        public TRefType Type { get; set; }
        [TColumn("Модель", Length = 128, Nullable = true)]
        public string? Model { get; set; }
        [TColumn("Серийный номер", Length = 32, Nullable = true)]
        public string? Serial { get; set; }
    }
}
