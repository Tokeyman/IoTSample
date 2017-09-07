using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace GLibrary.Device.Net
{
    public class UdpServer:CommonBase
    {
        #region 私有属性
        private DatagramSocket Socket { get; set; }
        #endregion 私有属性

        public bool IsRunning { get; private set; }

        public HostName LocalHost { get; private set; } 
        public int Port { get; private set; }
      

        #region 构造函数
        public UdpServer(int Port)
        {
            this.Port = Port;
            Socket = new DatagramSocket();
        }

        public UdpServer(HostName LocalHost,int LocalPort)
        {
            this.LocalHost = LocalHost;
            this.Port = LocalPort;
            Socket = new DatagramSocket();
        }
        #endregion 构造函数

        #region 方法
        public void Listen()
        {
            if (Socket != null)
            {
                try
                {
                    Socket.MessageReceived += Socket_MessageReceived;
                    BindService(Port.ToString());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Server listen failed:" + ex.Message);
                    RaiseOnException(this, ex);
                }
                
            }
        }

        private async void BindService(string ServiceName)
        {
            if(LocalHost!=null)
            {
                await Socket.BindEndpointAsync(LocalHost, Port.ToString());
            }
            else
            {
                await Socket.BindServiceNameAsync(ServiceName);
            }
            
        }


        public override void Dispose()
        {
            if (Socket != null)
            {
                Socket.MessageReceived -= Socket_MessageReceived;
                Socket.Dispose();
                Socket = null;
            }
        }


        public async void Send(string RemoteIP, string PortNumber, byte[] Buffer)
        {
            if (Socket != null)
            {
                var OutputStream = await Socket.GetOutputStreamAsync(new HostName(RemoteIP), PortNumber);
                DataWriter writer = new DataWriter(OutputStream);
                writer.WriteBytes(Buffer);
                await writer.StoreAsync();
                writer.DetachStream();
                writer.Dispose();
            }
        }

        #endregion 方法

        #region 事件
        public event TypedEventHandler<UdpServer, UdpServerEventArgs> DataReceived;

        #endregion

        #region 事件触发
        private void RaiseDataReceived(HostName RemoteHost, string RemotePort, int BytesToRead, byte[] ReceivedBuffer)
        {
            DataReceived?.Invoke(this, new UdpServerEventArgs(RemoteHost, RemotePort, BytesToRead, ReceivedBuffer));
        }
        #endregion  

        #region 事件响应

        private void Socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            var reader = args.GetDataReader();  //Get DataReader
            var buffer = reader.DetachBuffer().ToArray();
            HostName RemoteHost = args.RemoteAddress;
            RaiseDataReceived(args.RemoteAddress, args.RemotePort, buffer.Length, buffer);
            reader.DetachStream();
            reader.Dispose();
        }


        #endregion 事件响应
    }
    public class UdpServerEventArgs
    {
        public HostName RemoteHost { get; set; }
        public string RemotePort { get; set; }
        public int BytesToRead { get; set; }
        public byte[] ReceivedBuffer { get; set; }

        public UdpServerEventArgs(HostName RemoteHost, string RemotePort, int BytesToRead, byte[] ReceivedBuffer)
        {
            this.RemoteHost = RemoteHost;
            this.RemotePort = RemotePort;
            this.BytesToRead = BytesToRead;
            this.ReceivedBuffer = ReceivedBuffer;
        }
    }
}
