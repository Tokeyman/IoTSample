using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace GLibrary.Device.Net
{
    public class TcpServer:CommonBase
    {
        private StreamSocketListener Socket;

        #region 属性

        /// <summary>
        /// 是否处于运行状态
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 客户端列表
        /// </summary>
        public ObservableCollection<TcpClient> ClientList { get; private set; }

        public HostName LocalHost { get; private set; }

        /// <summary>
        /// 监听端口号
        /// </summary>
        public int Port { get; private set; }

        private CancellationTokenSource CancellationTokenSource = new CancellationTokenSource(); 
        #endregion 属性

        #region 构造函数

        public TcpServer(int Port)
        {
            this.Port = Port;
            ClientList = new ObservableCollection<TcpClient>();
            Socket = new StreamSocketListener();
        }

        public TcpServer(HostName LocalHost,int Port)
        {
            this.LocalHost = LocalHost;
            this.Port = Port;
            ClientList = new ObservableCollection<TcpClient>();
            Socket = new StreamSocketListener();
        }

        #endregion 构造函数

        #region 方法
        public async void Listen()
        {
            if (Socket != null)
            {
                try
                {
                    Socket.ConnectionReceived += Listener_ConnectionReceived;
                    await BindService();
                    IsRunning = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Server listen failed:" + ex.Message);
                    RaiseOnException(this, ex);
                    Dispose();
                }
            }
        }

        private async Task BindService()
        {
            if (LocalHost != null)
            {
                await Socket.BindServiceNameAsync(Port.ToString());
            }
            else
            {
                await Socket.BindEndpointAsync(LocalHost, Port.ToString());
            }
           
        }

        public async override void Dispose()
        {
            this.IsRunning = false;
            lock (this.ClientList)
            {
                foreach (var item in ClientList)
                {
                    item.Dispose();
                }
                ClientList.Clear();
            }

            Socket.ConnectionReceived -= Listener_ConnectionReceived;
            CancelRead();
            await Socket?.CancelIOAsync();
            Socket?.Dispose();
            Socket = null;
        }

        private void CancelRead()
        {
            CancellationTokenSource.Cancel();
        }

        public void Send(byte[] buffer, TcpClient Client)
        {
            if (IsRunning)
            {
                if (ClientList.Any(a => a == Client))
                {
                    Client.Send(buffer);
                }
                else
                {
                    Debug.WriteLine("Connected ClientList does not contains Client.");
                }
            }
        }

        #endregion 方法

        #region 事件


        public event TypedEventHandler<TcpServer, TcpServerConnectionStateArgs> ClientConnected;
        private void RaiseClientConnect(TcpClient Client)
        {
            ClientConnected?.Invoke(this, new TcpServerConnectionStateArgs(Client));
        }

        public event TypedEventHandler<TcpServer, TcpServerConnectionStateArgs> ClientDisconnected;
        private void RaiseClientDisconnect(TcpClient Client)
        {
            ClientDisconnected?.Invoke(this, new TcpServerConnectionStateArgs(Client));
        }

        public event TypedEventHandler<TcpServer, TcpServerDataReceivedArgs> DataReceived;
        private void RaiseDataReceived(HostName RemoteHost, string RemotePort, int BytesToRead, byte[] ReceivedBuffer)
        {
            DataReceived?.Invoke(this, new TcpServerDataReceivedArgs(RemoteHost, RemotePort, BytesToRead, ReceivedBuffer));
        }

        #endregion 事件

        #region 事件处理
        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            TcpClient client = new TcpClient(args.Socket);
            ClientList.Add(client);
            RaiseClientConnect(client);

            DataReader dataReader = new DataReader(args.Socket.InputStream);
            dataReader.InputStreamOptions = InputStreamOptions.Partial;
            uint ReadBufferLength = 8192;
            try
            {
                while (IsRunning)
                {
                    UInt32 bytesRead = await dataReader.LoadAsync(ReadBufferLength).AsTask(CancellationTokenSource.Token);
                    if (bytesRead > 0)
                    {
                        // Sencond way to read bytes from a stream buffer.
                        byte[] buffer = new byte[bytesRead];
                        dataReader.ReadBytes(buffer);
                        RaiseDataReceived(args.Socket.Information.RemoteAddress, args.Socket.Information.RemotePort, Convert.ToInt32(bytesRead), buffer);
                    }
                    else //如果接收为0，表示客户端断开连接 Client Disconnected!
                    {
                        HostName remoteHost = args.Socket.Information.RemoteAddress;
                        int remotePort = Convert.ToInt32(args.Socket.Information.RemotePort);
                        var c = ClientList.FirstOrDefault(f => f.RemoteHost == remoteHost && f.RemotePort == remotePort);
                        if (c != null)
                        {
                            ClientList.Remove(c);
                        }
                        RaiseClientDisconnect(new TcpClient(args.Socket));
                        dataReader.DetachStream();
                        dataReader.Dispose();
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
                var result = SocketError.GetStatus(ex.HResult);
                if(result==SocketErrorStatus.ConnectionResetByPeer)  //远程StreamSocket客户端断开连接
                {
                    HostName remoteHost = args.Socket.Information.RemoteAddress;
                    int remotePort = Convert.ToInt32(args.Socket.Information.RemotePort);
                    var c = ClientList.FirstOrDefault(f => f.RemoteHost == remoteHost && f.RemotePort == remotePort);
                    if (c != null)
                    {
                        ClientList.Remove(c);
                    }
                    RaiseClientDisconnect(new TcpClient(args.Socket));
                    dataReader.DetachStream();
                    dataReader.Dispose();
                    return;
                }
                dataReader.DetachStream();
                dataReader.Dispose();
                Debug.WriteLine("Server read stream error:" + ex.Message);
                RaiseOnException(this, ex);
            }
        }

        #endregion 事件处理
    }


    public class TcpServerConnectionStateArgs
    {
        public TcpClient Client { get; set; }
        public TcpServerConnectionStateArgs(TcpClient Client)
        {
            this.Client = Client;
        }
    }

    public class TcpServerDataReceivedArgs
    {
        public HostName RemoteHost { get; set; }
        public string RemotePort { get; set; }
        public int BytesToRead { get; set; }
        public byte[] ReceivedBuffer { get; set; }
        public TcpServerDataReceivedArgs(HostName RemoteHost, string RemotePort, int BytesToRead, byte[] ReceivedBuffer)
        {
            this.RemoteHost = RemoteHost;
            this.RemotePort = RemotePort;
            this.BytesToRead = BytesToRead;
            this.ReceivedBuffer = ReceivedBuffer;
        }
    }
}
