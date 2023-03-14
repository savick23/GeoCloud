//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». Smart System Platform 3.1. Все права защищены.
// Описание: TelnetSession.Services – Терминальная клиентская сессия telnet.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    #region Using
    using System.Text;
    using System.Data;
    using System.Net;
    using System.Diagnostics;
    using RmSolution.Server;
    #endregion Using

    partial class TelnetSession
    {
        static DateTime? _prevCpuStartTime;
        static TimeSpan _prevTotalProcTime;

        /// <summary> Получить список модулей.</summary>
        void ShowModules(StringBuilder output, string command, string[] args)
        {
            using var data = new DataTable();
            data.Columns.AddRange(new[] {
                new DataColumn("ИД"),
                new DataColumn("Наименование сервиса"),
                new DataColumn("Статус"),
                new DataColumn("Сообщений"),
                new DataColumn("Время")
            });
            var rtm = (RuntimeService)Runtime;
            data.Rows.Add(0, rtm.Name, RuntimeStatus.Running, rtm.MessageCount);

            ((RuntimeService)Runtime).Modules.GetModules<IModule>().ForEach(m =>
                data.Rows.Add(
                    m.ProcessId,
                    m.Name,
                    m.Status,
                    "—",
                    "—")
                );

            PrintTable(output, data);
        }

        /// <summary> Получить сведения о системе СКПТ (объектовый сервер).</summary>
        void ShowSystemInfo(StringBuilder output, string command, string[] args)
        {
            var rtm = (RuntimeService)Runtime;
            var prc = Process.GetCurrentProcess();

            PrintDictionary(output, new Dictionary<string, string>()
            {
                { "Версия ОС", Environment.OSVersion.VersionString
                    + (System.Runtime.InteropServices.Marshal.SizeOf(typeof(IntPtr)) == 8 ? " x64 " : " x86 ")
                    + Environment.OSVersion.ServicePack },
                { "Среда выполнения", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription },
                { "Загрузка процессора", GetCpuUsage().ToString() + " %" },
                { "Используемая память", Math.Round(prc.PrivateMemorySize64 / 1048576.0, 2).ToString() + " Мб" }
            }
            , ": ");
        }

        static double GetCpuUsage()
        {
            var start = DateTime.UtcNow;
            var usage = Process.GetCurrentProcess().TotalProcessorTime;
            if (!_prevCpuStartTime.HasValue)
            {
                _prevCpuStartTime = start;
                _prevTotalProcTime = usage;
            }
            var cpuUsedMs = (usage - _prevTotalProcTime).TotalMilliseconds;
            var totalMsPassed = (start - _prevCpuStartTime.Value).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            _prevCpuStartTime = start;
            _prevTotalProcTime = usage;

            return Math.Round(cpuUsageTotal * 100.0, 2);
        }
    }
}
