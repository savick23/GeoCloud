//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TObjectExtensions –
//--------------------------------------------------------------------------------------------------
#pragma warning disable CS8619
namespace RmSolution.Runtime
{
    using System.Linq;
    using System.Reflection;
    using RmSolution.DataAnnotations;

    public static class TObjectExtensions
    {
        /// <summary> Возвращает описание объекта на основании TableAttribute.</summary>
        public static TableAttribute? GetDefinition(this object obj) =>
            obj.GetType().GetCustomAttribute<TableAttribute?>(false);

        /// <summary> Возвращает список свойств маркированных ColumnAttribute для указанного типа. Если таких колонок нет, то все свойства.</summary>
        public static Dictionary<string, ColumnAttribute> GetAttributes(this object obj) =>
            obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(c => c.GetCustomAttribute<ColumnAttribute>(true) != null)
                .OrderBy(c => c.MetadataToken)
                .ToDictionary(k => k.Name, v => v.GetCustomAttribute<ColumnAttribute>(true));
    }
}
#pragma warning restore CS8619
