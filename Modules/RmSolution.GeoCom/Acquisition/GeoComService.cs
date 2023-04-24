//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: GeoComService - Сервер сбора геоданных.
// Тунель к COM-порту: 109.74.129.114:32325
// HTTP API: 109.74.129.114:32328
//--------------------------------------------------------------------------------------------------
namespace RmSolution.GeoCom
{
    #region Using
    using System.Data;
    using System.IO.Ports;
    using System.Reflection;
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

        #region Private methods

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
                        Send((IDeviceConnection)Devices.First(d => d.Code.ToUpper() == receiver || d.Name.ToUpper() == receiver), args);

                    else
                        Runtime.Send(MSG.Terminal, ProcessId, idTerminal, "Не распознан адрес \"" + args[1] + "\"");
                    break;

                case "DEVICES":
                    Runtime.Send(MSG.Terminal, ProcessId, 0, Devices.ToDictionary(k => k.Code, v => string.Concat(v.Name)));
                    break;

                case "CONFIG":
                    if (args.Length > 1)
                        ReadDeviceConfig(args[1], args.Skip(2).ToArray());
                    break;

                case "FUNC":
                case "FUNCTIONS":
                    if (args.Length > 1)
                        ShowDeviceFunctions(args[1], args.Skip(2).ToArray());
                    break;

                case "CALL":
                    if (args.Length > 1)
                        CallDeviceFunction(args[1], args.Skip(2).ToArray());
                    break;

                default:
                    Runtime.Send(MSG.Terminal, ProcessId, idTerminal, "Неизвестная команда: " + string.Join(' ', args));
                    break;
            }
        }

        bool TryFindDevice(string idDevice, out IDevice device)
        {
            idDevice = idDevice.ToUpper();
            device = Devices.First(d => d.Code.ToUpper() == idDevice || d.Name.ToUpper() == idDevice);
            return device != null;
        }

        IDevice? FindDevice(string idDevice) =>
            TryFindDevice(idDevice, out var dev) ? dev : null;

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

        void ReadDeviceConfig(string idDevice, string[] args)
        {
            try
            {
                if (TryFindDevice(idDevice, out var dev) && dev is LeicaTotalStationDevice stdev)
                {
                    var cfg = stdev.ReadConfig();
                    if (cfg != null)
                        Runtime.Send(MSG.Terminal, ProcessId, 0, cfg.ToDictionary(k => k.Key, v => v.Value.ToString()));
                    else
                        Runtime.Send(MSG.Terminal, ProcessId, 0, "Ошибка чтения конфигурации устройства " + idDevice + ".");
                }
                else Runtime.Send(MSG.Terminal, ProcessId, 0, "Устройство " + idDevice + " не найдено!");
            }
            catch (Exception ex)
            {
                Runtime.Send(MSG.ErrorMessage, ProcessId, 0, ex);
            }
        }

        /// <summary> Возвращает список всех доступных функций.</summary>
        void ShowDeviceFunctions(string idDevice, string[] args)
        {
            if (TryFindDevice(idDevice, out var dev))
            {
                var dt = new DataTable();
                dt.Columns.AddRange(new DataColumn[] { new DataColumn("name", typeof(string)) });
                var t = dev.GetType().GetMethods().Where(call => call.GetCustomAttributes(typeof(COMFAttribute)) != null);
                dev.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(call => call.GetCustomAttribute<COMFAttribute>() != null)
                    .OrderBy(call => call.Name).ToList()
                    .ForEach(call => dt.Rows.Add(call.Name));

                Runtime.Send(MSG.Terminal, ProcessId, 0, dt);
            }
            else Runtime.Send(MSG.Terminal, ProcessId, 0, "Устройство " + idDevice + " не найдено!");
        }

        /// <summary> Выполнить функцию (инструкцию) на устройстве.</summary>
        void CallDeviceFunction(string idDevice, string[] args)
        {
            if (TryFindDevice(idDevice, out var dev) && args.Length > 0
                && dev.GetType().GetMethod(args[0], BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase) is MethodInfo call
                && call.GetCustomAttributes<COMFAttribute>() != null)
            {
                var parameters = call.GetParameters();
                var prms = new object[parameters.Length];
                int i = 0;
                foreach(var prm in parameters)
                {
                    if (args.Length <= i + 1) return;
                    var val = args[i + 1];
                    if (prm.ParameterType.IsEnum)
                        prms[i] = Enum.TryParse(prm.ParameterType, val, true, out var eval) ? eval : Activator.CreateInstance(prm.ParameterType);
                    else
                        prms[i] = val;

                    i++;
                }
                try
                {
                    var val = call.Invoke(dev, prms);
                    if (val is LeicaTotalStationDevice.ZResponse resp)
                        Runtime.Send(MSG.Terminal, ProcessId, 0, resp.Data == null || resp.Data.Length == 0 ? "<нет данных>"
                            : string.Concat(string.Join(' ', resp.Data.Select(n => n.ToString("x2"))), " > ", resp.Response, " [", resp.Executed.Value.ToString(@"hh\:mm\:ss\.fff"), "]"));
                    else
                        Runtime.Send(MSG.Terminal, ProcessId, 0, "Результат: " + val.ToString());
                }
                catch (Exception ex)
                {
                    Runtime.Send(MSG.Terminal, ProcessId, 0, ex.InnerException?.Message ?? ex.Message);
                }
            }
        }

        #endregion Private methods
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
