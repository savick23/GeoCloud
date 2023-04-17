//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: ConsoleController – Веб-консоль (телнет).
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using RmSolution.Runtime;
    #endregion Using

    public class ConsoleController : SmartController
    {
        public ConsoleController(IRuntime runtime) : base(runtime)
        {
        }

        /// <summary> http://localhost:8087/api/console </summary>
        [HttpGet("[action]")]
        public async Task<ContentResult> Console() => await Task.Run(() =>
        {
            return new ContentResult()
            {
                ContentType = "text/html",
                Content = new ConsolePageBuilder().Build()
            };
        });
    }
}