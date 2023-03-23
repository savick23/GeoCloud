//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: RmHttpClient – Веб-клиент.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Cyclops
{
    #region Using
    using System;
    using System.Net.Http.Json;
    using System.Reflection;
    using RmSolution.Data;
    #endregion Using

    public class RmHttpClient : HttpClient
    {
        public static string Title => Assembly.GetExecutingAssembly()?.GetCustomAttributes<AssemblyProductAttribute>().FirstOrDefault()?.Product ?? "RmSolution.Cyclops";
        public static string Version => Assembly.GetExecutingAssembly().GetName()?.Version?.ToString(2) ?? "3.1";
        public static string DataServer => "http://localhost:8087/api/";

        public RmHttpClient()
        {
        }

        public async Task<TObjectDto?> GetObject(string? typeName)
        {
            if (typeName != null)
                return await this.GetFromJsonAsync<TObjectDto>(string.Concat(DataServer, "object/" + typeName));

            throw new Exception("Не найден объект типа " + typeName);
        }

        public async Task<dynamic?> Query(string? typeName)
        {
            if (typeName != null)
            {
                var mdtype = await this.GetFromJsonAsync<TObjectDto>(string.Concat(DataServer, "object/" + typeName));
                var type = Type.GetType(mdtype.Type);
                return await this.GetFromJsonAsync(string.Concat(DataServer, "data/" + mdtype.Source), Array.CreateInstance(type, 0).GetType());
            }
            throw new Exception("Не найден объект типа " + typeName);
        }
    }
}
