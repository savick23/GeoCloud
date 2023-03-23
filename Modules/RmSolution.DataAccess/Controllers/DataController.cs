//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: DataController – Доступ к данным.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using Microsoft.AspNetCore.Mvc;
    using RmSolution.Runtime;
    #endregion Using

    public class DataController: SmartController
    {
        public DataController(IRuntime runtime) : base (runtime)
        {
        }

        [HttpGet("[action]/{name}")]
        public async Task<IActionResult> Data(string name) => await UseDatabase(db =>
        {
            var mdtype = Runtime.Metadata.Entities.FirstOrDefault(oi => oi.Source == name)?.Type;
            if (mdtype != null)
            {
                var data = db.Query(mdtype);
                return new JsonResult(data);
            }
            throw new Exception("Тип " + name + " не найден!");
        });
    }
}
