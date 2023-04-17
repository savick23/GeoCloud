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
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    #endregion Using

    public class ConsoleController : SmartController
    {
        const string SESSION = "_sid";
        static Dictionary<string, Socket> _sessions = new();

        public ConsoleController(IRuntime runtime) : base(runtime)
        {
        }

        /// <summary> http://localhost:8087/api/console </summary>
        [HttpGet("[action]")]
        public async Task<ContentResult> Console() => await Task.Run(() =>
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SESSION)))
            {
                HttpContext.Session.SetString(SESSION, Guid.NewGuid().ToString());
            }
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
            var t = HttpContext.Session.GetString(SESSION);
            return new JsonResult(new string[] { form.Input, "> " });
        });

        public class XInput
        {
            public string Input { get; set; }
        }
    }
}