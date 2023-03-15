//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: Служба Windows или демон Linux.
//--------------------------------------------------------------------------------------------------
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using RmSolution.Data;
using RmSolution.Server;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // fix error: No data is available for encoding 1251

await Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .UseSystemd()
    .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg.AddCommandLine(args, new Dictionary<string, string>());
        cfg.AddJsonFile(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location) + ".runtimeconfig.json", true);
    })
    .ConfigureServices(srv =>
    {
        srv.AddTransient(srv => CreateDatabaseConnection(srv));
        srv.AddHostedService<RuntimeService>();
    })
    .Build()
    .RunAsync();

static IDatabase CreateDatabaseConnection(IServiceProvider services)
{
    var cfg = services.GetService<IConfiguration>();
    var providers = cfg.GetSection("runtimeOptions:providers").GetChildren().ToDictionary(sect => cfg[sect.Path + ":name"], sect => cfg[sect.Path + ":type"]);
    var connstr = cfg.GetSection("runtimeOptions:datasource").Value;
    var provider = Regex.Match(connstr, "(?<=Provider=).*?(?=;)").Value;
    if (!providers.ContainsKey(provider)) return null;
    connstr = Regex.Replace(connstr, @"Provider=[^;.]*;", string.Empty);
    if (!connstr.EndsWith(";")) connstr += ";";
    return (IDatabase)Activator.CreateInstance(Type.GetType(providers[provider]), connstr);
} 