using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Threading;
using System.Diagnostics;

namespace GLibrary.Device.Net
{
    public class TcpClient:CommonBase
    {
        private StreamSocket Socket { get; set; }
        private DataWriter DataWriter { get; set; }
        private DataReader DataReader { get; set; }
        private CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public bool IsRunning { get; private set; }

        public int LocalPort { get; private set; }
        public HostName LocalHost { get; private set; }

        public int RemotePort { get; private set; }
        public HostName RemoteHost { get; private set; }



        #region 构造函数

        /// <summary>
        /// 给TcpServer使用的，外部无法调用
        /// </summary>
        /// <param name="Socket"></param>
        internal TcpClient(StreamSocket Socket)
        {
            this.IsRunning = true;
            this.LocalHost = Socket.Information.LocalAddress;
            this.LocalPort = Convert.ToInt32(Socket.Information.LocalPort);
            this.RemoteHost = Socket.Information.RemoteAddress;
            this.RemotePort = Convert.ToInt32(Socket.Information.RemotePort);
            this.Socket = Socket;
        }

        /// <summary>
        /// 匿名发送 无需绑定本地IP和端口
        /// </summary>
        /// <param name="RemoteHost"></param>
        /// <param name="RemotePort"></param>
        public TcpClient(HostName RemoteHost, int RemotePort)
        {
            this.RemoteHost = RemoteHost;
            this.RemotePort = RemotePort;
            try
            {
                Socket = new StreamSocket();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Methord:TcpClient(HostName RemoteHost,int RemotePort):Initiali Failed:" + ex.Message);
                RaiseOnException(this, ex);
            }
        }

        /// <summary>
        /// 标准发送 绑定本地IP和端口
        /// </summary>
        /// <param name="LocalHost"></param>
        /// <param name="LocalPort"></param>
        /// <param name="RemoteHost"></param>
        /// <param name="RemotePort"></param>
        public TcpClient(HostName LocalHost, int LocalPort, HostName RemoteHost, int RemotePort)
        {
            this.LocalHost = LocalHost;
            this.LocalPort = LocalPort;
            this.RemoteHost = RemoteHost;
            this.RemotePort = RemotePort;

            try
            {
                Socket = new StreamSocket();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Initiali Failed:" + ex.Message);
                RaiseOnException(this, ex);
            }
        }
        #endregion 构造函数

        #region 方法
        public void Connect()
        {
            if (Socket != null)
            {
                try
                {
                    BindService();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Connect Failed:" + ex.Message);
                    RaiseOnException(this, ex);
                }
            }
        }

        private async void BindService()
        {
            if (Socket != null)
            {
                try
                {
                    if (LocalHost == null)
                    {
                        await Socket.ConnectAsync(RemoteHost, RemotePort.ToString());
                    }
                    else
                    {
                        EndpointPair Pair = new EndpointPair(LocalHost, LocalPort.ToString(), RemoteHost, RemotePort.ToString());
                        await Socket.ConnectAsync(Pair);
                    }
                    this.IsRunning = true;
                    RaiseConnected(RemoteHost, RemotePort.ToString());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Connect Failed:" + ex.Message);
                    this.IsRunning = false;
                    RaiseDisconnected(RemoteHost, RemotePort.ToString());
                    RaiseOnException(this, ex);
                    Dispose();
                    return;
                }

                try
                {
                    DataReader = new DataReader(Socket.InputStream);
                    DataReader.InputStreamOptions = InputStreamOptions.Partial;
                    uint ReadBufferLength = 8192;
                    while (IsRunning)
                    {
                        UInt32 bytesRead = await DataReader.LoadAsync(ReadBufferLength).AsTask(CancellationTokenSource.Token);
                        if (bytesRead > 0)
                        {
                            byte[] buffer = new byte[bytesRead];
                            DataReader.ReadBytes(buffer);
                            RaiseDataReceived(RemoteHost, RemotePort.ToString(), Convert.ToInt32(bytesRead), buffer);
                        }
                        else //主动断开
                        {
                            DataReader.DetachStream();
                            RaiseDisconnected(RemoteHost, RemotePort.ToString());
                            Dispose();
                            return;
                        }
                    }
                }
                catch(TaskCanceledException tce)
                {
                    //Task Cancelled 
                    //Doing Nothing
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Read stream error:" + ex.Message);
                    RaiseOnException(this, ex);
                }
                finally
                {
                    Dispose();
                }

            }
        }
        private void CancelRead()
        {
            CancellationTokenSource.Cancel(true);
        }

        public override void Dispose()
        {
            try
            {
                IsRunning = false;
                CancelRead();
                //Confusing here:
                //When Close Connection. An exception will throw here calling "The operation identifier is not valid." StackTrace shows Detach methord.
                //But if putting a breakpoint here, nothing exception occured.. weird.
                //At last, just simply commented here because using CancellationTokenSource, the socket will automatically closed as part of cancelling the read operation.
                // DataReader?.DetachStream();  
                DataReader?.Dispose();
                DataReader = null;
                DataWriter?.DetachStream();
                DataWriter?.Dispose();
                DataWriter = null;
                Socket?.Dispose();
                Socket = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Client Dispose Error:" + ex.Message);
                //throw;
            }
           
        }

        public async void Send(byte[] buffer)
        {
            try
            {
                if (Socket != null)
                {
                    DataWriter = new DataWriter(Socket.OutputStream);
                    await WriteAsync(buffer);
                }
            }
            catch (Exception ex)
            {
                RaiseOnException(this, ex);
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
            if (IsRunning)
            {
                Task<UInt32> storeAsyncTask;
                if (buffer.Length != 0)
                {
                    DataWriter.WriteBytes(buffer);
                    storeAsyncTask = DataWriter.StoreAsync().AsTask();
                    UInt32 bytesWritten = await storeAsyncTask;
                    return Convert.ToInt32(bytesWritten);
                }
            }
            return -1;
        }

        #endregion 方法

        #region 事件
        public event TypedEventHandler<TcpClient, TcpClientDataReceivedArgs> DataReceived;
        public event TypedEventHandler<TcpClient, TcpClientConnectionStateArgs> Connected;
        public event TypedEventHandler<TcpClient, TcpClientConnectionStateArgs> Disconnected;

        private void RaiseDataReceived(HostName RemoteHost, string RemotePort, int BytesToRead, byte[] ReceivedBuffer)
        {
            DataReceived?.Invoke(this, new TcpClientDataReceivedArgs(RemoteHost, RemotePort, BytesToRead, ReceivedBuffer));
        }

        private void RaiseConnected(HostName RemoteHost, string RemotePort)
        {
            Connected?.Invoke(this, new TcpClientConnectionStateArgs(RemoteHost, RemotePort));
        }

        private void RaiseDisconnected(HostName RemoteHost, string RemotePort)
        {
            Disconnected?.Invoke(this, new TcpClientConnectionStateArgs(RemoteHost, RemotePort));
        }
        #endregion 事件
    }

    public class TcpClientConnectionStateArgs
    {
        public HostName RemoteHost { get; set; }
        public string RemotePort { get; set; }
        public TcpClientConnectionStateArgs(HostName RemoteHost, string RemotePort)
        {
            this.RemoteHost = RemoteHost;
            this.RemotePort = RemotePort;
        }
    }

    public class TcpClientDataReceivedArgs
    {
        public HostName RemoteHost { get; set; }
        public string RemotePort { get; set; }
        public int BytesToRead { get; set; }
        public byte[] ReceivedBuffer { get; set; }

        public TcpClientDataReceivedArgs(HostName RemoteHost, string RemotePort, int BytesToRead, byte[] ReceivedBuffer)
        {
            this.RemoteHost = RemoteHost;
            this.RemotePort = RemotePort;
            this.BytesToRead = BytesToRead;
            this.ReceivedBuffer = ReceivedBuffer;
        }
    }
}

