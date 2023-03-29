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
    [TObject("Узлы БД", "config.nodes", Ordinal = 5)]
    public class TNode : TCatalogRow
    {
        /// <summary> Индекс (номер) базы данных.</summary>
        public int Index { get; set; }
    }

    /// <summary> Площадки.</summary>
    [TObject("Площадки", "config.sites", Ordinal = 10)]
    public class TSite : TCatalogRow
    {
    }

    /// <summary> Роли пользователей.</summary>
    [TObject("Роли пользователей", "config.roles", Ordinal = 15)]
    public class TRole : TCatalogRow
    {
    }

    /// <summary> Пользователи.</summary>
    [TObject("Пользователи", "config.users", Ordinal = 20)]
    public class TUser : TCatalogRow
    {
        /// <summary> ИД учётной записи администратора.</summary>
        internal static readonly long ADMINISTRATOR = 0x7000000000400;
    }
}
