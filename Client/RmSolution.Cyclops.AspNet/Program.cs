//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание:
//--------------------------------------------------------------------------------------------------
using RmSolution.Web;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
    ContentRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
    WebRootPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "wwwroot")
});
builder.Host.UseWindowsService()
    .ConfigureServices(srv =>
    {
        srv.AddRazorPages();
        srv.AddServerSideBlazor();
        srv.AddScoped(sp => new RmHttpClient());
        srv.AddHostedService<RmInformationService>();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
