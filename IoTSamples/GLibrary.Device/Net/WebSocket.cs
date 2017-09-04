using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Web;
using Windows.Foundation;

namespace GLibrary.Device.Net
{
    //Un Tested
    public class WebSocket:CommonBase
    {
        #region 属性
        private MessageWebSocket Socket;

        public Uri ServerUri { get; private set; }
        #endregion 属性

        #region 构造函数
        public WebSocket(Uri ServerUri)
        {
            this.ServerUri = ServerUri;
        }

        #endregion 构造函数

        #region 方法
        public async void Open()
        {
            Socket = new MessageWebSocket();
            Socket.Control.MessageType = SocketMessageType.Utf8;
            Socket.MessageReceived += Socket_MessageReceived;
            Socket.Closed += Socket_Closed;
            try
            {
                await Socket.ConnectAsync(ServerUri);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Message Socket Open Error:" + ex.Message);
            }
        }

        public async void Send(byte[] buffer)
        {
            if(Socket!=null)
            {
                try
                {
                    DataWriter dataWriter = new DataWriter(Socket.OutputStream);
                    dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    dataWriter.WriteBytes(buffer);
                    //dataWriter.WriteString("1234");
                    await dataWriter.StoreAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Send Error:" + ex.Message);
                }

            }
        }

        public override void Dispose()
        {
            Socket?.Dispose();
        }


        private void Socket_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            RaiseClosed(args);
        }

        private void Socket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            try
            {
                DataReader dataReader=args.GetDataReader();
                dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                var buffer = dataReader.DetachBuffer().ToArray();
                //string str = dataReader.ReadString(dataReader.UnconsumedBufferLength);


               RaiseMessageReceived(buffer);
            }
            catch (Exception ex)
            {
                RaiseOnException(this, ex);
                Debug.WriteLine("Receive message error：" + ex.Message);
            }
            //throw new NotImplementedException();

        }
        
        #endregion

        #region 事件
        public event TypedEventHandler<WebSocket, WebSocketDataReceivedArgs> MessageReceived;
        public event TypedEventHandler<WebSocket, WebSocketClosedArgs> Closed;

        #endregion 事件

        #region 事件触发
        private void RaiseMessageReceived(byte[] ReceivedBuffer)
        {
            MessageReceived?.Invoke(this, new WebSocketDataReceivedArgs(ReceivedBuffer));
        }

        private void RaiseClosed(WebSocketClosedEventArgs args)
        {
            Closed?.Invoke(this, new WebSocketClosedArgs(args));
        }


        #endregion  事件触发
    }

    public class WebSocketDataReceivedArgs
    {
        public int BytesToRead { get; set; }
        public byte[] ReceivedBuffer { get; set; }
        public WebSocketDataReceivedArgs(int BytesToRead, byte[] ReceivedBuffer)
        {
            this.BytesToRead = BytesToRead;
            this.ReceivedBuffer = ReceivedBuffer;
        }

        public WebSocketDataReceivedArgs(byte[] ReceivedBuffer) : this(ReceivedBuffer.Length, ReceivedBuffer) { }
    }

    public class WebSocketClosedArgs
    {
        public ushort Code { get; set; }
        public string Reason { get; set; }
        public WebSocketClosedArgs(WebSocketClosedEventArgs args)
        {
            this.Code = args.Code;
            this.Reason = args.Reason;
        }
    }
}
