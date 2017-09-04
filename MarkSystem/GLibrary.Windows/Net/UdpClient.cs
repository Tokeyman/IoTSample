using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GLibrary.Windows.Net
{
    /// <summary>
    /// UDP客户端
    /// </summary>
    public class UdpClient:CommonBase
    {
        #region 私有属性
        private System.Net.Sockets.UdpClient udp;
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
        /// 本地IP端口
        /// </summary>
        public int LocalPort { get; set; }
        /// <summary>
        /// 远程IP地址
        /// </summary>
        public IPAddress RemoteAddress { get; private set; }
        /// <summary>
        /// 远程IP端口
        /// </summary>
        public int RemotePort { get; set; }


        #endregion

        #region 构造函数
        /// <summary>
        /// UDP客户端
        /// </summary>
        /// <param name="LocalAddress">本地IP地址</param>
        /// <param name="LocalPort">本地IP端口</param>
        /// <param name="RemoteAddress">远程IP地址</param>
        /// <param name="RemotePort">远程IP端口</param>
        public UdpClient(IPAddress LocalAddress, int LocalPort, IPAddress RemoteAddress, int RemotePort)
        {
            this.LocalAddress = LocalAddress;
            this.LocalPort = LocalPort;
            this.RemoteAddress = RemoteAddress;
            this.RemotePort = RemotePort;
            try
            {
                udp = new System.Net.Sockets.UdpClient(new IPEndPoint(LocalAddress, LocalPort));
            }
            catch (Exception ex)
            {
                RaiseOnException(this,ex);
            }

        }

        /// <summary>
        /// UDP客户端 - 匿名发送数据 不绑定本地IP地址和端口
        /// </summary>
        /// <param name="RemoteAddress">远程IP地址</param>
        /// <param name="RemotePort">远程IP端口</param>
        public UdpClient(IPAddress RemoteAddress, int RemotePort)
        {
            this.RemoteAddress = RemoteAddress;
            this.RemotePort = RemotePort;
            try
            {
                udp = new System.Net.Sockets.UdpClient(new IPEndPoint(IPAddress.Any, 0));
            }
            catch (Exception ex)
            {
                RaiseOnException(this,ex);
            }
        }
        #endregion

        #region 方法
        /// <summary>
        /// 连接到远程服务器
        /// </summary>
        public void Connect()
        {
            if (!IsRunning)
            {
                try
                {
                    IsRunning = true;
                    udp.Connect(new IPEndPoint(RemoteAddress, RemotePort));
                    udp.BeginReceive(new AsyncCallback(DataReceivedCallBack), udp);
                }
                catch (Exception ex)
                {
                    IsRunning = false;
                    RaiseOnException(this,ex);
                }
            }
        }
        /// <summary>
        /// 从服务器断开连接
        /// </summary>
        public override void Dispose()
        {
            try
            {
                IsRunning = false;
                udp?.Close();
                udp?.Dispose();
                udp = null;
            }
            catch (Exception ex)
            {
                IsRunning = false;
                RaiseOnException(this,ex);
            }

        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="buffer">待发送的数据</param>
        public void Send(byte[] buffer)
        {
            if (IsRunning)
            {
                try
                {
                    udp.BeginSend(buffer, buffer.Length, new AsyncCallback(UdpClientSendCallBack), udp);
                }
                catch (Exception ex)
                {
                    RaiseOnException(this,ex);
                }

            }
        }


        #endregion

        #region 事件
        /// <summary>
        /// 接收到数据事件
        /// </summary>
        public event EventHandler<UdpClientDataReceivedArgs> DataReceived;

        #endregion

        #region 触发事件

        private void RaiseDataReceived(IPEndPoint iPEndPoint,byte[] ReceivedBuffer)
        {
            DataReceived?.Invoke(this, new UdpClientDataReceivedArgs(iPEndPoint, ReceivedBuffer));
        }

        #endregion

        #region 异步处理
        private void DataReceivedCallBack(IAsyncResult iar)
        {
            if (IsRunning)
            {
                try  //异步断开会报错 Try一下 错误无需处理即可
                {
                    System.Net.Sockets.UdpClient udp = (System.Net.Sockets.UdpClient)iar.AsyncState;
                    IPEndPoint iEP = new IPEndPoint(RemoteAddress, RemotePort);
                    byte[] buffer = udp.EndReceive(iar, ref iEP);
                    RaiseDataReceived(iEP, buffer);
                    udp.BeginReceive(new AsyncCallback(DataReceivedCallBack), udp);
                }
                catch (Exception)
                {
                }


            }
        }

        private void UdpClientSendCallBack(IAsyncResult iar)
        {
            if (IsRunning)
            {
                System.Net.Sockets.UdpClient udp = (System.Net.Sockets.UdpClient)iar.AsyncState;
                udp.EndSend(iar);
            }
        }
        #endregion
    }

    public class UdpClientDataReceivedArgs
    {
        public IPAddress RemoteHost { get; private set; }
        public int RemotePort { get; private set; }
        public int BytesToRead { get; private set; }
        public byte[] ReceivedBuffer { get; private set; }

        public UdpClientDataReceivedArgs(IPEndPoint iPEndPoint,byte[] ReceivedBuffer)
        {
            this.RemoteHost = iPEndPoint.Address;
            this.RemotePort = iPEndPoint.Port;
            this.BytesToRead = ReceivedBuffer.Length;
            this.ReceivedBuffer = ReceivedBuffer;
        }
    }
}
