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

        public async Task<TObjectDto[]?> GetObjectsAsync()
        {
            return await this.GetFromJsonAsync<TObjectDto[]?>(string.Concat(DataServer, "objects"));
        }

        public async Task<TObjectDto?> GetObjectAsync(string? typeName)
        {
            if (typeName != null)
                return await this.GetFromJsonAsync<TObjectDto>(string.Concat(DataServer, "object/" + typeName));

            throw new Exception("Не найден объект типа " + typeName);
        }

        public async Task<object?> QueryAsync(string? typeName)
        {
            if (typeName != null)
            {
                var mdtype = await GetObjectAsync(typeName);
                if (mdtype != null)
                {
                    var type = Type.GetType(mdtype.Type);
                    if (type != null)
                        return await this.GetFromJsonAsync(string.Concat(DataServer, "data/", mdtype.Source), Array.CreateInstance(type, 0).GetType(), _jsonOptions);
                }
            }
            throw new Exception("Не найден объект типа " + typeName);
        }

        public async Task<Type> GetObjectTypeAsync(string? typeName) =>
            Type.GetType((await GetObjectAsync(typeName)).Type);

        public async Task UpdateAsync(object? item)
        {
            if (item != null)
            {
                await PostAsync(string.Concat(DataServer, "update"), JsonContent.Create(new XItemEnvelop(item), typeof(XItemEnvelop), null, _jsonOptions));
            }
        }

        public async Task<object?> NewItem(TObjectDto? mdtype)
        {
            if (mdtype != null)
            {
                return Activator.CreateInstance(await GetObjectTypeAsync(mdtype.Code));
            }
            throw new Exception("Объект не указан!");
        }

        class XItemEnvelop
        {
            public string Type { get; }
            public object? Item { get; }

            public XItemEnvelop(object? item)
            {
                Type = item.GetType().AssemblyQualifiedName;
                Item = item;
            }
        }
    }
}
