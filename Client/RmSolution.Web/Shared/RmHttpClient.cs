//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: RmHttpClient – Веб-клиент.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Web
{
    #region Using
    using System;
    using System.Net;
    using System.Net.Http.Json;
    using System.Reflection;
    using System.Text.Json.Serialization;
    using System.Text.Json;
    using RmSolution.Data;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Authentication;
    using System.Reflection.Metadata;
    using System.Security.Cryptography;
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

        /// <summary> Кэш метаданных.</summary>
        List<TObjectDto> _objects = new();

        #endregion Declarations

        #region Properties

        public static string Title => Assembly.GetExecutingAssembly()?.GetCustomAttributes<AssemblyProductAttribute>().FirstOrDefault()?.Product ?? "RmSolution.RmGeo";
        public static string Version => Assembly.GetExecutingAssembly().GetName()?.Version?.ToString(2) ?? "0.0.0.0";
        public static string DataServer => "http://localhost:8087/api/";

        #endregion Properties

        public RmHttpClient()
        {
            /*
            var webhandler = new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                SslProtocols = SslProtocols.Tls12
            };
            var cert2 = new X509Certificate2(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "cert\\RmSolution.RmGeo.pfx"), "7HfpJnvthm!", X509KeyStorageFlags.MachineKeySet);
            webhandler.ClientCertificates.Add(cert2);

            //     h.ClientCertificates.Add();
            // ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            */
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
                await PostAsync(WellKnownObjects.Api.PostUpdateItem, JsonContent.Create(new XItemEnvelop(item), typeof(XItemEnvelop), null, _jsonOptions));
            }
        }

        /// <summary> Возвращает новую запись объекта конфигурации.</summary>
        public async Task<object?> NewItemAsync(TObjectDto? mdtype)
        {
            if (mdtype != null)
            {
                var resp = await PostAsync(WellKnownObjects.Api.PostNewItem, JsonContent.Create(new XItemEnvelop(mdtype.Id), typeof(XItemEnvelop), null, _jsonOptions));
                return await resp.Content.ReadFromJsonAsync(Type.GetType(mdtype.Type));
            }
            throw new Exception("Объект не указан!");
        }
        
        public List<TRefType>? GetReferenceData(long objid)
        {
            var resp = AsyncHelper.RunSync(() => PostBytes(WellKnownObjects.Api.PostReference, JsonContent.Create(new XItemEnvelop(objid), typeof(XItemEnvelop), null, _jsonOptions)));         
            if (resp != null)
            {
                using var ms = new TMemoryStream(resp);
                var cnt = ms.ReadInt32();
                var res = new List<TRefType>(cnt);
                for (int i = 0; i < cnt; i++)
                    res.Add(new TRefType(ms.ReadInt64(), ms.ReadString()));

                return res;
            }
            return null;
        }

        async Task<byte[]?> GetBytes(string uri)
        {
            using var resp = await GetAsync(uri);
            return resp.IsSuccessStatusCode ? await resp.Content.ReadAsByteArrayAsync() : null;
        }

        async Task<byte[]?> PostBytes(string uri, HttpContent? content)
        {
            using var resp = await PostAsync(uri, content);
            return resp.IsSuccessStatusCode ? await resp.Content.ReadAsByteArrayAsync() : null;
        }

        #endregion API Data operations

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
