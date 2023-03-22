//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TUser – Пользователи Системы.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using RmSolution.DataAnnotations;
    #endregion Using

    /// <summary> Роли пользователей.</summary>
    [Table("config.roles")]
    public class TRole : TCatalogRow
    {
    }

    /// <summary> Пользователи.</summary>
    [Table("config.users")]
    public class TUser : TCatalogRow
    {
        /// <summary> ИД учётной записи администратора.</summary>
        internal static readonly long ADMINISTRATOR = 0x7000000000400;
    }
}
