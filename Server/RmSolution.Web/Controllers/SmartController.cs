//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: DataController – Доступ к данным.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Web
{
    #region Using
    using Microsoft.AspNetCore.Mvc;
    using RmSolution.Data;
    using RmSolution.Runtime;
    using System.Text;
    #endregion Using

    [ApiController]
    [Route(HttpApiService.ROUTEBASE)]
    public class SmartController : ControllerBase
    {
        protected readonly IRuntime Runtime;

        public SmartController(IRuntime runtime)
        {
            Runtime = runtime;
        }

        #region Database methods...

        /// <summary> Подключение и выполнение в среде БД.</summary>
        protected async Task<T> UseDatabase<T>(Func<IDatabase, T> handler)
        {
            var db = Runtime.CreateDbConnection();
            try
            {
                db.Open();
                return await Task.Run(() => handler.Invoke(db));
            }
            finally
            {
                db.Close();
            }
        }
        #endregion Database methods...
    }
}
