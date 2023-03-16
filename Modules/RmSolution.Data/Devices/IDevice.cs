//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TagBase – Базовый тэг данных.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Data
{
    #region Using
    using System;
    #endregion Using

    public interface IDevice
    {
        /// <summary> Униальный 64-разрядный идентификатор устройства в Системе.</summary>
        long Id { get; set; }
        /// <summary> Код (шифр) устройства.</summary>
        string? Code { get; set; }
        /// <summary> Наименование устройства.</summary>
        string? Name { get; set; }
        /// <summary> Описание устройства (примечание, комментарий).</summary>
        string? Description { get; set; }
    }
}
