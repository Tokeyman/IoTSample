using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GLibrary.Windows.Net
{
    /// <summary>
    /// TcpClient类
    /// </summary>
    public class TcpClient : CommonBase
    {
        #region 私有属性
        private System.Net.Sockets.TcpClient tcpClient;
        //private NetworkStream ns;
        private const int ReceiveTimeOut = 10;
        #endregion

        #region 属性
        /// <summary>
        /// 是否运行
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
        /// 远程IP地址
        /// </summary>
        public IPAddress RemoteAddress { get; private set; }
        /// <summary>
        /// 远程端口
        /// </summary>
        public int RemotePort { get; private set; }
        #endregion

        #region 构造函数

        internal TcpClient(System.Net.Sockets.TcpClient TcpClient)
        {
            IsRunning = true;
            this.tcpClient = TcpClient;
            this.LocalAddress = ((IPEndPoint)TcpClient.Client.LocalEndPoint).Address;
            this.LocalPort = ((IPEndPoint)TcpClient.Client.LocalEndPoint).Port;
            this.RemoteAddress = ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Address;
            this.RemotePort = ((IPEndPoint)TcpClient.Client.RemoteEndPoint).Port;
        }
        /// <summary>
        /// 为TCP网络服务提供客户端连接
        /// </summary>
        /// <param name="LocalAddress">本地IP地址</param>
        /// <param name="LocalPort">本地端口</param>
        /// <param name="RemoteAddress">远程IP地址</param>
        /// <param name="RemotePort">远程端口</param>
        public TcpClient(IPAddress LocalAddress, int LocalPort, IPAddress RemoteAddress, int RemotePort)
        {
            this.LocalAddress = LocalAddress;
            this.LocalPort = LocalPort;
            this.RemoteAddress = RemoteAddress;
            this.RemotePort = RemotePort;
        }


        #endregion

        #region 方法
        /// <summary>
        /// 连接到服务端
        /// </summary>
        public void Connect()
        {
            try
            {
                IPEndPoint localEP = new IPEndPoint(LocalAddress, LocalPort);
                tcpClient = new System.Net.Sockets.TcpClient(localEP);
                tcpClient.ReceiveTimeout = ReceiveTimeOut;
                tcpClient.BeginConnect(RemoteAddress, RemotePort, new AsyncCallback(ConnectedCallBack), tcpClient);
            }
            catch (Exception ex)
            {
                RaiseOnException(this, ex);
            }


        }
        /// <summary>
        /// 从服务端断开连接
        /// </summary>
        public override void Dispose()
        {
            try
            {
                if ((tcpClient != null) && (tcpClient.Connected))
                {
                    IsRunning = false;
                    tcpClient?.Close();
                    RaiseDisconnected();
                    tcpClient?.Dispose();
                    tcpClient = null;
                }
            }
            catch (Exception ex)
            {
                RaiseOnException(this,ex);
            }
        }

        /// <summary>
        /// 发送数据到服务端
        /// </summary>
        /// <param name="buffer">要发送的数据</param>
        public void Send(byte[] buffer)
        {
            if (IsRunning)
            {
                NetworkStream ns = tcpClient.GetStream();
                if (ns.CanWrite)
                {
                    ns.BeginWrite(buffer, 0, buffer.Length, new AsyncCallback(StreamWriteCallBack), ns);
                }
            }
        }
        #endregion

        #region 异步处理
        private void ConnectedCallBack(IAsyncResult iar)
        {
            System.Net.Sockets.TcpClient tcpClient = (System.Net.Sockets.TcpClient)iar.AsyncState;
            try
            {
                tcpClient.EndConnect(iar);
                if ((tcpClient != null) && (tcpClient.Connected))
                {
                    IsRunning = true;
                    RaiseConnected();
                    NetworkStream ns = tcpClient.GetStream();
                    ClientDataState state = new ClientDataState(tcpClient);

                    if (ns.CanRead)
                    {
                        byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
                        ns.BeginRead(state.ReceivedBuffer, 0, tcpClient.ReceiveBufferSize, new AsyncCallback(AsyncReadCallBack), state);
                    }
                }
                else
                {
                    IsRunning = false;
                    RaiseDisconnected();
                }
            }
            catch (Exception ex)
            {
                RaiseOnException(this, ex);
            }



        }

        private void AsyncReadCallBack(IAsyncResult iar)
        {
            ClientDataState dataState = (ClientDataState)iar.AsyncState;
            if ((dataState.Client == null) || (!dataState.Client.Connected)) return;
            int NumOfBytesRead;
            NetworkStream ns = dataState.Client.GetStream();
            NumOfBytesRead = ns.EndRead(iar);
            if (NumOfBytesRead > 0)  //读取到数据
            {
                byte[] buffer = new byte[NumOfBytesRead];
                Array.Copy(dataState.ReceivedBuffer, 0, buffer, 0, NumOfBytesRead);
               // TcpClientState state = new TcpClientState(dataState.Client, NumOfBytesRead, dataState.ReceivedBuffer, (IPEndPoint)dataState.Client.Client.RemoteEndPoint);
               // RaiseDataReceived(state);
                RaiseDataReceived((IPEndPoint)dataState.Client.Client.RemoteEndPoint, dataState.ReceivedBuffer);
                ns.BeginRead(dataState.ReceivedBuffer, 0, dataState.Client.ReceiveBufferSize, new AsyncCallback(AsyncReadCallBack), dataState);
            }
            else  //连接被服务端断开
            {
                IsRunning = false;
                RaiseDisconnected();
            }
        }

        private void StreamWriteCallBack(IAsyncResult iar)
        {
            NetworkStream ns = (NetworkStream)iar.AsyncState;
            ns.EndWrite(iar);
        }
        #endregion


        #region 事件
        /// <summary>
        /// 接收到数据
        /// </summary>
        public event EventHandler<TcpClientDataReceivedArgs> DataReceived;
        /// <summary>
        /// 连接到服务端
        /// </summary>
        public event EventHandler Connected;
        /// <summary>
        /// 从服务端断开
        /// </summary>
        public event EventHandler Disconected;

        #endregion

        #region 触发事件

        private void RaiseDataReceived(IPEndPoint iPEndPoint, byte[] ReceivedBuffer)
        {
            DataReceived?.Invoke(this, new TcpClientDataReceivedArgs(iPEndPoint, ReceivedBuffer));
        }
        private void RaiseConnected() => Connected?.Invoke(this, EventArgs.Empty);

        private void RaiseDisconnected() => Disconected?.Invoke(this, EventArgs.Empty);
        #endregion
    }

    public class TcpClientDataReceivedArgs
    {
        public IPAddress RemoteHost { get; private set; }
        public int RemotePort { get; private set; }
        public int BytesToRead { get; private set; }
        public byte[] ReceivedBuffer { get; private set; }

        public TcpClientDataReceivedArgs(IPEndPoint iPEndPoint, byte[] ReceivedBuffer)
        {
            this.RemoteHost = iPEndPoint.Address;
            this.RemotePort = iPEndPoint.Port;
            this.BytesToRead = ReceivedBuffer.Length;
            this.ReceivedBuffer = ReceivedBuffer;
        }
    }

    /// <summary>
    /// 异步传递数据用类
    /// </summary>
    internal class ClientDataState
    {
        public System.Net.Sockets.TcpClient Client { get; set; }
        public byte[] ReceivedBuffer { get; set; }
        public int BytesToRead { get; set; }

        public ClientDataState(System.Net.Sockets.TcpClient Client)
        {
            this.Client = Client;
            this.ReceivedBuffer = new byte[Client.ReceiveBufferSize];
            this.BytesToRead = 0;
        }
    }
}
