//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: GeoComService - Сервер сбора геоданных.
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
                case "SEND":
                    if (args.Length > 1 && Regex.IsMatch(args[1].ToUpper(), @"^COM\d+$"))
                    {
                        SendCom(args[1].ToUpper(), args.Skip(2).ToArray());
                    }
                    else
                        Runtime.Send(MSG.Terminal, ProcessId, idTerminal, "Не распознан COM-порт \"" + args[1] + "\"");
                    break;

                default:
                    Runtime.Send(MSG.Terminal, ProcessId, idTerminal, "Неизвестная команда: " + string.Join(' ', args));
                    break;
            }
        }

        void SendCom(string name, string[] args)
        {
            _comsets.Name = name;
            var com = new TSerialPort(_comsets);
            try
            {
                com.Open();
                com.Write(Encoding.ASCII.GetBytes(string.Concat(args)));
                Task.Delay(1000);
                var resp = com.Read();
                var txt = string.Concat("0x", resp.Select(n => n.ToString("x2")));
            }
            catch (Exception ex)
            {
                Runtime.Send(MSG.Terminal, ProcessId, 0, "Ошибка отправки " + name + ": " + ex.Message);
            }
            finally
            {
                com.Close();
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
