//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TUser – Пользователи Системы.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using RmSolution.DataAnnotations;
    #endregion Using

    /// <summary> Узлы баз данных в случае распределённой БД.</summary>
    [Table("Узлы БД", "config.nodes")]
    public class TNode : TCatalogRow
    {
        /// <summary> Индекс (номер) базы данных.</summary>
        public int Index { get; set; }
    }

    /// <summary> Площадки.</summary>
    [Table("Площадки", "config.sites")]
    public class TSite : TCatalogRow
    {
    }

    /// <summary> Роли пользователей.</summary>
    [Table("Роли пользователей", "config.roles")]
    public class TRole : TCatalogRow
    {
    }

    /// <summary> Пользователи.</summary>
    [Table("Пользователи", "config.users")]
    public class TUser : TCatalogRow
    {
        /// <summary> ИД учётной записи администратора.</summary>
        internal static readonly long ADMINISTRATOR = 0x7000000000400;
    }
}
