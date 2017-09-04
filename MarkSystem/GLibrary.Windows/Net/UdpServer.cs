using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GLibrary.Windows.Net
{
    public class UdpServer : CommonBase
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
        /// 本地端口
        /// </summary>
        public int Port { get; set; }


        #endregion

        #region 构造函数
        /// <summary>
        /// UDP服务端
        /// </summary>
        /// <param name="LocalIPAddress">本地IP地址</param>
        /// <param name="Port">本地端口</param>
        public UdpServer(IPAddress LocalIPAddress, int Port)
        {
            this.LocalAddress = LocalIPAddress;
            this.Port = Port;
            try
            {
                udp = new System.Net.Sockets.UdpClient(new IPEndPoint(this.LocalAddress, this.Port));
            }
            catch (Exception ex)
            {
                RaiseOnException(this,ex);
            }
        }

        #endregion

        #region 方法
        /// <summary>
        /// 开始侦听
        /// </summary>
        public void Start()
        {
            if (!IsRunning)
            {
                try
                {
                    udp.BeginReceive(new AsyncCallback(DataReceivedCallBack), udp);
                    IsRunning = true;
                }
                catch (Exception ex)
                {
                    IsRunning = false;
                    RaiseOnException(this,ex);
                }

            }
        }
        /// <summary>
        /// 停止侦听
        /// </summary>
        public override void Dispose()
        {
            if (IsRunning)
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
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="Buffer">要发送的数据</param>
        /// <param name="RemoteEP">远程节点</param>
        public void Send(byte[] Buffer, IPEndPoint RemoteEP)
        {
            if (IsRunning)
            {
                try
                {
                    udp.BeginSend(Buffer, Buffer.Length, RemoteEP, new AsyncCallback(UdpServerSendCallBack), udp);
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
        /// 接收到数据
        /// </summary>
        public event EventHandler<UdpServerDataReceivedArgs> DataReceived;

        #endregion

        #region 触发事件处理

        private void RaiseDataReceived(IPEndPoint iPEndPoint,byte[] ReceivedBuffer)
        {
            DataReceived?.Invoke(this, new UdpServerDataReceivedArgs(iPEndPoint, ReceivedBuffer));
        }
        #endregion

        //异步处理
        private void DataReceivedCallBack(IAsyncResult iar)
        {
            if (IsRunning)
            {
                System.Net.Sockets.UdpClient udp = (System.Net.Sockets.UdpClient)iar.AsyncState;
                IPEndPoint iEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] buffer = udp.EndReceive(iar, ref iEP);
                //触发接收事件
                RaiseDataReceived(iEP, buffer);
                udp.BeginReceive(new AsyncCallback(DataReceivedCallBack), udp);
            }
        }

        private void UdpServerSendCallBack(IAsyncResult iar)
        {
            if (IsRunning)
            {
                System.Net.Sockets.UdpClient udp = (System.Net.Sockets.UdpClient)iar.AsyncState;
                udp.EndSend(iar);
            }
        }
    }


    public class UdpServerDataReceivedArgs
    {
        public IPAddress RemoteHost { get; private set; }
        public int RemotePort { get; private set; }
        public int BytesToRead { get; private set; }
        public byte[] ReceivedBuffer { get; private set; }

        public UdpServerDataReceivedArgs(IPEndPoint iPEndPoint, byte[] ReceivedBuffer) : this(iPEndPoint.Address, iPEndPoint.Port, ReceivedBuffer.Length, ReceivedBuffer) { }

        public UdpServerDataReceivedArgs(IPAddress RemoteHost, int RemotePort, int BytesToRead, byte[] ReceivedBuffer)
        {
            this.RemoteHost = RemoteHost;
            this.RemotePort = RemotePort;
            this.BytesToRead = BytesToRead;
            this.ReceivedBuffer = ReceivedBuffer;
        }

    }
}
