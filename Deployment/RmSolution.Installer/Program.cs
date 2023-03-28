using System;
using System.Windows.Forms;

namespace RmSolution.Deployment
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#if DEBUG
            new RmPackage().CreateCabinetFile();
#endif
            Application.Run(new RmSolution());
        }
    }
}
