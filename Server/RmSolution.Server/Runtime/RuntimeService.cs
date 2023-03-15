//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: RuntimeService –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Server
{
    #region Using
    using RmSolution.Runtime;
    using System.Collections.Concurrent;
    using RmSolution.DataAccess;
    using System.Net.WebSockets;
    using System.Reflection;
    #endregion Using

    delegate void ProcessMessageEventHandler(ref TMessage m);

    public sealed class RuntimeService : BackgroundService, IRuntime
    {
        #region Declarations

        readonly ILogger<RuntimeService> _logger;

        /// <summary> Системная шина предприятия. Очередь сообщений.</summary>
        readonly ConcurrentQueue<TMessage> _esb = new();
        /// <summary> Системная шина предприятия. Менеджер расписаний.</summary>
        readonly TaskScheduler<TMessage> _schedule = new();

        /// <summary> Запущенные модули в системе. Диспетчер задач.</summary>
        readonly ConcurrentDictionary<ModuleDescript, IModule> _modules = new();
        /// <summary> Диспетчер системной шины предприятия ESB.</summary>
        internal readonly ConcurrentDictionary<int, ProcessMessageEventHandler> Dispatcher = new();

        #endregion Declarations

        #region Properties

        public string Name { get; set; }
        public Version Version { get; }

        /// <summary> Запущенные модули в системе. Диспетчер задач.</summary>
        internal readonly ModuleManager Modules;
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

        #region Constructors

        public RuntimeService(ILogger<RuntimeService> logger, IConfiguration config)
        {
            _logger = logger;
            Name = "Сервер приложений " + (Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute)).FirstOrDefault() as AssemblyProductAttribute)?.Product;
            Version = Assembly.GetExecutingAssembly().GetName()?.Version ?? new Version();

            Modules = new ModuleManager(this, config, logger);
            Modules.Created += OnModuleCreated;
            Modules.Removed += OnModuleRemoved;
        }

        #endregion Constructors

        public override Task StartAsync(CancellationToken tkn)
        {
            return base.StartAsync(tkn);
        }

        public override Task StopAsync(CancellationToken tkn)
        {
            return base.StopAsync(tkn);
        }

        protected override async Task ExecuteAsync(CancellationToken tkn)
        {
            AppDomain.CurrentDomain.GetAssemblies() // Звапуск системных обязательных модулей -->
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetInterfaces().Contains(typeof(IStartup))).ToList()
                .ForEach(t => Modules.AddSingleton(t));

            _schedule.Start();
            await Task.Delay(100, tkn);

            Send(MSG.RuntimeStarted, 0, 0, null);

            while (!tkn.IsCancellationRequested)
            {
                if (_esb.TryDequeue(out TMessage m))
                {
                    if (Dispatcher.ContainsKey(MSG.All))
                        Dispatcher[MSG.All].Invoke(ref m);

                    if (Dispatcher.ContainsKey(m.Msg))
                        Dispatcher[m.Msg]?.Invoke(ref m);
                }
                else
                    await Task.Delay(50, tkn);
            }
            _schedule.Stop();
        }

        public string GetWorkDirectory() => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        #region Events

        void OnModuleCreated(object sender, EventArgs e)
        {
            if (sender is IModule mod)
                Console.WriteLine($"Created[{mod.ProcessId}]: {mod.Name}");
        }

        void OnModuleRemoved(object sender, EventArgs e)
        {
            if (sender is IModule mod)
                Console.WriteLine($"Removed[{mod.ProcessId}]: {mod.Name}");
        }

        #endregion Events

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