//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: RmPageBase – Базовая страница для всех форм.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Web
{
    #region Using
    using Microsoft.AspNetCore.Components;
    using RmSolution.Data;
    using RmSolution.Web;
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
        public string NullValue { get; set; } = "(null)";

        #endregion Properties

        #region Data operations

        protected async Task<object?> NewItem(TObjectDto mdtype) =>
            await Client.NewItemAsync(mdtype);

        protected string GetValue(object item, string name) =>
          item.GetType().GetProperty(name, _propFlags)?.GetValue(item)?.ToString() ?? NullValue;

        protected string GetValueEdit(object item, string name) =>
          item.GetType().GetProperty(name, _propFlags)?.GetValue(item)?.ToString() ?? string.Empty;

        protected void OnValueChanged(object data, string name, object? value)
        {
            var prop = data.GetType().GetProperty(name, _propFlags);
            if (prop.PropertyType == typeof(TRefType))
            {
                prop.SetValue(data, new TRefType((long)value, value.ToString()));
            }
            else
                data.GetType().GetProperty(name, _propFlags)?.SetValue(data, value);
        }

        protected void Cancel(object dataRow, object? originValues)
        {
            if (originValues != null && dataRow.GetType() == originValues.GetType())
                dataRow.GetType().GetProperties().ToList().ForEach(p => p.SetValue(dataRow, p.GetValue(originValues)));
        }

        protected List<TItem> RefData(TAttributeDto ai) =>
            Client.GetReferenceData(ai.Type);

        #endregion Data operations
    }

    public enum ActionState
    {
        Select, Edit, New
    }

    public struct TItem
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public TItem(long id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
