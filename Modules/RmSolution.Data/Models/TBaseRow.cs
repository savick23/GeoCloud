//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TCatalogRow – Базовый клас записей (DataRow) базы данных.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using RmSolution.DataAnnotations;
    using System.Text.Json.Serialization;
    #endregion Using

    public class TBaseRow: ICloneable
    {
        internal static readonly DateTime DATETIMEEMPTY = new(1970, 1, 1); // UnixTime minimum

        [TAttribute("Идентификатор", IsKey = true, Visible = false)]
        public long Id { get; set; }
        [TAttribute("Состояние", Visible = false)]
        public int State { get; set; }

        public object Clone() =>
            MemberwiseClone();
    }

    public class TCatalogRow : TBaseRow
    {
        [TAttribute("Код", Length = 8)]
        public string Code { get; set; }
        [TAttribute("Наименование", Length = 64)]
        public string Name { get; set; }
        [TAttribute("Описание", Length = 1024, Nullable = true)]
        public string? Descript { get; set; }
        [TAttribute("Создал", Visible = false)]
        public long Creator { get; set; } = TUser.ADMINISTRATOR;
        [TAttribute("Создано", Visible = false)]
        public DateTime Created { get; set; } = DateTime.Now;
        [TAttribute("Изменил", Visible = false)]
        public long? Modifier { get; set; }
        [TAttribute("Изменено", Visible = false)]
        public DateTime? Modified { get; set; }
    }

    public class TCatalogTreeRow : TCatalogRow
    {
        [TAttribute("Родитель")]
        public long Parent { get; set; }
    }

    public class TCatalogGroupRow : TCatalogRow
    {
        [TAttribute("Группа")]
        public long Group { get; set; }
    }

    public class TCatalogGroupTreeRow : TCatalogTreeRow
    {
        [TAttribute("Группа")]
        public long Group { get; set; }
    }
}
