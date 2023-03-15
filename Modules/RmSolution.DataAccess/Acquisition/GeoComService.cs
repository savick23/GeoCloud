//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: GeoComService - Сервер сбора геоданных.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.DataAccess
{
    #region Using
    using System;
    using System.Threading.Tasks;
    using RmSolution.Runtime;
    #endregion Using

    public class GeoComService : ModuleBase
    {
        public GeoComService(IRuntime runtime) : base(runtime)
        {
            Subscribe = new[] { MSG.RuntimeStarted }; 
        }

        protected override Task ExecuteProcess()
        {
            Status = RuntimeStatus.Running;
            while ((Status & RuntimeStatus.Loop) > 0)
            {
                while (_esb.TryDequeue(out TMessage m))
                    switch (m.Msg)
                    {
                        case MSG.RuntimeStarted:
                            break;
                    }
            }
            return base.ExecuteProcess();
        }
    }
}
