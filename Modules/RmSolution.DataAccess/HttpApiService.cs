//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: HttpApiService – Веб-служба доступа к данным Системы.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using RmSolution.Runtime;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    #endregion Using

    public class HttpApiService : TModule
    {
        #region Declarations

        public const string ROUTEBASE = "/api/";
        const string CORSPOLICENAME = "RmSolution";

        WebApplication? _host;

        readonly SslCertificate? _cert;
        readonly int _port;

        #endregion Declarations

        #region Constructor

        public HttpApiService(IRuntime runtime, int? port, string scheme, SslCertificate certificate) : base(runtime)
        {
            _cert = scheme.ToLower() == "https" ? certificate : null;
            _port = port ?? (scheme.ToLower() == "https" ? 443 : 80);
            Name = "Веб-сервис доступа к данным, порт " + _port;
        }

        #endregion Constructor

        void Init()
        {
            var builder = WebApplication.CreateBuilder();
            if (_cert != null)
            {
                var cert = Path.Combine(Runtime.GetWorkDirectory(), _cert.Path ?? string.Empty);
                if (!File.Exists(cert)) throw new Exception("Не найден SSL-сертификат: " + cert);
                builder.WebHost.ConfigureKestrel(opt =>
                {
                    opt.ListenAnyIP(_port, listen =>
                    {
                        listen.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                        listen.UseHttps(new X509Certificate2(cert, _cert.Password));
                    });
                });
            }
            else
                builder.WebHost.ConfigureKestrel(opt =>
                {
                    opt.ListenAnyIP(_port);
                });

            builder.Services.AddCors(opt =>
                opt.AddPolicy(CORSPOLICENAME, police => // Политика для всех узлов -->
                    police.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()));

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
            _host.UseHttpsRedirection();
            _host.UseRouting();
            _host.UseCors(CORSPOLICENAME);
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