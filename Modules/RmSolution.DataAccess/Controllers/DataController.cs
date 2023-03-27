//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: DataController – Доступ к данным.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
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

        /// <summary> http://localhost:8087/api/objects </summary>
        [HttpGet("[action]")]
        public async Task<IActionResult> Objects() => await UseDatabase(db =>
            new JsonResult(Runtime.Metadata.Entities.Where(e => e.Parent == TType.Catalog).OrderBy(e => e.Ordinal).Select(entity =>
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

        /// <summary> http://localhost:8087/api/object/equipments </summary>
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

        /// <summary> http://localhost:8087/api/data/equipments </summary>
        [HttpGet("[action]/{name}")]
        public async Task<IActionResult> Data(string name) => await UseDatabase(db =>
        {
            var mdtype = Runtime.Metadata.Entities.FirstOrDefault(oi => oi.Source == name)?.Type;
            if (mdtype != null)
            {
                var data = Runtime.Metadata.GetData(name);
                return new JsonResult(data);
            }
            throw new Exception("Тип " + name + " не найден!");
        });
    }
}