//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». Smart System Platform 3.1. Все права защищены.
// Описание: RuntimeService –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Server
{
    using RmSolution.DataAccess;
    #region Using
    using RmSolution.Runtime;
    using System.Collections.Concurrent;
    using System.Reflection;
    #endregion Using

    public sealed class RuntimeService : BackgroundService, IRuntime
    {
        #region Declarations

        readonly ConcurrentDictionary<ModuleDescript, IModule> _modules = new();

        #endregion Declarations

        #region Properties

        public RuntimeStatus Status { get; private set; }

        /// <summary> Запущенные модули в системе. Диспетчер задач.</summary>
        internal readonly ModuleCollection Modules = new();

        #endregion Properties

        private readonly ILogger<RuntimeService> _logger;

        public RuntimeService(ILogger<RuntimeService> logger)
        {
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Status = RuntimeStatus.StartPending;

            AppDomain.CurrentDomain.GetAssemblies() // Звапуск системных обязательных модулей -->
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetInterfaces().Contains(typeof(IStartup))).ToList()
                .ForEach(t => Modules.AddSingleton(t));

            Status = RuntimeStatus.Running;
            new LeicaTotalStationDevice();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
            Status = RuntimeStatus.Stopped;
        }

        #region Nested types

        struct ModuleDescript
        {
            public long ProcessId;
            public Type Type;

            public ModuleDescript(long processId, Type type)
            {
                ProcessId = processId;
                Type = type;
            }

            public static implicit operator ModuleDescript(long value) =>
                new ModuleDescript(value, typeof(object));

            public static bool operator ==(ModuleDescript a, ModuleDescript b)
            {
                return a.ProcessId == b.ProcessId;
            }

            public static bool operator !=(ModuleDescript a, ModuleDescript b)
            {
                return a.ProcessId != b.ProcessId;
            }

            public override bool Equals(object obj)
            {
                return this == (ModuleDescript)obj;
            }

            public override int GetHashCode()
            {
                return (int)ProcessId;
            }
        }

        #endregion Nested types
    }
}