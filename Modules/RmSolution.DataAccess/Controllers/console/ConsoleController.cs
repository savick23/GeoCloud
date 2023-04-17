//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: ConsoleController – Веб-консоль (телнет).
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using RmSolution.DataAnnotations;
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

        /// <summary> Ввод строки в консоли телнет.</summary>
        [HttpPost("console/[action]")]
        public async Task<IActionResult> Input(XInput form) => await Task.Run(() =>
        {
            return new JsonResult(new string[] { form.Input, "> " });
        });

        public class XInput
        {
            public string Input { get; set; }
        }
    }
}