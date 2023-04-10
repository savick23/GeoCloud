//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TEquipment – Справочник оборудования.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using RmSolution.DataAnnotations;
    #endregion Using

    [TObject("Команды", "commands", Ordinal = 103)]
    public class TCommand : TCatalogTreeRow
    {
        [TColumn("Команда", Length = 256)]
        public string? Command { get; set; }
        [TColumn("Описание", Length = 1024, Nullable = true)]
        public string? Description { get; set; }
    }
}
