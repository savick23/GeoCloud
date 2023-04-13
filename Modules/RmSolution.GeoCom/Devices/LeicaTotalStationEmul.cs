//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: LeicaTotalStationEmul - Эмулятор устройства Leica Total Station (чтение из COM-порта).
// Для работы эмулятора, необходимо создать пару связанных виртуальных COM-портов (null-modem bind)
// в операционной системе.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.GeoCom
{
    using System.Text;
    #region Using
    using System.Threading.Tasks;
    using RmSolution.Devices;
    using RmSolution.Runtime;
    #endregion Using

    /// <summary> Эмулятор устройства Leica Total Station. </summary>
    /// <remarks> Для работы эмулятора, необходимо создать виртуальный COM-порт в операционной системе.</remarks>
    public class LeicaTotalStationEmul : TModule
    {
        #region Declarations

        readonly static byte[] TERM = new byte[] { 0x0d, 0x0a };

        RmSerialPort _port;

        #endregion Declarations

        #region Properties

        public string PortName { get; set; }
        /// <summary> Задержака между опросами последовательного порта, мс.</summary>
        public int Delay { get; set; } = 50;

        #endregion Properties

        public LeicaTotalStationEmul(IRuntime runtime, string? portname, int? baudRate) : base(runtime)
        {
            Subscribe = new[] { MSG.RuntimeStarted };
            PortName = portname ?? "COM1";
            _port = new RmSerialPort(PortName, baudRate ?? 57600, 8, System.IO.Ports.StopBits.One, System.IO.Ports.Parity.None);
            Name = "Эмулятор устройства Leica Total Station, порт " + PortName;
        }

        protected async override Task ExecuteProcess()
        {
            _port.Open();
            Status = RuntimeStatus.Running;
            while ((Status & RuntimeStatus.Loop) > 0)
            {
                if (_port.Available > 0)
                {
                    var resp = _port.Read();
                    if (resp.Length > 0 && resp[^2..^0].SequenceEqual(TERM))
                    {
                        resp = resp[0] == '\n' ? resp[1..^2] : resp[0..^2];
                        _port.Write(resp.Concat(TERM).ToArray());
                    }
                    else _port.Write(Encoding.ASCII.GetBytes("undefined").ToArray());

                    while (_esb.TryDequeue(out TMessage m))
                        switch (m.Msg)
                        {
                            case MSG.RuntimeStarted:
                                break;
                        }
                }
                await Task.Delay(Delay);
            }
            await base.ExecuteProcess();
        }
    }
}
