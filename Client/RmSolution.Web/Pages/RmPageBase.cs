//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: RmPageBase – Базовая страница для всех форм.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Web
{
    #region Using
    using Microsoft.AspNetCore.Components;
    using RmSolution.Data;
    using System.Collections.Concurrent;
    using System.Reflection;
    #endregion Using

    public abstract class RmPageBase : ComponentBase
    {
        #region Constants

        public const string CMD_NEW = "NEW";
        public const string CMD_COPY = "COPY";
        public const string CMD_EDIT = "EDIT";
        public const string CMD_DELETE = "DELETE";
        public const string CMD_APPLY = "APPLY";
        public const string CMD_CANCEL = "CANCEL";

        readonly static BindingFlags _propFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;

        #endregion Constants

        #region Properties

        [Inject]
        protected RmHttpClient Client { get; set; }

        /// <summary> Представление, как отображать значения NULL.</summary>
        public string NullValue { get; set; } = string.Empty;

        #endregion Properties

        #region Data operations

        static readonly ConcurrentDictionary<Type, Dictionary<string, MethodInfo>> _prop_get_cache = new();

        protected async Task<object?> NewItem(TObjectDto mdtype) =>
            await Client.NewItemAsync(mdtype);

        protected string GetValue(object item, string name)
        {
            var type = item.GetType();
            if (!_prop_get_cache.ContainsKey(type))
                _prop_get_cache.TryAdd(type, type.GetProperties(_propFlags).OrderBy(p => p.Name).Where(p => p.GetGetMethod() != null)
                    .ToDictionary(k => k.Name.ToLower(), v => v.GetGetMethod()));

            return _prop_get_cache[type][name.ToLower()].Invoke(item, null)?.ToString() ?? NullValue;
        }

        protected string GetValueEdit(object item, string name) =>
            _prop_get_cache[item.GetType()][name.ToLower()].Invoke(item, null)?.ToString() ?? string.Empty;

        protected void OnValueChanged(object data, string name, object? value)
        {
            data.GetType().GetProperty(name, _propFlags)?.SetValue(data, value);
        }

        protected void Cancel(object dataRow, object? originValues)
        {
            if (originValues != null && dataRow.GetType() == originValues.GetType())
                dataRow.GetType().GetProperties().ToList().ForEach(p => p.SetValue(dataRow, p.GetValue(originValues)));
        }

        protected List<TRefType>? RefData(TAttributeDto ai) =>
            Client.GetReferenceData(ai.Type);

        #endregion Data operations
    }

    public enum ActionState
    {
        Select, Edit, New
    }
}
