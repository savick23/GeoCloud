//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». Smart System Platform 3.1. Все права защищены.
// Описание: RuntimeService –
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Server
{
    #region Using
    using RmSolution.Runtime;
    #endregion Using

    public sealed class TerminalService : ModuleBase, IStartup
    {
        #region Declarations

        public int Port { get; }

        #endregion Declarations

        public TerminalService(IRuntime runtime, int port) : base(runtime)
        {
            Port = 23;
            Name = "Служба терминалов, порт " + Port;
        }
    }
}