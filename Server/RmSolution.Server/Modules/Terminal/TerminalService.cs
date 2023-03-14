//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TerminalService – Терминальный сервер telnet. Консольный доступ к Системе.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Runtime
{
    #region Using
    using RmSolution.Server;
    using System.Net.Sockets;
    #endregion Using

    /// <summary> [Системный модуль] Терминальный сервер telnet. Консольный доступ к Системе.</summary>
    public sealed class TerminalService : ModuleBase, IStartup
    {
        #region Properties

        TcpServer _listener;

        public int Port { get; set; }

        #endregion Properties

        public TerminalService(IRuntime runtime, int? port) : base(runtime)
        {
            Port = port ?? 23;
            Name = "Служба терминалов, порт " + Port;
        }

        public override void Start()
        {
            Status = RuntimeStatus.StartPending;
            try
            {
                _listener = new TcpServer(Port);
                _listener.Listen(13);
            }
            catch (Exception ex)
            {
                LastError = ex;
                _listener = null;
                Runtime.Send(MSG.ErrorMessage, 0, 0, ex);
                throw;
            }
            base.Start();
        }

        protected override async Task ExecuteProcess()
        {
            var client = new SocketAsyncEventArgs();
            client.Completed += AcceptCompleted;
            _listener.AcceptAsync(client);

            Status = RuntimeStatus.Running;
            while ((Status & RuntimeStatus.Loop) > 0)
            {
                await Task.Delay(500);
            }
            Status = RuntimeStatus.StopPending;
            _listener.Shutdown(SocketShutdown.Both);
            _listener.Close(0);
            _listener.Dispose();
            _listener = null;

            await base.ExecuteProcess();
        }

        void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            do
            {
                try
                {
                    if (e.SocketError == SocketError.Success)
                        ((RuntimeService)Runtime).Modules.AddSingleton(typeof(TelnetSession), e.AcceptSocket);
                }
                catch
                { }
                finally
                {
                    e.AcceptSocket = null; // to enable reuse
                }
            }
            while (!_listener.AcceptAsync(e));
        }
    }
}