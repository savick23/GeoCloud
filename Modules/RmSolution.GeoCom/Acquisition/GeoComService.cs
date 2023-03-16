//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: GeoComService - Сервер сбора геоданных.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.GeoCom
{
    #region Using
    using System;
    using System.IO.Ports;
    using System.Threading.Tasks;
    using RmSolution.Runtime;
    #endregion Using

    public class GeoComService : ModuleBase, IOServer
    {
        public GeoComService(IRuntime runtime, GeoComAdapterSet adapter) : base(runtime)
        {
            Subscribe = new[] { MSG.RuntimeStarted }; 
        }

        protected override Task ExecuteProcess()
        {
            Status = RuntimeStatus.Running;
            while (_sync.WaitOne() && (Status & RuntimeStatus.Loop) > 0)
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

    public enum GeoComAdapterMode
    {
        Tcp, Com
    }

    public class GeoComAdapterSet
    {
        public GeoComAdapterMode? Mode { get; set; }
        public string? Port { get; set; }

        public string? Address { get; set; }
        public string? Mask { get; set; }

        public int? BaudRate { get; set; }
        public int? DataBits { get; set; }
        public float? StopBits { get; set; }
        public Parity Parity { get; set; }
        public string? FlowControl { get; set; }
        public bool? FIFO { get; set; }
        public string? Interface { get; set; }
    }
}
