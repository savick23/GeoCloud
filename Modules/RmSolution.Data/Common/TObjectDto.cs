//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TObjectDto – Класс обмена метаданными.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System.Text.RegularExpressions;
    #endregion Using

    /// <summary> Класс обмена метаданными.</summary>
    public class TObjectDto
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }

        public TAttributeDto[] Attributes {get;set;}
    }

    public class TAttributeDto
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public bool Visible { get; set; }
    }
}