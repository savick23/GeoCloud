//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». Smart System Platform 3.1. Все права защищены.
// Описание: ModuleBase – Базовый модуль.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    #region Using
    using System;
    #endregion Using

    public class ModuleBase : IModule
    {
        #region Declarations

        protected IRuntime Runtime;

        #endregion Declarations

        #region Properties

        public long Id { get; set; }
        public int ProcessId { get; set; }
        public RuntimeStatus Status { get; set; }

        public long Site { get; set; }
        public string Name { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion Properties

        public ModuleBase(IRuntime runtime)
        {
            Runtime = runtime;
        }
    }
}