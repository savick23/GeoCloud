//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: SmartController – Базовый класс контроллера для доступа к данным.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using System.Text;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;
    using RmSolution.Runtime;
    using RmSolution.Data;
    #endregion Using

    [ApiController]
    [Route(HttpApiService.ROUTEBASE)]
    public class SmartController : ControllerBase
    {
        /// <summary> Настройки JSON-сериализатора.</summary>
        static readonly JsonSerializerSettings _jsonOptions = new()
        {
            Converters = new[] { new JsonKnownTypeConverter() }
        };

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

        /// <summary> Подключение и выполнение в среде БД.</summary>
        protected async Task<T> UseDatabase<T>(Func<IDatabase, object?, T> handler)
        {
            var db = Runtime.CreateDbConnection();
            try
            {
                db.Open();
                return await Task.Run(() => handler.Invoke(db, FromJsonItem(ReadFromBody())));
            }
            finally
            {
                db.Close();
            }
        }

        /// <summary> Чтение JSON-параметров из POST-запроса.</summary>
        /// <remarks> Конструкция ([FromBody] string input) not worked </remarks>
        protected string ReadFromBody()
        {
            using var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8, true, 1024, false);
            return reader.ReadToEndAsync().Result.ToString();
        }

        /// <summary> Чтение Json-значения из тела запроса.</summary>
        static object? FromJsonItem(string jsonValue)
        {
            var envelop = (JObject?)JsonConvert.DeserializeObject(jsonValue);
            if (envelop != null)
            {
                if (envelop["Item"] is JValue val)
                    return val.Value;
                else
                {
                    var item = ((JObject?)envelop["Item"])?.ToString();
                    if (item != null)
                    {
                        var type = Type.GetType(((JValue?)envelop["Type"]).Value.ToString());
                        return JsonConvert.DeserializeObject(item, type, _jsonOptions);
                    }
                }
            }
            return null;
        }

        class JsonKnownTypeConverter: JsonConverter
        {
            public JsonKnownTypeConverter()
            {
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(TRefType);
            }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                string? view = null;
                long? id = null;
                int i = 0;
                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.PropertyName:
                            if (i == 0) id = long.Parse(reader.ReadAsString());
                            else if (i == 1) view = reader.ReadAsString();
                            i++;
                            break;
                        case JsonToken.EndObject:
                            return new TRefType(id, view);
                    }
                }
                return TRefType.Empty;
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        #endregion Database methods...
    }
}
