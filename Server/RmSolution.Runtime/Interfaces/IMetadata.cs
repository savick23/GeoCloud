//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: IMetadata – Метаданные.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    using System.Data;
    using RmSolution.DataAnnotations;

    public interface IMetadata
    {
        TObjectCollection Entities { get; }

        /// <summary> Возвращает данные объекта конфигурации.</summary>
        IEnumerable<object>? GetData(string id);

        /// <summary> Возвращает табличные данные объекта конфигурации.</summary>
        DataTable? GetDataTable(string id);

        /// <summary> Обновляет данные объекта конфигурации.</summary>
        object? UpdateData(object? item);
    }
}