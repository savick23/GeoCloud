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
          item.GetType().GetProperty(name)?.GetValue(item)?.ToString() ?? NullValue;

        protected string GetValueEdit(object item, string name) =>
          item.GetType().GetProperty(name)?.GetValue(item)?.ToString() ?? string.Empty;

        protected void OnValueChanged(object data, string name, object value)
        {
            data.GetType().GetProperty(name)?.SetValue(data, value);
        }

        protected void Cancel(object dataRow, object? originValues)
        {
            if (originValues != null && dataRow.GetType() == originValues.GetType())
                dataRow.GetType().GetProperties().ToList().ForEach(p => p.SetValue(dataRow, p.GetValue(originValues)));
        }

        #endregion Data operations
    }

    public enum ActionState
    {
        Select, Edit, New
    }
}
