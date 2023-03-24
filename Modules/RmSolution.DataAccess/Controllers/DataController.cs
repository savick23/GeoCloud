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
    using System.Xml.Linq;
    #endregion Using

    public class DataController : SmartController
    {
        public DataController(IRuntime runtime) : base(runtime)
        {
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Objects() => await UseDatabase(db =>
            new JsonResult(Runtime.Metadata.Entities.Select(entity =>
            new TObjectDto()
            {
                Name = entity.Name,
                Source = entity.Source,
                Type = entity.Type.AssemblyQualifiedName ?? entity.Type.Name,
                Attributes = entity.Attributes.Select(a => new TAttributeDto()
                {
                    Name = a.Name,
                    Field = a.Code,
                    Visible = a.Visible
                }).ToArray()
            }).ToArray())
        );

        [HttpGet("[action]/{name}")]
        public async Task<IActionResult> Object(string name) => await UseDatabase(db =>
        {
            var entity = Runtime.Metadata.Entities.FirstOrDefault(oi => oi.Source == name);
            if (entity != null)
            {
                return new JsonResult(new TObjectDto()
                {
                    Name = entity.Name,
                    Source = entity.Source,
                    Type = entity.Type.AssemblyQualifiedName ?? entity.Type.Name,
                    Attributes = entity.Attributes.Select(a => new TAttributeDto()
                    {
                        Name = a.Name,
                        Field = a.Code,
                        Visible = a.Visible
                    }).ToArray()
                });
            }
            throw new Exception("Тип " + name + " не найден!");
        });

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