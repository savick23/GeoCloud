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
    using RmSolution.Data;
    #endregion Using

    public class RmHttpClient : HttpClient
    {
        public static string Title => Assembly.GetExecutingAssembly()?.GetCustomAttributes<AssemblyProductAttribute>().FirstOrDefault()?.Product ?? "RmSolution.RmGeo";
        public static string Version => Assembly.GetExecutingAssembly().GetName()?.Version?.ToString(2) ?? "0.0.0.0";
        public static string DataServer => "http://localhost:8087/api/";

        public RmHttpClient()
        {
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
                        return await this.GetFromJsonAsync(string.Concat(DataServer, "data/" + mdtype.Source), Array.CreateInstance(type, 0).GetType());
                }
            }
            throw new Exception("Не найден объект типа " + typeName);
        }
    }
}
