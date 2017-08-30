using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using Microsoft.Practices.ServiceLocation;
using GalaSoft.MvvmLight.Threading;
using GLibrary.Device.Net;
using Windows.Networking;
using System.Collections.ObjectModel;

namespace IoTSamples.ViewModel
{
    public class UdpViewModel : ViewModelBase
    {
        private INavigationService navigationService;
        public UdpViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            ShowMain = new RelayCommand(GotoMain);
            ShowGpio = new RelayCommand(GotoGpio);
            ShowSerial = new RelayCommand(GotoSerial);
            ShowUdp = new RelayCommand(GotoUdp);
            ShowTcp = new RelayCommand(GotoTcp);

            ClientConnectCommand = new RelayCommand(ClientConnect);
            ClientClearCommand = new RelayCommand(ClientClear);
            ClientSendCommand = new RelayCommand(ClientSend);

            ServerListenCommand = new RelayCommand(ServerListen);
            ServerClearCommand = new RelayCommand(ServerClear);
            ServerSendCommand = new RelayCommand(ServerSend);
        }
        #region Navigation Button
        public ICommand ShowMain { get; set; }
        public ICommand ShowGpio { get; set; }
        public ICommand ShowSerial { get; set; }
        public ICommand ShowUdp { get; set; }
        public ICommand ShowTcp { get; set; }


        private void GotoMain() => navigationService.NavigateTo(NavigationString.MainPage);
        private void GotoGpio() => navigationService.NavigateTo(NavigationString.Gpio);
        private void GotoSerial() => navigationService.NavigateTo(NavigationString.SerialPort);
        private void GotoUdp() => navigationService.NavigateTo(NavigationString.Udp);
        private void GotoTcp() => navigationService.NavigateTo(NavigationString.Tcp);

        #endregion Navigation Button


        #region ClientSide
        private UdpClient Client;

        private string _ClientLocalIP = "127.0.0.1";
        public string ClientLocalIP { get { return _ClientLocalIP; } set { Set(ref _ClientLocalIP, value); } }

        private string _ClientLocalPort = "21679";
        public string ClientLocalPort { get { return _ClientLocalPort; } set { Set(ref _ClientLocalPort, value); } }

        private string _ClientRemoteIP = "127.0.0.1";
        public string ClientRemoteIP { get { return _ClientRemoteIP; } set { Set(ref _ClientRemoteIP, value); } }

        private string _ClientRemotePort = "21680";
        public string ClientRemotePort { get { return _ClientRemotePort; } set { Set(ref _ClientRemotePort, value); } }

        private string _ClientConnectButtonText = "Connect";
        public string ClientConnectButtonText { get { return _ClientConnectButtonText; } set { Set(ref _ClientConnectButtonText, value); } }

        private string _ClientReceivedMessage = "";
        public string ClientReceivedMessage { get { return _ClientReceivedMessage; } set { Set(ref _ClientReceivedMessage, value); } }

        private string _ClientSendMessage = "";
        public string ClientSendMessage { get { return _ClientSendMessage; } set { Set(ref _ClientSendMessage, value); } }

        public ICommand ClientConnectCommand { get; set; }
        public ICommand ClientClearCommand { get; set; }
        public ICommand ClientSendCommand { get; set; }

        private void ClientConnect()
        {
            if (ClientConnectButtonText == "Connect")
            {
                var remoteHost = new HostName(ClientRemoteIP);
                var remotePort = Convert.ToInt32(ClientRemotePort);

                if (string.IsNullOrWhiteSpace(ClientLocalIP))
                {
                    Client = new UdpClient(remoteHost, remotePort);
                }
                else
                {
                    var localHost = new HostName(ClientLocalIP);
                    var localPort = Convert.ToInt32(ClientLocalPort);
                    Client = new UdpClient(localHost, localPort, remoteHost, remotePort);
                }
                Client.DataReceived += Client_DataReceived;
                Client.Connect();
                this.ClientConnectButtonText = "Close";
            }
            else
            {
                Client.DataReceived -= Client_DataReceived;
                Client.Dispose();
                this.ClientConnectButtonText = "Connect";
            }
        }

        private void Client_DataReceived(UdpClient sender, UdpClientEventArgs args)
        {
            var message = System.Text.Encoding.UTF8.GetString(args.ReceivedBuffer);
            message = args.RemoteHost.DisplayName + ":" + args.RemotePort + ":" + message + "\n";
            DispatcherHelper.CheckBeginInvokeOnUI(() => { ClientReceivedMessage += message; });
        }

        private void ClientClear()
        {
            this.ClientReceivedMessage = "";
            this.ClientSendMessage = "";
        }

        private void ClientSend()
        {
            if (Client != null)
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(ClientSendMessage);
                Client.Send(buffer);
            }
        }
        #endregion ClientSide

        #region ServerSide
        private UdpServer Server;

        private string _ServerListenPort = "21680";
        public string ServerListenPort { get { return _ServerListenPort; } set { Set(ref _ServerListenPort, value); } }

        private ObservableCollection<ComboBoxItem> _ClientList;
        public ObservableCollection<ComboBoxItem> ClientList { get { return _ClientList; } set { Set(ref _ClientList, value); } }

        private string _ServerSelectValue;
        public string ServerSelectValue { get { return _ServerSelectValue; } set { Set(ref _ServerSelectValue, value); } }

        private string _ServerListenButtonText = "Listen";
        public string ServerListenButtonText { get { return _ServerListenButtonText; } set { Set(ref _ServerListenButtonText, value); } }

        private string _ServerReceivedMessage = "Listen";
        public string ServerReceivedMessage { get { return _ServerReceivedMessage; } set { Set(ref _ServerReceivedMessage, value); } }

        private string _ServerSendMessage = "Listen";
        public string ServerSendMessage { get { return _ServerSendMessage; } set { Set(ref _ServerSendMessage, value); } }

        public ICommand ServerListenCommand { get; set; }
        public ICommand ServerClearCommand { get; set; }
        public ICommand ServerSendCommand { get; set; }

        private void ServerListen()
        {
            if(ServerListenButtonText=="Listen")
            {
                var listenPort = Convert.ToInt32(ServerListenPort);
                Server = new UdpServer(listenPort);
                Server.DataReceived += Server_DataReceived;
                ClientList = new ObservableCollection<ComboBoxItem>();
                Server.Listen();
                ServerListenButtonText = "Stop";
            }
            else
            {
                Server.DataReceived -= Server_DataReceived;
                Server.Dispose();
                ServerListenButtonText = "Listen";
            }
        }

        private void Server_DataReceived(UdpServer sender, UdpServerEventArgs args)
        {
            var client = args.RemoteHost.DisplayName + ":" + args.RemotePort;
            var message = System.Text.Encoding.UTF8.GetString(args.ReceivedBuffer);
            message = args.RemoteHost.DisplayName + ":" + args.RemotePort + ":" + message + "\n";
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                ServerReceivedMessage += message;
                ClientList.Add(new ComboBoxItem(client, client));
                ServerSelectValue = client;
            });
        }

        private void ServerClear()
        {
            this.ServerReceivedMessage = "";
            this.ServerSendMessage = "";
            ClientList.Clear();
            ServerSelectValue = string.Empty;
        }

        private void ServerSend()
        {
            if(Server!=null)
            {
                if(!string.IsNullOrWhiteSpace(ServerSelectValue))
                {
                    string[] paras = ServerSelectValue.Split(':');
                    var buffer = System.Text.Encoding.UTF8.GetBytes(ServerSendMessage);
                    Server.Send(paras[0], paras[1], buffer);
                }
            }

        }
        #endregion
    }
}
