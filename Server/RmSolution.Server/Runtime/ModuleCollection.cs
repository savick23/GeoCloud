//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». Smart System Platform 3.1. Все права защищены.
// Описание: ModuleCollection –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Server
{
    #region Using
    using RmSolution.Runtime;
    using System.Reflection;
    #endregion Using

    /// <summary> Модуль управления модулями (микросервисами).</summary>
    internal sealed class ModuleCollection : List<IModule>
    {
        /// <summary> Создание и запуск единственного экземпляра модуля.</summary>
        public IModule AddSingleton(Type serviceType, params object[] args)
        {
            IModule mod = null;
            try
            {
                //var parameters = serviceType.GetConstructors()[0].GetParameters();
                //var prms = new object[parameters.Length];
                //int n = 0;
                //var modinfo = args.FirstOrDefault(m => m is ModuleInfo) as ModuleInfo;
                //for (int i = 0; i < prms.Length; i++)
                //{
                //    var prm = parameters[i];
                //    var ptype = prm.ParameterType;
                //    var injectionType = GetService(ptype)
                //        ?? (ptype == typeof(IConfiguration) ? _cfg : null);

                //    if (injectionType == null)
                //    {
                //        object pval = modinfo?[prm.Name];
                //        prms[i] = (args.Length > n && ptype == args[n].GetType())
                //            ? args[n]
                //            : pval != null
                //              ? (ptype.IsEnum && Enum.TryParse(ptype, modinfo[prm.Name], true, out object enumval))
                //                ? enumval
                //                : ptype == typeof(int?) && pval is string ? int.Parse(pval.ToString()) : pval
                //              : ptype.Equals(typeof(string))
                //                ? default(string)
                //                : ptype.IsClass
                //                    ? modinfo.Get(ptype, prm.Name)
                //                    : Activator.CreateInstance(ptype);

                //        n++;
                //    }
                //    else prms[i] = injectionType;
                //}
                //mod = (IModule)Activator.CreateInstance(serviceType, prms);
                //mod.ProcessId = ++_count;
                //mod.Name = modinfo?.Name ?? mod.Name;
                //_modules.TryAdd(new ModuleDescript(mod.ProcessId, mod.GetType()), mod);
                //Task.Run(() => Created?.Invoke(mod, EventArgs.Empty));

                //if (mod.Subscribe != null)
                //    foreach (var msg in mod.Subscribe)
                //        if (_rtm.Dispatcher.ContainsKey(msg))
                //            _rtm.Dispatcher[msg] += mod.ProcessMessage;
                //        else
                //            _rtm.Dispatcher.TryAdd(msg, mod.ProcessMessage);

                //mod.Start();
            }
            catch (Exception ex)
            {
                //mod?.Kill();
                //_logger.LogError($"Ошибка запуска модуля {serviceType.Name}: \n" + ex.Message);
            }
            return mod;
        }
    }
}