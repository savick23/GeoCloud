//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: IMetadata – Метаданные.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    public interface IMetadata
    {
        TObjectCollection Entities { get; }

        /// <summary> Возвращает данные объекта конфигурации.</summary>
        IEnumerable<object>? GetData(string id);
    }
}