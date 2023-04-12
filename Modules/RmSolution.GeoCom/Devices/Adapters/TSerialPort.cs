//--------------------------------------------------------------------------------------------------
// (С) 2020-2023 ООО «РМ Солюшн». RM System Platform 3.1. Все права защищены.
// Описание: TSerialPort - Класс серийного порта COM.
//--------------------------------------------------------------------------------------------------
namespace RmSolution.Devices
{
    #region Using
    using System;
    using System.IO.Ports;
    #endregion Using

    interface ISerialPort
    {
        int Available { get; }

        bool Open();
        byte[] Read();
        void Write(byte[] data);
    }

    public class TSerialPort : ISerialPort
    {
        const int BUFSIZE = 1024;
        SerialPort _port;
        byte[] _buffer = new byte[BUFSIZE];

        public int Available { get { return _port.BytesToRead; } }

        public TSerialPort(string name, int baudrate, int databits, StopBits stopbits, Parity parity)
        {
            _port = new SerialPort(name, baudrate, parity, databits, stopbits)
            {
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };
        }

        public TSerialPort(SerialPortSetting sets)
            : this(sets.Name, sets.BaudRate, sets.DataBits, sets.StopBits, sets.Parity)
        {
        }

        /// <summary> Открыть порт.</summary>
        public bool Open()
        {
            try
            {
                _port.Open();
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary> Закрыть порт.</summary>
        public bool Close()
        {
            try
            {
                _port.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public int BytesToRead
        {
            get { return _port.BytesToRead; }
        }

        public byte[] Read()
        {
            byte[] data = null;
            if (_port.BytesToRead > 0)
                try
                {
                    int i = 0;
                    while (i == 0 || _port.BytesToRead > 0)
                    {
                        var cnt = _port.Read(_buffer, i, BUFSIZE - i);
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

        public int Read(byte[] buffer, int offset, int count)
        {
            return _port.Read(buffer, offset, count);
        }

        public void Write(byte[] data)
        {
            _port.Write(data, 0, data.Length);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _port.Write(buffer, offset, count);
        }
    }

    public struct SerialPortSetting
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public Parity Parity { get; set; }
        public int FlowControl { get; set; }
        public string Interface { get; set; }
        public bool Fifo { get; set; }

        public override string ToString() =>
            $"Baud rate:{BaudRate}; Stop bits:{StopBits}";
    }
}
