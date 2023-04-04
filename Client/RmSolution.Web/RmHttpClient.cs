//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: RmHttpClient – Веб-клиент.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Web
{
    #region Using
    using System;
    using System.Net.Http.Json;
    using System.Reflection;
    using System.Text.Json.Serialization;
    using System.Text.Json;
    using RmSolution.Data;
    using System.ComponentModel;
    using RmSolution.DataAnnotations;
    using System.Xml.Linq;
    #endregion Using

    public class RmHttpClient : HttpClient
    {
        #region Declarations

        static readonly JsonSerializerOptions _jsonOptions = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            PropertyNameCaseInsensitive = true,
            Converters =
                {
                    new TRefTypeConverter(),
                }
        };

        #endregion Declarations

        public static string Title => Assembly.GetExecutingAssembly()?.GetCustomAttributes<AssemblyProductAttribute>().FirstOrDefault()?.Product ?? "RmSolution.RmGeo";
        public static string Version => Assembly.GetExecutingAssembly().GetName()?.Version?.ToString(2) ?? "0.0.0.0";
        public static string DataServer => "http://localhost:8087/api/";

        public RmHttpClient()
        {
            BaseAddress = new Uri(DataServer);
        }

        #region API Data operations

        /// <summary> Возвращает список метаданных всех объектов в Системе.</summary>
        public async Task<TObjectDto[]?> GetObjectsAsync()
        {
            return await this.GetFromJsonAsync<TObjectDto[]?>(string.Concat(WellKnownObjects.Api.GetObjects));
        }

        /// <summary> Возвращает метаданные объекта конфигурации.</summary>
        public async Task<TObjectDto?> GetObjectAsync(string? typeName)
        {
            if (typeName != null)
                return await this.GetFromJsonAsync<TObjectDto>(string.Concat(WellKnownObjects.Api.GetObject + typeName));

            throw new Exception("Не найден объект типа " + typeName);
        }

        /// <summary> Возвращает данные объекта конфигурации в виде классов (модели).</summary>
        public async Task<object?> GetDataAsync(string? typeName)
        {
            if (typeName != null)
            {
                var mdtype = await GetObjectAsync(typeName);
                if (mdtype != null)
                {
                    var type = Type.GetType(mdtype.Type);
                    if (type != null)
                        return await this.GetFromJsonAsync(string.Concat(WellKnownObjects.Api.GetData + mdtype.Source), Array.CreateInstance(type, 0).GetType(), _jsonOptions);
                }
            }
            throw new Exception("Не найден объект типа " + typeName);
        }

        /// <summary> Обновляет данные записи на сервере.</summary>
        public async Task UpdateAsync(object? item)
        {
            if (item != null)
            {
                await PostAsync(WellKnownObjects.Api.PostUpdate, JsonContent.Create(new XItemEnvelop(item), typeof(XItemEnvelop), null, _jsonOptions));
            }
        }

        /// <summary> Возвращает новую запись объекта конфигурации.</summary>
        public async Task<object?> NewItemAsync(TObjectDto? mdtype)
        {
            if (mdtype != null)
            {
                //return await this.GetFromJsonAsync<TObjectDto>(string.Concat(DataServer, "new/" + mdtype.Code));
                return Activator.CreateInstance(await GetObjectTypeAsync(mdtype.Code));
            }
            throw new Exception("Объект не указан!");
        }

        #endregion API Data operations

        public async Task<Type> GetObjectTypeAsync(string? typeName) =>
            Type.GetType((await GetObjectAsync(typeName)).Type);

        #region Nested types

        class XItemEnvelop
        {
            public string? Type { get; }
            public object? Item { get; }

            public XItemEnvelop(object? item)
            {
                Type = item?.GetType().AssemblyQualifiedName;
                Item = item;
            }
        }

        #endregion Nested types
    }
}
