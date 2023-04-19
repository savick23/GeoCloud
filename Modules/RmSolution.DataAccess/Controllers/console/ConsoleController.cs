//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: ConsoleController – Веб-консоль (телнет).
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using System.Text;
    using Microsoft.AspNetCore.Mvc;
    using RmSolution.Runtime;
    #endregion Using

    public class ConsoleController : SmartController
    {
        const string SESSION = "_sid";
        static Dictionary<string, ConsolePageBuilder> _telnet = new();

        static TelnetStream? _stream;

        public ConsoleController(IRuntime runtime) : base(runtime)
        {
        }

        /// <summary> http://localhost:8087/api/console </summary>
        [HttpGet("[action]")]
        public async Task<ContentResult> Console() => await Task.Run(() =>
        {
            string seckey;
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SESSION)))
            {
                seckey = Guid.NewGuid().ToString();
                HttpContext.Session.SetString(SESSION, seckey);
                _telnet.Add(seckey, new ConsolePageBuilder());
            }
            else seckey = HttpContext.Session.GetString(SESSION);

            _stream = new TelnetStream(_telnet[seckey]);
            var bytes = Encoding.UTF8.GetBytes("LETUNOVSKY");
            _stream.Write(bytes, 0, bytes.Length);

            return new ContentResult()
            {
                ContentType = "text/html",
                Content = _telnet[seckey].Build()
            };
        });

        /// <summary> http://localhost:8087/api/console/read </summary>
        [HttpGet("console/[action]")]
        public async Task<ActionResult> Read() => await Task.Run(() =>
        {
            //    var file = new FileStream(@"d:\test.txt", FileMode.Open);
            //      return File(file, "application/octet-stream", false);
            try
            {
                return File(_stream, "application/octet-stream", false);
            }
            catch (Exception ex)
            {
                throw;
            }
        });

        /// <summary> Ввод строки в консоли телнет.</summary>
        [HttpPost("console/[action]")]
        public async Task<IActionResult> Input(XInput form) => await Task.Run(() =>
        {
            if (_telnet.TryGetValue(HttpContext.Session.GetString(SESSION), out var console))
            {
                var resp = console.ReadLines(Encoding.UTF8.GetBytes(form.Input + "\n"));
                return new JsonResult(resp.Length > 0 && resp[0].Equals(form.Input) ? resp.Skip(1).ToArray() : resp);
            }
            return new JsonResult(Array.Empty<string>());
        });

        public class XInput
        {
            public string Input { get; set; }
        }
    }
}