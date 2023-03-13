//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». Smart System Platform 3.1. Все права защищены.
// Описание: Служба Windows или демон Linux.
//--------------------------------------------------------------------------------------------------
using System.Reflection;
using System.Text;
using RmSolution.Server;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // fix error: No data is available for encoding 1251

await Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .UseSystemd()
    .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
    .ConfigureServices(srv => srv.AddHostedService<RuntimeService>())
    .Build()
    .RunAsync();
