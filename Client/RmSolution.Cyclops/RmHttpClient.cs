//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: RmHttpClient – Веб-клиент.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Cyclops
{
    using System;
    using System.Net.Http.Json;
    #region Using
    using System.Reflection;
    using RmSolution.Data;
    #endregion Using

    public class RmHttpClient : HttpClient
    {
        public static string Title => Assembly.GetExecutingAssembly()?.GetCustomAttributes<AssemblyProductAttribute>().FirstOrDefault()?.Product ?? "RmSolution.Cyclops";
        public static string Version => Assembly.GetExecutingAssembly().GetName()?.Version?.ToString(2) ?? "3.1";
        public static string DataServer => "http://localhost:8087/api/";

        public async Task<dynamic?> Query(Type type)
        {
            return await this.GetFromJsonAsync(string.Concat(DataServer, "data/equipments"), type);
        }
    }
}
