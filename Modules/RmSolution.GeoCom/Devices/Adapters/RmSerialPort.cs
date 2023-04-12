//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: RmSerialPort - Класс серийного порта COM.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Devices
{
    #region Using
    using System;
    using System.IO.Ports;
    #endregion Using

    public class RmSerialPort : SerialPort, IDeviceConnection
    {
        const int BUFSIZE = 1024;
        byte[] _buffer = new byte[BUFSIZE];

        public int Available { get { return BytesToRead; } }
        public bool Connected => IsOpen;
        public bool DataAvailable => BytesToRead > 0;

        public RmSerialPort(string name, int baudrate, int databits, StopBits stopbits, Parity parity)
            : base(name, baudrate, parity, databits, stopbits)
        {
            ReadTimeout = 1000;
            WriteTimeout = 1000;

        }

        public RmSerialPort(SerialPortSetting sets)
            : this(sets.Name, sets.BaudRate, sets.DataBits, sets.StopBits, sets.Parity)
        {
        }

        public byte[]? Read()
        {
            byte[]? data = null;
            if (BytesToRead > 0)
                try
                {
                    int i = 0;
                    while (i == 0 || BytesToRead > 0)
                    {
                        var cnt = Read(_buffer, i, BUFSIZE - i);
                        i += cnt;
                    }
                    data = new byte[i];
                    Buffer.BlockCopy(_buffer, 0, data, 0, i);
                }
                catch (TimeoutException)
                {
                    data = null;
                }
                catch (Exception)
                {
                    data = null;
                }
            return data;
        }

        public void Write(byte[] data) =>
            Write(data, 0, data.Length);
    }

    public struct SerialPortSetting
    {
        /// <summary> Имя последовательного порта.</summary>
        public string Name { get; set; }
        /// <summary> Описание при необходимости.</summary>
        public string Description { get; set; }
        /// <summary> Скорость, бит в секунду.</summary>
        public int BaudRate { get; set; }
        /// <summary> Биты данных.</summary>
        public int DataBits { get; set; }
        /// <summary> Стоповые биты.</summary>
        public StopBits StopBits { get; set; }
        /// <summary> Чётность.</summary>
        public Parity Parity { get; set; }
        /// <summary> Управление потоком.</summary>
        public int FlowControl { get; set; }
        /// <summary> Физический интерфейс.</summary>
        public string Interface { get; set; }
        /// <summary> Буфер ввода/вывода.</summary>
        public bool Fifo { get; set; }

        public override string ToString() =>
            $"Baud rate:{BaudRate}; Stop bits:{StopBits}";
    }
}
