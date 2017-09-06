using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GLibrary.Windows.Net
{
    public class TcpServer : CommonBase
    {
        #region 私有属性
        private TcpListener tcpListener;
        #endregion

        #region 属性
        /// <summary>
        /// 运行状态
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 本地IP地址
        /// </summary>
        public IPAddress LocalAddress { get; private set; }
        /// <summary>
        /// 本地端口
        /// </summary>
        public int LocalPort { get; private set; }
        /// <summary>
        /// 已连接的客户端列表
        /// </summary>
        public List<TcpClient> ClientList { get; private set; }
        #endregion

        #region 构造函数
        /// <summary>
        /// TCP网络服务端
        /// </summary>
        /// <param name="LocalAddress">本地IP地址</param>
        /// <param name="LocalPort">本地端口</param>
        public TcpServer(IPAddress LocalAddress, int LocalPort)
        {
            this.LocalAddress = LocalAddress;
            this.LocalPort = LocalPort;
            this.ClientList = new List<TcpClient>();
            tcpListener = new TcpListener(new IPEndPoint(LocalAddress, LocalPort));
        }

        public TcpServer(int LocalPort)
        {
            this.LocalAddress = null;
            this.LocalPort = LocalPort;
            this.ClientList = new List<TcpClient>();
            tcpListener = new TcpListener(IPAddress.Any, LocalPort);
        }
        #endregion


        #region 方法
        /// <summary>
        /// 开始侦听
        /// </summary>
        public void Start()
        {
            if ((tcpListener != null) && (!IsRunning))
            {
                try
                {
                    tcpListener.Start();
                    IsRunning = true;
                    tcpListener.BeginAcceptTcpClient(new AsyncCallback(AcceptClientCallBack), tcpListener);
                    ClientList = new List<TcpClient>();
                }
                catch (Exception ex)
                {
                    RaiseOnException(this, ex);
                    IsRunning = false;
                }
            }
        }
        /// <summary>
        /// 停止侦听
        /// </summary>
        public override void Dispose()
        {
            if ((tcpListener != null) && (IsRunning))
            {
                try
                {
                    IsRunning = false;
                    tcpListener.Server.Close();
                    tcpListener.Stop();
                    lock (this.ClientList)
                    {
                        foreach (TcpClient item in ClientList)
                        {
                            item.Dispose();
                        }
                        ClientList.Clear();
                    }

                }
                catch (Exception ex)
                {
                    RaiseOnException(this, ex);
                    IsRunning = false;
                }
            }
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="Buffer">待发送的数据</param>
        /// <param name="Client">目标客户端</param>
        public void Send(byte[] Buffer, TcpClient Client)
        {
            Client.Send(Buffer);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="Buffer">待发送的数据</param>
        /// <param name="iPEndPoint">目标节点</param>
        public void Send(byte[] Buffer, IPEndPoint iPEndPoint)
        {
            var c = ClientList.FirstOrDefault(f => f.RemoteAddress == iPEndPoint.Address && f.RemotePort == iPEndPoint.Port);
            if (c != null)
            {
                Send(Buffer, c);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="Buffer">待发送的数据</param>
        /// <param name="RemoteIp">目标IP</param>
        /// <param name="RemotePort">目标端口</param>
        public void Send(byte[] Buffer, IPAddress RemoteIp, int RemotePort)
        {
            var c = ClientList.FirstOrDefault(f => f.RemoteAddress == RemoteIp && f.RemotePort == RemotePort);
            if (c != null)
            {
                Send(Buffer, c);
            }
        }
        #endregion

        #region 异步处理
        private void AcceptClientCallBack(IAsyncResult iar)
        {
            TcpListener listener = (TcpListener)iar.AsyncState;
            if (IsRunning)
            {
                System.Net.Sockets.TcpClient client = listener.EndAcceptTcpClient(iar);
                ClientList.Add(new TcpClient(client));
                ServerDataState dataState = new ServerDataState(listener, client);
                //TcpServerState state = new TcpServerState(listener, new TcpClient(client), 0, new byte[0], (IPEndPoint)dataState.Client.Client.RemoteEndPoint);
                //RaiseClientConnected(state);
                RaiseClientConnected((IPEndPoint)client.Client.RemoteEndPoint);
                NetworkStream ns = dataState.Client.GetStream();
                if (ns.CanRead)
                {
                    ns.BeginRead(dataState.ReceivedBuffer, 0, dataState.Client.ReceiveBufferSize, new AsyncCallback(ServerDataReceivedCallBack), dataState);
                }
                listener.BeginAcceptTcpClient(new AsyncCallback(AcceptClientCallBack), listener);
            }
        }

        private void ServerDataReceivedCallBack(IAsyncResult iar)
        {
            ServerDataState dataState = (ServerDataState)iar.AsyncState;
            if ((dataState.Client == null) || (!dataState.Client.Connected)) return;
            NetworkStream ns = dataState.Client.GetStream();
            int NumOfBytesRead = ns.EndRead(iar);
            if (NumOfBytesRead > 0) //接收到数据
            {
                byte[] buffer = new byte[NumOfBytesRead];
                Array.Copy(dataState.ReceivedBuffer, buffer, NumOfBytesRead);
                //TcpServerState state = new TcpServerState(dataState.Listener, new TcpClient(dataState.Client), NumOfBytesRead, buffer, (IPEndPoint)dataState.Client.Client.RemoteEndPoint);
                //RaiseDataReceived(state);
                RaiseDataReceived((IPEndPoint)dataState.Client.Client.RemoteEndPoint, buffer);
                ns.BeginRead(dataState.ReceivedBuffer, 0, dataState.Client.ReceiveBufferSize, new AsyncCallback(ServerDataReceivedCallBack), dataState);
            }
            else  //客户端断开连接
            {
                TcpClient client = new TcpClient(dataState.Client);
                ClientList.Remove(ClientList.First(s => s.RemoteAddress == client.RemoteAddress && s.RemotePort == client.RemotePort));
                //TcpServerState state = new TcpServerState(dataState.Listener, new TcpClient(dataState.Client), 0, new byte[0], (IPEndPoint)dataState.Client.Client.RemoteEndPoint);
                //RaiseClientDisconnected(state);
                RaiseClientDisconnected((IPEndPoint)dataState.Client.Client.RemoteEndPoint);
                ns.Close();
                dataState.Client.Close();
                ns = null;
                dataState = null;
            }
        }
        #endregion

        #region 事件
        /// <summary>
        /// 接收到数据事件
        /// </summary>
        public event EventHandler<TcpServerDataReceivedArgs> DataReceived;
        /// <summary>
        /// 客户端连接事件
        /// </summary>
        public event EventHandler<TcpServerClientConnectdArgs> ClientConnected;
        /// <summary>
        /// 客户端断开连接事件
        /// </summary>
        public event EventHandler<TcpServerClientDisconnectdArgs> ClientDisconnected;

        #endregion

        #region 触发事件

        private void RaiseDataReceived(IPEndPoint iPEndPoint, byte[] ReceivedBuffer) => DataReceived?.Invoke(this, new TcpServerDataReceivedArgs(iPEndPoint, ReceivedBuffer));
        private void RaiseClientConnected(IPEndPoint iPEndPoint) => ClientConnected?.Invoke(this, new TcpServerClientConnectdArgs(iPEndPoint));
        private void RaiseClientDisconnected(IPEndPoint iPEndPoint) => ClientDisconnected?.Invoke(this, new TcpServerClientDisconnectdArgs(iPEndPoint));

        #endregion
    }

    public class TcpServerDataReceivedArgs
    {
        public IPAddress RemoteHost { get; private set; }
        public int RemotePort { get; private set; }
        public int BytesToRead { get; private set; }
        public byte[] ReceivedBuffer { get; private set; }

        public TcpServerDataReceivedArgs(IPEndPoint iPEndPoint, byte[] ReceivedBuffer)
        {
            this.RemoteHost = iPEndPoint.Address;
            this.RemotePort = iPEndPoint.Port;
            this.BytesToRead = ReceivedBuffer.Length;
            this.ReceivedBuffer = ReceivedBuffer;
        }
    }

    public class TcpServerClientConnectdArgs
    {
        public IPAddress RemoteHost { get; private set; }
        public int RemotePort { get; private set; }

        public TcpServerClientConnectdArgs(IPEndPoint iPEndPoint)
        {
            this.RemoteHost = iPEndPoint.Address;
            this.RemotePort = iPEndPoint.Port;
        }
    }

    public class TcpServerClientDisconnectdArgs
    {
        public IPAddress RemoteHost { get; private set; }
        public int RemotePort { get; private set; }

        public TcpServerClientDisconnectdArgs(IPEndPoint iPEndPoint)
        {
            this.RemoteHost = iPEndPoint.Address;
            this.RemotePort = iPEndPoint.Port;
        }
    }

    /// <summary>
    /// 异步传递数据用--内部用
    /// </summary>
    internal class ServerDataState
    {
        public System.Net.Sockets.TcpListener Listener { get; set; }
        public System.Net.Sockets.TcpClient Client { get; set; }
        public byte[] ReceivedBuffer { get; set; }
        public int BytesToRead { get; set; }

        public ServerDataState(System.Net.Sockets.TcpListener Listener, System.Net.Sockets.TcpClient Client)
        {
            this.Listener = Listener;
            this.Client = Client;
            this.ReceivedBuffer = new byte[Client.ReceiveBufferSize];
            this.BytesToRead = 0;
        }
    }
}
