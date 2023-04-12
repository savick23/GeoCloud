//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: GeoComService - Сервер сбора геоданных.
// Тунель к COM-порту: 109.74.129.114:32325
// HTTP API: 109.74.129.114:32328
// Документация: tps1200_series.pdf
//--------------------------------------------------------------------------------------------------
namespace RmSolution.GeoCom
{
    #region Using
    using System.IO.Ports;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using RmSolution.Data;
    using RmSolution.Devices;
    using RmSolution.Runtime;
    using SmartMinex.Runtime;
    #endregion Using

    public class GeoComService : TModule, IOServer
    {
        #region Declarations

        readonly static byte[] LF = new byte[] { 0x0a };
        readonly static byte[] TERM = new byte[] { 0x0d, 0x0a };
        readonly List<IDevice> _devices = new();
        SerialPortSetting _comsets;

        #endregion Declarations

        #region Properties

        public List<IDevice> Devices => _devices;

        #endregion Properties

        public GeoComService(IRuntime runtime, GeoComAdapterSet adapter) : base(runtime)
        {
            Subscribe = new[] { MSG.RuntimeStarted, MSG.ConsoleCommand };
            _comsets = new SerialPortSetting()
            {
                Name = adapter.Name ?? "COM1",
                BaudRate = adapter.BaudRate ?? 57600,
                DataBits = adapter.DataBits ?? 8,
                StopBits = adapter.StopBits == 1f ? StopBits.One : adapter.StopBits == 1.5f ? StopBits.OnePointFive : adapter.StopBits == 2f ? StopBits.Two : StopBits.None,
                Parity = adapter.Parity,
                FlowControl = int.TryParse(adapter.FlowControl, out var fc) ? fc: 0,
                Interface = adapter.Interface ?? "RS-232", // RS-485
                Fifo = adapter.FIFO ?? true
            };
        }

        public override void Start() => UseDatabase(db =>
        {
            var tEquips = db.Query<TEquipment>();
            base.Start();
        });

        protected override Task ExecuteProcess()
        {
            var sock = new GeoComSocket(new LeicaTotalStationDevice());

            Status = RuntimeStatus.Running;
            while (_sync.WaitOne() && (Status & RuntimeStatus.Loop) > 0)
            {
                while (_esb.TryDequeue(out TMessage m))
                    switch (m.Msg)
                    {
                        case MSG.RuntimeStarted:
                            break;

                        case MSG.ConsoleCommand:
                            if (m.HParam == ProcessId && m.Data is string[] args && args.Length > 0)
                                DoCommand(m.LParam, args);
                            break;
                    }
            }
            return base.ExecuteProcess();
        }

        /// <summary> Выполнить консольную команду.</summary>
        void DoCommand(long idTerminal, string[] args)
        {
            switch (args[0].ToUpper())
            {
                case "COM":
                    Runtime.Send(MSG.Terminal, ProcessId, 0, new Dictionary<string, string>()
                    {
                        { "Name", _comsets.Name },
                        { "BaudRate", _comsets.BaudRate.ToString() },
                        { "DataBits", _comsets.DataBits.ToString() },
                        { "StopBits", _comsets.StopBits switch { StopBits.One => "1", StopBits.OnePointFive => "1.5", StopBits.Two => "2", _ => "0"} },
                        { "Parity", _comsets.Parity.ToString() },
                        { "FlowControl", _comsets.FlowControl.ToString() },
                        { "FIFO", _comsets.Fifo.ToString() },
                        { "Interface", _comsets.Interface }
                    });
                    break;

                case "TCP":
                    Runtime.Send(MSG.Terminal, ProcessId, 0, new Dictionary<string, string>()
                    {
                        { "Name", _comsets.Name },
                        { "BaudRate", _comsets.BaudRate.ToString() },
                        { "DataBits", _comsets.DataBits.ToString() },
                        { "StopBits", _comsets.StopBits switch { StopBits.One => "1", StopBits.OnePointFive => "1.5", StopBits.Two => "2", _ => "0"} },
                        { "Parity", _comsets.Parity.ToString() },
                        { "FlowControl", _comsets.FlowControl.ToString() },
                        { "FIFO", _comsets.Fifo.ToString() },
                        { "Interface", _comsets.Interface }
                    });
                    break;

                case "SEND":
                    if (args.Length > 1 && Regex.IsMatch(args[1].ToUpper(), @"^\d+\.\d+\.\d+\.\d+\:*\d*$"))
                        SendToTcp(Regex.Match(args[1], @"^\d+\.\d+\.\d+\.\d+\:*\d*$").Value, args.Skip(2).ToArray());

                    else if (args.Length > 1 && Regex.IsMatch(args[1].ToUpper(), @"^COM\d+$"))
                        SendToCom(args[1].ToUpper(), args.Skip(2).ToArray());

                    else
                        Runtime.Send(MSG.Terminal, ProcessId, idTerminal, "Не распознан COM-порт \"" + args[1] + "\"");
                    break;

                default:
                    Runtime.Send(MSG.Terminal, ProcessId, idTerminal, "Неизвестная команда: " + string.Join(' ', args));
                    break;
            }
        }

        void SendToCom(string portName, string[] args)
        {
            _comsets.Name = portName;
            var com = new TSerialPort(_comsets);
            try
            {
                com.Open();
                com.Write(Encoding.ASCII.GetBytes(string.Concat(args)));
                Task.Delay(250).Wait();
                var resp = com.Read();
                Runtime.Send(MSG.Terminal, ProcessId, 0, resp == null ? "<нет данных>"
                    : string.Concat(string.Join(' ', resp.Select(n => n.ToString("x2"))), " > ", Encoding.ASCII.GetString(resp)));
            }
            catch (Exception ex)
            {
                Runtime.Send(MSG.Terminal, ProcessId, 0, "Ошибка отправки " + _comsets.Name + ": " + ex.Message);
            }
            finally
            {
                com.Close();
            }
        }

        void SendToTcp(string hostName, string[] args)
        {
            using var tcp = new TTcpClient(Regex.Match(hostName, @"(\d+\.*){4}").Value, int.TryParse(Regex.Match(hostName, @"(?<=\:)\d+?(?=$)").Value, out var port) ? port : 80);
            try
            {
                tcp.Connect();
                tcp.Send(LF.Concat(Encoding.ASCII.GetBytes(string.Concat(args))).Concat(TERM).ToArray());
                byte[] resp;
                int attempt = 120;
                do
                {
                    Task.Delay(250).Wait();
                    resp = tcp.Receive();
                }
                while (resp.Length == 0 && attempt-- > 0);

                Runtime.Send(MSG.Terminal, ProcessId, 0, resp == null || resp.Length == 0 ? "<нет данных>"
                    : string.Concat(string.Join(' ', resp.Select(n => n.ToString("x2"))), " > ", Encoding.ASCII.GetString(resp)));
            }
            catch (Exception ex)
            {
            }
            finally
            {
                tcp.Close();
            }
        }
    }

    public class GeoComAdapterSet
    {
        public GeoComAccessMode? Mode { get; set; }
        public string? Name { get; set; }

        public string? Address { get; set; }

        public int? BaudRate { get; set; }
        public int? DataBits { get; set; }
        public float? StopBits { get; set; }
        public Parity Parity { get; set; }
        public string? FlowControl { get; set; }
        public bool? FIFO { get; set; }
        public string? Interface { get; set; }
    }
}
