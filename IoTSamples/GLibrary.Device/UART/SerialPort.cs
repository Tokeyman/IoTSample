using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using Windows.Foundation;

namespace GLibrary.Device.UART
{
    public class SerialPort : CommonBase
    {
        #region 私有属性
        private SerialDevice SerialDevice { get; set; }
        private DataWriter DataWriter { get; set; }
        private DataReader DataReader { get; set; }

        private CancellationTokenSource ReadCancellationToken { get; set; }

        public string Selector { get; private set; }
        public int BaudRate { get; private set; }

        #endregion

        #region 构造函数
        public SerialPort(string Selector, int BaudRate)
        {
            this.Selector = Selector;
            this.BaudRate = BaudRate;
        }

        private async Task GetSerialPort(string Selector)
        {
            string AdvancedQuerySyntax = SerialDevice.GetDeviceSelector(Selector);
            var DeviceInformationCollection = await DeviceInformation.FindAllAsync(AdvancedQuerySyntax);
            DeviceInformation Entry = DeviceInformationCollection[0];
            this.SerialDevice = await SerialDevice.FromIdAsync(Entry.Id);
            if (SerialDevice == null) throw new NullReferenceException();
        }

        /// <summary>
        /// 获得所有可用的串口号
        /// </summary>
        /// <returns>串口号</returns>
        public static async Task<List<string>> GetPortNames()
        {
            string aqs = SerialDevice.GetDeviceSelector();
            var dis = await DeviceInformation.FindAllAsync(aqs);
            List<string> portNames = new List<string>();
            foreach (var item in dis)
            {
                var serialDevice = await SerialDevice.FromIdAsync(item.Id);
                if (serialDevice != null)
                {
                    portNames.Add(serialDevice.PortName);
                }
            }
            portNames.Sort();
            return portNames;
        }

        #endregion 构造函数

        #region 方法
        public async void Open()
        {
            await GetSerialPort(Selector);
            if (SerialDevice != null)
            {
                this.SerialDevice.BaudRate = Convert.ToUInt32(BaudRate);
                this.SerialDevice.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                this.SerialDevice.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                this.SerialDevice.Parity = SerialParity.None;
                this.SerialDevice.StopBits = SerialStopBitCount.One;
                this.SerialDevice.DataBits = 8;
                this.SerialDevice.Handshake = SerialHandshake.None;
            }
            // Create cancellation token object to close I/O operations when closing the device
            this.ReadCancellationToken = new CancellationTokenSource();
            Listen();
        }

        private async void Listen()
        {
            try
            {
                if (SerialDevice != null)
                {
                    DataReader = new DataReader(SerialDevice.InputStream);

                    while (true)
                    {
                        await ReadAsync(ReadCancellationToken.Token);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataReader != null)
                {
                    DataReader.DetachStream();
                    DataReader = null;
                }
            }
        }

        private async Task ReadAsync(CancellationToken CancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            // If task cancellation was requested, comply
            CancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            DataReader.InputStreamOptions = InputStreamOptions.Partial;

            using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken))
            {
                // Create a task object to wait for data on the serialPort.InputStream
                loadAsyncTask = DataReader.LoadAsync(ReadBufferLength).AsTask(childCancellationTokenSource.Token);
                // Launch the task and wait
                UInt32 bytesRead = await loadAsyncTask;
                if (bytesRead > 0)
                {
                    byte[] buffer = new byte[bytesRead];
                    DataReader.ReadBytes(buffer);
                    RaiseDataReceived(buffer);
                }
            }
        }

        public override void Dispose()
        {
            CancelReadTask();
            if (SerialDevice != null)
            {
                SerialDevice.Dispose();
            }
            SerialDevice = null;
        }

        /// <summary>
        /// CancelReadTask:
        /// - Uses the ReadCancellationTokenSource to cancel read operations
        /// </summary>
        private void CancelReadTask()
        {
            if (ReadCancellationToken != null)
            {
                if (!ReadCancellationToken.IsCancellationRequested)
                {
                    ReadCancellationToken.Cancel();
                }
            }
        }

        public async void Send(byte[] buffer)
        {
            try
            {
                if (SerialDevice != null)
                {
                    DataWriter = new DataWriter(SerialDevice.OutputStream);
                    await WriteAsync(buffer);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (DataWriter != null)
                {
                    DataWriter.DetachStream();
                    DataWriter = null;
                }
            }
        }

        private async Task<int> WriteAsync(byte[] buffer)
        {
            Task<UInt32> storeAsyncTask;

            if (buffer.Length != 0)
            {
                // Load the text from the sendText input text box to the dataWriter object
                DataWriter.WriteBytes(buffer);

                // Launch an async task to complete the write operation
                storeAsyncTask = DataWriter.StoreAsync().AsTask();

                UInt32 bytesWritten = await storeAsyncTask;
                return Convert.ToInt32(bytesWritten);
            }
            else
            {
                return -1;
            }
        }
        #endregion 方法

        #region 事件
        public event TypedEventHandler<SerialPort, SerialDataReceivedArgs> DataReceived;

        private void RaiseDataReceived(byte[] buffer)
        {
            DataReceived?.Invoke(this, new SerialDataReceivedArgs(buffer.Length, buffer));
        }
        #endregion 事件
    }

    public class SerialDataReceivedArgs
    {
        public int BytesToRead { get; set; }
        public byte[] ReceivedBuffer { get; set; }

        public SerialDataReceivedArgs(int BytesToRead, byte[] ReceivedBuffer)
        {
            this.BytesToRead = BytesToRead;
            this.ReceivedBuffer = ReceivedBuffer;
        }
    }
}
