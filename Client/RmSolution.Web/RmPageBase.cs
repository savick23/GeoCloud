//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: RmPageBase – Базовая страница для всех форм.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Web
{
    #region Using
    using Microsoft.AspNetCore.Components;
    using RmSolution.Web;
    #endregion Using

    public abstract class RmPageBase : ComponentBase
    {
        #region Properties

        [Inject]
        protected RmHttpClient Client { get; set; }

        /// <summary> Представление, как отображать значения NULL.</summary>
        public string NullValue { get; set; } = "(null)";

        #endregion Properties

        protected string GetValue(object item, string name) =>
          item.GetType().GetProperty(name)?.GetValue(item)?.ToString() ?? NullValue;
    }
}
