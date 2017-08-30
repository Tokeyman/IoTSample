using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;

namespace GLibrary.Device.Net
{
    public class UdpClient:CommonBase
    {
        #region 私有属性
        private DatagramSocket Socket { get; set; }
        #endregion

        #region 属性
        public int LocalPort { get; set; }
        public HostName LocalHost { get; set; }

        public int RemotePort { get; set; }
        public HostName RemoteHost { get; set; }
        #endregion

        #region 构造函数

        /// <summary>
        /// 匿名发送 无需绑定本地IP和端口
        /// </summary>
        /// <param name="RemoteHost"></param>
        /// <param name="RemotePort"></param>
        public UdpClient(HostName RemoteHost, int RemotePort)
        {
            this.RemoteHost = RemoteHost;
            this.RemotePort = RemotePort;
            try
            {
                Socket = new DatagramSocket();
            }
            catch (Exception)
            {

                throw;
            }
        }



        /// <summary>
        /// 标准发送 绑定本地IP和端口
        /// </summary>
        /// <param name="LocalHost">null为绑定所有IP，或者指定本地IP</param>
        /// <param name="LocalPort"></param>
        /// <param name="RemoteHost"></param>
        /// <param name="RemotePort"></param>
        public UdpClient(HostName LocalHost, int LocalPort, HostName RemoteHost, int RemotePort)
        {
            this.LocalHost = LocalHost;
            this.LocalPort = LocalPort;
            this.RemoteHost = RemoteHost;
            this.RemotePort = RemotePort;

            try
            {
                Socket = new DatagramSocket();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region 方法
        public void Connect()
        {
            if (Socket != null)
            {
                try
                {
                    Socket.MessageReceived += Socket_MessageReceived;
                    BindService();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Client conenct failed:" + ex.Message);
                    RaiseOnException(this, ex);
                }
               
            }
        }

        public override void Dispose()
        {
            if (Socket != null)
            {
                Socket.Dispose();
                Socket = null;
            }
        }


        public async void Send(byte[] Buffer)
        {
            if (Socket != null)
            {
                var OutputStream = await Socket.GetOutputStreamAsync(this.RemoteHost, this.RemotePort.ToString());
                DataWriter writer = new DataWriter(OutputStream);
                writer.WriteBytes(Buffer);
                await writer.StoreAsync();
                writer.DetachStream();
                writer.Dispose();
            }
        }

        #endregion

        #region 事件
        public event TypedEventHandler<UdpClient, UdpClientEventArgs> DataReceived;
        #endregion


        #region 事件触发
        private void RaiseDataReceived(HostName RemoteHost, string RemotePort, int BytesToRead, byte[] ReceivedBuffer)
        {
            DataReceived?.Invoke(this, new UdpClientEventArgs(RemoteHost, RemotePort, BytesToRead, ReceivedBuffer));
        }
        #endregion  

        #region 私有方法
        private async void BindService()
        {
            if (Socket != null)
            {
                if (LocalHost == null)  //未指定本地IP  使用所有IP
                {
                    await Socket.BindServiceNameAsync(this.LocalPort.ToString());
                }
                else //指定本地IP
                {
                    await Socket.BindEndpointAsync(LocalHost, this.LocalPort.ToString());
                }
                await Socket.ConnectAsync(this.RemoteHost, this.RemotePort.ToString());
            }
        }

        private void Socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            var reader = args.GetDataReader();  //Get DataReader
            var buffer = reader.DetachBuffer().ToArray();
            HostName RemoteHost = args.RemoteAddress;
            RaiseDataReceived(args.RemoteAddress, args.RemotePort, buffer.Length, buffer);
            reader.DetachStream();
            reader.Dispose();
        }
        #endregion
    }



    public class UdpClientEventArgs
    {
        public HostName RemoteHost { get; set; }
        public string RemotePort { get; set; }
        public int BytesToRead { get; set; }
        public byte[] ReceivedBuffer { get; set; }

        public UdpClientEventArgs(HostName RemoteHost, string RemotePort, int BytesToRead, byte[] ReceivedBuffer)
        {
            this.RemoteHost = RemoteHost;
            this.RemotePort = RemotePort;
            this.BytesToRead = BytesToRead;
            this.ReceivedBuffer = ReceivedBuffer;
        }
    }
}
