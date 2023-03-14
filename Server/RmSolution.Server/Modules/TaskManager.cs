﻿//--------------------------------------------------------------------------------------------------
// (С) 2011-2022 ООО «Инфорсер Инжиниринг». Inforser E-Passport Personalization Platform 6.11.
// Описание: TaskManager – Диспетчер задач. Сервис управления модулями (микросервисами).
//--------------------------------------------------------------------------------------------------
namespace Inforser.Runtime
{
    #region Using
    using System;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using RmSolution.Runtime;
    using RmSolution.Server;
    #endregion Using

    /// <summary> [Системный модуль] Диспетчер задач. Сервис управления модулями (микросервисами).</summary>
    sealed class TaskManager : ModuleBase, IStartup
    {
        #region Declarations

        const string ASMEXT = ".dll";

        readonly IConfiguration _cfg;

        #endregion Declarations

        public TaskManager(IRuntime runtime, IConfiguration config) : base(runtime)
        {
            Subscribe = new[] { MSG.StartRuntime };
            Name = "Диспетчер задач";
            _cfg = config;
        }

        protected override async Task ExecuteProcess()
        {
            Status = RuntimeStatus.Running;
            while (_sync.WaitOne() && (Status & RuntimeStatus.Loop) > 0)
            {
                while (_esb.TryDequeue(out TMessage m))
                {
                    switch (m.Msg)
                    {
                        case MSG.StartRuntime:
                            try
                            {
                                StartModules();
                            }
                            catch (Exception ex)
                            {

                            }
                            finally
                            {
                                Runtime.Send(MSG.StartServer, 0);
                            }
                            break;
                    }
                }
            }
            await base.ExecuteProcess();
        }

        #region Private methods

        /// <summary> Запуск системы. Инициалиазация всех приложений и модулей. Даётся 3 мин.</summary>
        void StartModules()
        {
            var rtm = (RuntimeService)Runtime;
            var startlist = ReadConfiguration().Values.ToList();

            var started = DateTime.Now;
            do
            {
                foreach (var modinfo in startlist.ToList())
                {
                    bool canrun = true;
                    if (modinfo.Depend != null)
                        foreach (var depend in modinfo.Depend)
                        {
                            if (startlist.FirstOrDefault(m => m.Id == depend)?.Depend?.Contains(modinfo.Id) ?? false)
                            {
                                Runtime.Send(MSG.ErrorMessage, 0, 0, $"Обнаружена циклическая ссылка зависимостей #{modinfo.Id} <–> #{depend}. Модули не запущены!");
                                startlist.Remove(modinfo);
                                startlist.Remove(startlist.FirstOrDefault(m => m.Id == depend));
                                canrun = false;
                            }
                            else
                            {
                                var masterMod = rtm.Modules.FirstOrDefault(m => m.Id == depend);
                                if (masterMod == null || masterMod.Status != RuntimeStatus.Running)
                                {
                                    canrun = false;
                                    break;
                                }
                            }
                        }

                    if (canrun)
                    {
                        string lib;
                        startlist.Remove(modinfo);
                        Type type;
                        type = Type.GetType(modinfo.Type, false, true)
                            ?? Type.GetType(modinfo.Type,
                                (asm) => File.Exists(lib = Path.Combine(Runtime.GetWorkDirectory(), asm.FullName + ASMEXT)) ? Assembly.LoadFile(lib) : null,
                                (asm, name, ignore) => asm.GetType(name, ignore, true),
                                false, true);

                        if (type != null)
                        {
                            Runtime.Send(MSG.Terminal, 0, 0, DateTime.Now.ToString("HH:mm:ss.fff") + " Создание модуля: " + modinfo.Name);

                            var startMod = rtm.Modules.AddSingleton(type, modinfo);

                            if (startMod != null && startMod.Status == RuntimeStatus.StartPending)
                            {
                                startMod.Id = modinfo.Id;
                                Runtime.Send(MSG.Terminal, 0, 0, DateTime.Now.ToString("HH:mm:ss.fff") + " Запуск модуля: " + modinfo.Name);
                            }
                        }
                        else
                        {
                            lib = Regex.Match(modinfo.Type, @"(?<=,\s*).*").Value.Trim();
                            var cls = Regex.Match(modinfo.Type, @".*?(?=,|$)").Value.Trim();
                            if (!string.IsNullOrEmpty(lib) && !File.Exists(lib + ASMEXT) && !File.Exists(lib + ".exe"))
                                Runtime.Send(MSG.ErrorMessage, 0, 0, $"Ошибка запуска модуля \"{modinfo.Type}\"\nФайл {lib}.dll не найден!");
                            else
                                Runtime.Send(MSG.ErrorMessage, 0, 0, $"Ошибка запуска модуля \"{modinfo.Type}\"\nКласс {cls} не найден!");
                        }
                    }
                }
                Task.Delay(100);
            }
            while (startlist.Count > 0 && (DateTime.Now - started).TotalSeconds < 120);
        }

        #endregion Private methods

        #region Configurations

        /// <summary> Чтение данных из конфигурационного файла приложения.</summary>
        Dictionary<string, ModuleInfo> ReadConfiguration() =>
            _cfg.GetSection("runtimeOptions:modules").GetChildren()
                .ToDictionary(sect => _cfg[sect.Path + ":name"], sect => new ModuleInfo(sect));

        #endregion Configurations
    }
}