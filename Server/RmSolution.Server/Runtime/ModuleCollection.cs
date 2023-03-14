//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». Smart System Platform 3.1. Все права защищены.
// Описание: ModuleCollection –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Server
{
    #region Using
    using RmSolution.Runtime;
    using System.Collections.Concurrent;
    using System.Reflection;
    #endregion Using

    /// <summary> Модуль управления модулями (микросервисами).</summary>
    internal sealed class ModuleCollection : List<IModule>
    {
        #region Declarations

        readonly ILogger _logger;
        readonly RuntimeService _rtm;
        readonly IConfiguration _cfg;
        readonly ConcurrentDictionary<ModuleDescript, IService> _modules = new();

        volatile int _count;

        public event EventHandler Created;
        public event EventHandler Removed;

        #endregion Declarations

        public ModuleCollection(RuntimeService runtime, IConfiguration configuration, ILogger<RuntimeService> logger)
        {
            _rtm = runtime;
            _cfg = configuration;
            _logger = logger;
            AddSingleton<IRuntime>(runtime);
        }

        #region Add/Remove operations

        public ModuleCollection AddSingleton<TModule>(IService implementationInstance) where TModule : IService
        {
            _modules.TryAdd(new ModuleDescript(++_count, typeof(IService)), implementationInstance);
            return this;
        }

        /// <summary> Создание и запуск единственного экземпляра модуля.</summary>
        public IModule AddSingleton(Type serviceType, params object[] args)
        {
            IModule mod = null;
            try
            {
                var parameters = serviceType.GetConstructors()[0].GetParameters();
                var prms = new object[parameters.Length];
                int n = 0;
                var modinfo = args.FirstOrDefault(m => m is ModuleInfo) as ModuleInfo;
                for (int i = 0; i < prms.Length; i++)
                {
                    var prm = parameters[i];
                    var ptype = prm.ParameterType;
                    var injectionType = GetService(ptype)
                        ?? (ptype == typeof(IConfiguration) ? _cfg : null);

                    if (injectionType == null)
                    {
                        object pval = modinfo?[prm.Name];
                        prms[i] = (args.Length > n && ptype == args[n].GetType())
                            ? args[n]
                            : pval != null
                              ? (ptype.IsEnum && Enum.TryParse(ptype, modinfo[prm.Name], true, out object enumval))
                                ? enumval
                                : ptype == typeof(int?) && pval is string ? int.Parse(pval.ToString()) : pval
                              : ptype.Equals(typeof(string))
                                ? default(string)
                                : ptype.IsClass
                                    ? modinfo.Get(ptype, prm.Name)
                                    : Activator.CreateInstance(ptype);

                        n++;
                    }
                    else prms[i] = injectionType;
                }
                mod = (IModule)Activator.CreateInstance(serviceType, prms);
                mod.ProcessId = ++_count;
                mod.Name = modinfo?.Name ?? mod.Name;
                _modules.TryAdd(new ModuleDescript(mod.ProcessId, mod.GetType()), mod);
                Task.Run(() => Created?.Invoke(mod, EventArgs.Empty));

                if (mod.Subscribe != null)
                    foreach (var msg in mod.Subscribe)
                        if (_rtm.Dispatcher.ContainsKey(msg))
                            _rtm.Dispatcher[msg] += mod.ProcessMessage;
                        else
                            _rtm.Dispatcher.TryAdd(msg, mod.ProcessMessage);

                mod.Start();
            }
            catch (Exception ex)
            {
                mod?.Kill();
                _logger.LogError($"Ошибка запуска модуля {serviceType.Name}: \n" + ex.Message);
            }
            return mod;
        }

        /// <summary> Возвращает модуль содержащий указанный интерфейс (тип).</summary>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        object GetService(Type serviceType) =>
            _modules.Values.FirstOrDefault(m => m.GetType().GetInterfaces().Contains(serviceType));

        public TService GetModule<TService>() =>
            (TService)_modules.Values.FirstOrDefault(m => m is TService);

        public List<TService> GetModules<TService>() =>
            _modules.Values.Where(m => m is TService).Select(m => (TService)m).ToList();

        #endregion Add/Remove operations

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

    class ModuleInfo
    {
        public long Id { get; }
        public StartState Start { get; }
        public string Name { get; }
        public string Type { get; }

        /// <summary> Зависимости. Идентификаторы процесса от которого зависит данная служба.</summary>
        public long[] Depend { get; set; }

        IConfigurationSection _config;

        public string this[string name] => _config.GetSection(name).Value;
        public object Get(Type type, string name)
        {
            object res = Activator.CreateInstance(type);
            _config.GetSection(name).Bind(res);
            return res;
        }

        /// <summary> Чтение параметров запуска модуля.</summary>
        public ModuleInfo(IConfigurationSection config)
        {
            _config = config;
            foreach (var p in config.GetChildren().ToDictionary(sect => sect.Key, sect => sect.Value))
                switch (p.Key)
                {
                    case "start":
                        Start = Enum.TryParse<StartState>(config[p.Key], true, out var start) ? start : StartState.Disable;
                        break;
                    case "name":
                        Name = p.Value;
                        break;
                    case "type":
                        Type = p.Value;
                        break;
                }
        }
    }

    public enum StartState
    {
        Auto, Manual, Disable
    }
}