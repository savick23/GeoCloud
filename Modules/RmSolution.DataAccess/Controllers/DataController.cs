//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: DataController – Доступ к данным.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using System.Data;
    using System.Net.Http.Headers;
    using Microsoft.AspNetCore.Mvc;
    using RmSolution.Data;
    using RmSolution.Runtime;
    #endregion Using

    public class DataController : SmartController
    {
        public DataController(IRuntime runtime) : base(runtime)
        {
        }

        /// <summary> http://localhost:8087/api/objects </summary>
        [HttpGet("[action]")]
        public async Task<IActionResult> Objects() => await UseDatabase(db =>
            new JsonResult(Runtime.Metadata.Entities.Where(e => e.Type == TType.Catalog).OrderBy(e => e.Ordinal).Select(entity =>
            new TObjectDto()
            {
                Code = entity.Code,
                Name = entity.Name,
                Source = entity.Source,
                Type = entity.CType.AssemblyQualifiedName ?? entity.CType.Name,
                Attributes = entity.Attributes.Select(a => new TAttributeDto()
                {
                    Name = a.Name,
                    Field = a.Field[1..^1],
                    DisplayField = a.DisplayField[1..^1],
                    Visible = a.Visible
                }).ToArray()
            }).ToArray())
        );

        /// <summary> http://localhost:8087/api/object/equipments </summary>
        [HttpGet("[action]/{name}")]
        public async Task<IActionResult> Object(string name) => await UseDatabase(db =>
        {
            var entity = Runtime.Metadata.Entities.FirstOrDefault(oi => oi.Code == name || oi.Name == name || oi.Source == name);
            if (entity != null)
            {
                return new JsonResult(new TObjectDto()
                {
                    Code = entity.Code,
                    Name = entity.Name,
                    Source = entity.Source,
                    Type = entity.CType.AssemblyQualifiedName ?? entity.CType.Name,
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
        public async Task<IActionResult> Data(string name)
        {
            if (Runtime.Metadata.Entities.FirstOrDefault(oi => oi.Source == name) != null)
            {
                return new JsonResult(await Runtime.Metadata.GetDataAsync(name));
            }
            throw new Exception("Тип " + name + " не найден!");
        }

        /// <summary> http://localhost:8087/api/datatable/equipments </summary>
        [HttpGet("[action]/{name}")]
        public async Task<IActionResult> DataTable(string name)
        {
            if (Runtime.Metadata.GetObject(name) != null)
            {
                var data = await Runtime.Metadata.GetDataTableAsync(name);
                if (data != null)
                    return File(TSerializer.Serialize(data), "application/octet-stream", false);

                throw new Exception("Ошибка запроса данных объекта " + name);
            }
            throw new Exception("Тип " + name + " не найден!");
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Update() => await UseDatabase((db, item) =>
        {
            if (item != null) db.Update(item);
            return new JsonResult("{\"result\":\"ok\"}");
        });

    }
}