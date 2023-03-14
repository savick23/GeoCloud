//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». Smart System Platform 3.1. Все права защищены.
// Описание: RuntimeService –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Server
{
    #region Using
    using RmSolution.Runtime;
    using System.Collections.Concurrent;
    using RmSolution.DataAccess;
    using System.Net.WebSockets;
    #endregion Using

    public sealed class RuntimeService : BackgroundService, IRuntime
    {
        #region Declarations

        readonly ConcurrentDictionary<ModuleDescript, IModule> _modules = new();

        readonly ILogger<RuntimeService> _logger;

        /// <summary> Системная шина предприятия. Очередь сообщений.</summary>
        readonly ConcurrentQueue<TMessage> _esb = new();
        /// <summary> Системная шина предприятия. Менеджер расписаний.</summary>
        readonly TaskScheduler<TMessage> _schedule = new();

        #endregion Declarations

        #region Properties

        public long Id { get; set; }
        public string Name { get; set; }
        public int ProcessId { get; set; }
        public RuntimeStatus Status { get; private set; }
        public Version Version { get; }

        /// <summary> Запущенные модули в системе. Диспетчер задач.</summary>
        internal readonly ModuleCollection Modules = new();
        public long MessageCount { get; private set; }

        public readonly Dictionary<string, object> Parameters;

        #endregion Properties

        #region Message

        public void Send(TMessage m)
        {
            ++MessageCount;
            _esb.Enqueue(m);
        }

        int Send(TMessage m, ScheduleParams pars)
        {
            if (_schedule.Add(m, pars) is int id)
                return id;

            Send(m);

            return 0;
        }

        public void Send(int type, long lparam) =>
            Send(type, lparam, 0, null);

        public void Send(int type, long lparam, long hparam) =>
            Send(type, lparam, hparam, null);

        public void Send(int type, long lparam, long hparam, object data) =>
            Send(new TMessage(type, lparam, hparam, data));

        public int Send(TMessage m, long delay) =>
            Send(m, new ScheduleParams { InitialDelay = delay });

        public int Send(TMessage m, long delay, long period) =>
            Send(m, new ScheduleParams { InitialDelay = delay, Period = period });

        public int Send(TMessage m, DateTime dateTime) =>
            Send(m, new ScheduleParams { DateTime = dateTime });

        public int Send(int type, long lparam, long hparam, object data, long delay) =>
            Send(new TMessage(type, lparam, hparam, data), new ScheduleParams { InitialDelay = delay });

        public int Send(int type, long lparam, long hparam, object data, long delay, long period) =>
            Send(new TMessage(type, lparam, hparam, data), new ScheduleParams { InitialDelay = delay, Period = period });

        public int Send(int type, long lparam, long hparam, object data, DateTime dateTime) =>
            Send(new TMessage(type, lparam, hparam, data), new ScheduleParams { DateTime = dateTime });

        public bool RemoveTask(int id) =>
            _schedule.Remove(id);

        public bool ResetTask(int id) =>
            _schedule.Reset(id);

        public bool Modify(int id, TMessage m, long delay) =>
            _schedule.Modify(id, m, new ScheduleParams { InitialDelay = delay });

        public bool Modify(int id, TMessage m, long delay, long period) =>
            _schedule.Modify(id, m, new ScheduleParams { InitialDelay = delay, Period = period });

        public bool Modify(int id, TMessage m, DateTime dateTime) =>
            _schedule.Modify(id, m, new ScheduleParams { DateTime = dateTime });

        #endregion Message

        public RuntimeService(ILogger<RuntimeService> logger)
        {
            _logger = logger;
            Name = "РМ Гео";
            Version = System.Reflection.Assembly.GetExecutingAssembly().GetName()?.Version ?? new Version();
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