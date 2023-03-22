//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: HttpApiService – Веб-служба доступа к данным Системы.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Web
{
    #region Using
    using RmSolution.Runtime;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    #endregion Using

    public class HttpApiService : ModuleBase
    {
        #region Declarations

        public const string ROUTEBASE = "/api/";
        const string CORSPOLICENAME = "RmSolution";

        WebApplication? _host;

        int _port;

        #endregion Declarations

        public HttpApiService(IRuntime runtime, int? port) : base(runtime)
        {
            _port = port ?? 80;
        }

        void Init()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.ConfigureKestrel(opt =>
            {
                opt.ListenAnyIP(_port);
            });

            builder.Services.AddCors(opt =>
                opt.AddPolicy(CORSPOLICENAME, police => // Политика для всех узлов -->
                    police.WithOrigins("http://*")
                    .AllowAnyHeader()
                    /*.AllowAnyMethod()*/.WithMethods("GET", "POST")));

            builder.Services.AddSingleton(Runtime);
            builder.Services.AddControllers() // Добавим контроллеры из сборки -->
                .AddApplicationPart(GetType().Assembly)
                .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.PropertyNamingPolicy = null;  // Выводит наименования параметров как есть, без изменения регистра букв
                    opt.JsonSerializerOptions.IncludeFields = true;         // Включим в результат поля и свойства
                    opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull;
                });

            _host = builder.Build();
            //if (!_host.Environment.IsDevelopment())
            {
                _host.UseExceptionHandler(new ExceptionHandlerOptions() { ExceptionHandler = OnExceptionHandler });
            }
            _host.UseCors(CORSPOLICENAME);
            _host.UseHttpsRedirection();
            _host.UseAuthorization();
            _host.MapControllers();
        }

        protected override Task ExecuteProcess()
        {
            Init();
            _host.RunAsync().ConfigureAwait(false);

            Status = RuntimeStatus.Running;
            while (_sync.WaitOne() && (Status & RuntimeStatus.Loop) > 0)
            {

            }
            return base.ExecuteProcess();
        }

        #region Private methods

        /// <summary> Глобальный обработчик ошибок WebAPI.</summary>
        Task OnExceptionHandler(HttpContext context)
        {
            return Task.CompletedTask;
        }

        #endregion Private methods
    }
}