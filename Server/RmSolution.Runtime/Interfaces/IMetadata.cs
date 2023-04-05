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

        /// <summary> Возвращает сведения об объекте конфигурации.</summary>
        TObject? GetObject(long id);

        /// <summary> Возвращает сведения об объекте конфигурации.</summary>
        TObject? GetObject(string id);

        /// <summary> Возвращает данные объекта конфигурации.</summary>
        Task<IEnumerable<object>?> GetDataAsync(string id);

        /// <summary> Возвращает табличные данные объекта конфигурации.</summary>
        Task<DataTable?> GetDataTableAsync(string id);

        /// <summary> Обновляет данные объекта конфигурации.</summary>
        object? UpdateData(object? item);

        /// <summary> Возвращает новую запись для объекта конфигурации.</summary>
        object? NewItem(object? id);
    }
}