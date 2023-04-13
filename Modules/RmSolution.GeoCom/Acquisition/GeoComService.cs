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
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using RmSolution.Data;
    using RmSolution.Devices;
    using RmSolution.Runtime;
    #endregion Using

    public class GeoComService : TModule, IOServer
    {
        #region Declarations

        /// <summary> При посылке в начале строки (не обязательно), отчищает входной буфер команд на устройстве.</summary>
        readonly static byte[] LF = new byte[] { 0x0a };
        readonly static byte[] TERM = new byte[] { 0x0d, 0x0a };
        /// <summary> GeoCOM request type 1.</summary>
        readonly static byte[] REQTYPE1 = "%R1Q"u8.ToArray();
        readonly List<IDevice> _devices = new();
        SerialPortSetting _comsets;
        NetworkSetting _netsets;

        #endregion Declarations

        #region Properties

        public List<IDevice> Devices => _devices;
        public int Delay => 250;

        #endregion Properties

        public GeoComService(IRuntime runtime, GeoComAdapterSet adapter) : base(runtime)
        {
            Subscribe = new[] { MSG.RuntimeStarted, MSG.ConsoleCommand };
            _netsets = new NetworkSetting()
            {
                Host = Regex.Match(adapter?.Address ?? string.Empty, @"(\d+\.*){4}").Value,
                Port = int.TryParse(Regex.Match(adapter?.Address ?? string.Empty, @"(?<=\:)\d+").Value, out var port) ? port : 80
            };
            _comsets = new SerialPortSetting()
            {
                PornName = adapter.Name ?? "COM1",
                BaudRate = adapter.BaudRate ?? 19200,
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
            Runtime.Metadata.GetData<TEquipment>()?.ToList().ForEach(d => Devices.Add(new LeicaTotalStationDevice(d, _netsets)));

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
                        { "Name", _comsets.PornName },
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
                        { "Host", _netsets.Host },
                        { "Port", _netsets.Port.ToString() }
                    });
                    break;

                case "SEND":
                    var receiver = args[1].ToUpper();
                    if (args.Length > 1 && Regex.IsMatch(receiver, @"^\d+\.\d+\.\d+\.\d+\:*\d*$"))
                        SendToTcp(Regex.Match(args[1], @"^\d+\.\d+\.\d+\.\d+\:*\d*$").Value, args.Skip(2).ToArray());

                    else if (args.Length > 1 && Regex.IsMatch(receiver, @"^COM\d+$"))
                        SendToCom(args[1].ToUpper(), args.Skip(2).ToArray());

                    else if (Devices.Any(d => d.Code.ToUpper() == receiver || d.Name.ToUpper() == receiver))
                        Send(((IDeviceContext)Devices.First(d => d.Code.ToUpper() == receiver || d.Name.ToUpper() == receiver)).Connection, args);

                    else
                        Runtime.Send(MSG.Terminal, ProcessId, idTerminal, "Не распознан адрес \"" + args[1] + "\"");
                    break;

                default:
                    Runtime.Send(MSG.Terminal, ProcessId, idTerminal, "Неизвестная команда: " + string.Join(' ', args));
                    break;
            }
        }

        void SendToCom(string portName, string[] args)
        {
            _comsets.PornName = portName;
            var com = new RmSerialConnection(_comsets);
            Send(com, args);
        }

        void SendToTcp(string hostName, string[] args)
        {
            using var tcp = new RmNetworkConnection(Regex.Match(hostName, @"(\d+\.*){4}").Value, int.TryParse(Regex.Match(hostName, @"(?<=\:)\d+?(?=$)").Value, out var port) ? port : 80);
            Send(tcp, args);
        }

        void Send(IDeviceConnection device, string[] args)
        {
            try
            {
                device.Open();                
                device.Write(LF.Concat(Encoding.ASCII.GetBytes(string.Concat(args))).Concat(TERM).ToArray());

                byte[] resp;
                int attempt = 120;
                do
                {
                    Task.Delay(Delay).Wait();
                    resp = device.Read();
                }
                while ((resp == null || resp.Length == 0) && attempt-- > 0);

                Runtime.Send(MSG.Terminal, ProcessId, 0, resp == null || resp.Length == 0 ? "<нет данных>"
                    : string.Concat(string.Join(' ', resp.Select(n => n.ToString("x2"))), " > ", Encoding.ASCII.GetString(resp)));
            }
            catch (Exception ex)
            {
                Runtime.Send(MSG.Terminal, ProcessId, 0, nameof(GeoComService) + ": " + ex.Message);
            }
            finally
            {
                device.Close();
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
