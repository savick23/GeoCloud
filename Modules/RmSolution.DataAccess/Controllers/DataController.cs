//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: DataController – Доступ к данным.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using System.Data;
    using Microsoft.AspNetCore.Mvc;
    using RmSolution.Data;
    using RmSolution.Runtime;
    #endregion Using

    public class DataController : SmartController
    {
        public DataController(IRuntime runtime) : base(runtime)
        {
        }

        /// <summary> https://localhost:8087/api/objects </summary>
        [HttpGet("[action]")]
        public async Task<IActionResult> Objects() => await UseDatabase(db =>
            new JsonResult(Runtime.Metadata.Entities
                .Where(e => e.Type == TType.Catalog).OrderBy(e => e.Ordinal)
                .Select(entity => entity.ToDto())));

        /// <summary> https://localhost:8087/api/object/equipments </summary>
        [HttpGet("[action]/{name}")]
        public async Task<IActionResult> Object(string name) => await UseDatabase(db =>
        {
            var entity = Runtime.Metadata.Entities[name];
            if (entity != null)
                return new JsonResult(entity.ToDto());

            throw new Exception("Тип " + name + " не найден!");
        });

        /// <summary> https://localhost:8087/api/data/equipments </summary>
        [HttpGet("[action]/{name}")]
        public async Task<IActionResult> Data(string name)
        {
            if (Runtime.Metadata.Entities[name] != null)
            {
                return new JsonResult(await Runtime.Metadata.GetDataAsync(name));
            }
            throw new Exception("Тип " + name + " не найден!");
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Reference(object request)
        {
            var obj = Runtime.Metadata.GetObject(((System.Text.Json.JsonElement)request).GetProperty("Item").GetInt64());
            if (obj != null)
            {
                using var dt = await Runtime.Metadata.GetReferenceData(obj.Id);
                if (dt != null)
                {
                    using var ms = new TMemoryStream();
                    ms.Write(dt.Rows.Count);
                    foreach (DataRow row in dt.Rows)
                    {
                        ms.Write((long)row[0]);
                        ms.Write((string)row[1]);
                    }
                    return File(ms.ToArray(), "application/octet-stream", false);
                }
            }
            throw new Exception("Тип не найден!");
        }

        /// <summary> https://localhost:8087/api/datatable/equipments </summary>
        [HttpGet("[action]/{name}")]
        public async Task<IActionResult> Rows(string name)
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
        public async Task<IActionResult> Update() => await UseDatabase((db, request) =>
        {
            if (request != null && request is TItemBase item)
                if ((item.Id & TType.RecordMask) == TType.NewId)
                    db.Insert(item);
                else
                    db.Update(item);

            return new JsonResult("{\"result\":\"ok\"}");
        });

        [HttpPost("[action]")]
        public async Task<IActionResult> New() => await UseDatabase((db, objid) =>
        {
            return new JsonResult(Runtime.Metadata.NewItem(objid));
        });
    }
}