//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: Shell –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Deployment
{
    #region Using
    using System.Diagnostics;
    #endregion Using

    /// <summary> Команды операционной системы.</summary>
    public static class Shell
    {
        /// <summary> Выполнить команду операционной системы с возвратом результата.</summary>
        public static string Run(string name, params string[] args)
        {
            try
            {
                var cmd = new Process();
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.FileName = name;
                cmd.StartInfo.Arguments = string.Join(" ", args);
                cmd.Start();
                cmd.WaitForExit();
                return cmd.StandardOutput.ReadToEnd().Replace("\n", "\r\n");
            }
            catch
            {
                throw;
            }
        }
    }
}
