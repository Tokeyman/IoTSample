using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using GLibrary.Device.Net;
using GLibrary.Device.UART;

using DataModelStandard.MessageModel;
using Windows.Networking;

namespace MarkClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public string Copyright = "Designed by GuanShiyang";
        public string Github = "https://github.com/Tokeyman/IoTSample/";
        public string Power = "Powered by Win10 IoT Core & RPi3b";

        private HostName ServerIp = new HostName("127.0.0.1");
        private int ServerPort = 21890;
        public MainViewModel()
        {

        }

        private TcpClient Socket;
        private MarkClinet Client;
        private string Guid = "0001";

        private void Connect()
        {
            Client = new MarkClinet(this.Guid);
            Socket = new TcpClient(ServerIp, ServerPort);
            Socket.Connected += Socket_Connected;
            Socket.Disconnected += Socket_Disconnected;
            Socket.DataReceived += Socket_DataReceived;
            Socket.OnException += Socket_OnException;
            Socket.Connect();
            
        }



        private void Socket_OnException(object sender, GLibrary.Device.ExceptionArgs args)
        {
            Socket.Dispose();
            Socket.Connected -= Socket_Connected;
            Socket.Disconnected -= Socket_Disconnected;
            Socket.DataReceived -= Socket_DataReceived;
            Socket.OnException -= Socket_OnException;

            Connect();
        }

        private void Socket_DataReceived(TcpClient sender, TcpClientDataReceivedArgs args)
        {
            throw new NotImplementedException();
        }

        private void Socket_Disconnected(TcpClient sender, TcpClientConnectionStateArgs args)
        {
            throw new NotImplementedException();
        }

        private void Socket_Connected(TcpClient sender, TcpClientConnectionStateArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
