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

        [TColumn("Идентификатор", IsKey = true, Visible = false)]
        public long Id { get; set; }
        [TColumn("Состояние", Visible = false)]
        public int State { get; set; }

        public object Clone() =>
            MemberwiseClone();
    }

    public class TCatalogRow : TBaseRow
    {
        [TColumn("Код", Length = 8)]
        public string Code { get; set; }
        [TColumn("Наименование", Length = 64)]
        public string Name { get; set; }
        [TColumn("Описание", Length = 1024, Nullable = true)]
        public string? Descript { get; set; }
        [TColumn("Создал", Visible = false)]
        public long Creator { get; set; } = TUser.ADMINISTRATOR;
        [TColumn("Создано", Visible = false)]
        public DateTime Created { get; set; } = DateTime.Now;
        [TColumn("Изменил", Visible = false)]
        public long? Modifier { get; set; }
        [TColumn("Изменено", Visible = false)]
        public DateTime? Modified { get; set; }
    }

    public class TCatalogTreeRow : TCatalogRow
    {
        [TColumn("Родитель")]
        public long Parent { get; set; }
    }

    public class TCatalogGroupRow : TCatalogRow
    {
        [TColumn("Группа")]
        public long Group { get; set; }
    }

    public class TCatalogGroupTreeRow : TCatalogTreeRow
    {
        [TColumn("Группа")]
        public long Group { get; set; }
    }
}
