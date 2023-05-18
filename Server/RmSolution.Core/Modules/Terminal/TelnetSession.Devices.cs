//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TelnetSession.Devices – Терминальная клиентская сессия telnet.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    using RmSolution.Data;
    #region Using
    using System;
    using System.Data;
    using System.Reflection;
    using System.Text;
    #endregion Using

    partial class TelnetSession
    {
        List<IDevice>? GetDevices() =>
            GetModules().Where(m => typeof(IOServer).IsAssignableFrom(m.GetType())).SelectMany(d => ((IOServer)d).Devices).ToList();

        /// <summary> Обработчик команд для различных устройств.</summary>
        void DoDeviceCommand(StringBuilder output, string command, string[] args)
        {
        }

        /// <summary> Получить список подключённых устройств. Команда DEV.</summary>
        void ShowDevices(StringBuilder output, string command, string[] args)
        {
            using var data = new DataTable();
            data.Columns.AddRange(new[] {
                new DataColumn("N"),
                new DataColumn("Код"),
                new DataColumn("Наименование")
            });
            var devs = GetDevices();
            if (devs?.Count > 0)
            {
                int i = 0;
                devs.ForEach(d => data.Rows.Add(++i, d.Code, d.Name));

                PrintTable(output, data);
            }
            else output.Append("Нет подключённых устройств!");
        }
    }
}
