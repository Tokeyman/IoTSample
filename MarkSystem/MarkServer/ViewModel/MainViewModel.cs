using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using System.Windows.Input;
using GLibrary.Windows.Net;
using System.Net;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MarkServer.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}

            ClientConnectCommand = new RelayCommand(ClientConnect);
            ClientClearCommand = new RelayCommand(ClientClear);
            ClientSendCommand = new RelayCommand(ClientSend);

            ServerListenCommand = new RelayCommand(ServerListen);
            ServerClearCommand = new RelayCommand(ServerClear);
            ServerSendCommand = new RelayCommand(ServerSend);
        }


        #region Client Side

        private string _ClientRemoteIP = "127.0.0.1";
        public string ClientRemoteIP { get { return _ClientRemoteIP; } set { Set(ref _ClientRemoteIP, value); } }

        private string _ClientRemotePort = "21680";
        public string ClientRemotePort { get { return _ClientRemotePort; } set { Set(ref _ClientRemotePort, value); } }

        private string _ClientLocalIP = "127.0.0.1";
        public string ClientLocalIP { get { return _ClientLocalIP; } set { Set(ref _ClientLocalIP, value); } }

        private string _ClientLocalPort = "21680";
        public string ClientLocalPort { get { return _ClientLocalPort; } set { Set(ref _ClientLocalPort, value); } }

        private string _ClientConnectButtonText = "Connect";
        public string ClientConnectButtonText { get { return _ClientConnectButtonText; } set { Set(ref _ClientConnectButtonText, value); } }

        private string _ClientReceivedMessage = "";
        public string ClientReceivedMessage { get { return _ClientReceivedMessage; } set { Set(ref _ClientReceivedMessage, value); } }

        private string _ClientSendMessage = "";
        public string ClientSendMessage { get { return _ClientSendMessage; } set { Set(ref _ClientSendMessage, value); } }

        public ICommand ClientConnectCommand { get; set; }
        public ICommand ClientClearCommand { get; set; }
        public ICommand ClientSendCommand { get; set; }

        private UdpClient Client;

        private void ClientConnect()
        {
            if (ClientConnectButtonText == "Connect")
            {
                IPAddress remoteHost;
                IPAddress.TryParse(ClientRemoteIP, out remoteHost);
                var port = Convert.ToInt32(ClientRemotePort);

                if (!string.IsNullOrWhiteSpace(ClientLocalIP))
                {
                    IPAddress localHost;
                    IPAddress.TryParse(ClientLocalIP, out localHost);
                    var localport = Convert.ToInt32(ClientLocalPort);
                    Client = new UdpClient(localHost, localport, remoteHost, port);
                }
                else
                {
                    Client = new UdpClient(remoteHost, port);
                }


                Client.DataReceived += Client_DataReceived;
                Client.Connect();
                this.ClientConnectButtonText = "Close";
            }
            else
            {
                Client.Dispose();
                Client = null;
                this.ClientConnectButtonText = "Connect";
            }
        }

        private void Client_DataReceived(object sender, UdpClientDataReceivedArgs e)
        {
            var message = System.Text.Encoding.UTF8.GetString(e.ReceivedBuffer);
            message = e.RemoteHost.ToString() + ":" + e.RemotePort.ToString() + ":" + message + "\n";
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ClientReceivedMessage += message;
            });
            // throw new NotImplementedException();
        }

        private void ClientClear()
        {
            this.ClientReceivedMessage = ""; this.ClientSendMessage = "";
        }

        private void ClientSend()
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(ClientSendMessage);
            if ((Client != null) && (Client.IsRunning))
            {
                Client.Send(buffer);
            }
        }

        #endregion

        #region Server Side

        private string _ServerLocalIP = "127.0.0.1";
        public string ServerLocalIP { get { return _ServerLocalIP; } set { Set(ref _ServerLocalIP, value); } }

        private string _ServerLocalPort = "21690";
        public string ServerLocalPort { get { return _ServerLocalPort; } set { Set(ref _ServerLocalPort, value); } }

        private ObservableCollection<ComboBoxItem> _ClientList = new ObservableCollection<ComboBoxItem>();
        public ObservableCollection<ComboBoxItem> ClientList { get { return _ClientList; } set { Set(ref _ClientList, value); } }

        private string _SelectedValue;
        public string SelectedValue { get { return _SelectedValue; } set { Set(ref _SelectedValue, value); } }

        private string _ServerListenButtonText = "Listen";
        public string ServerListenButtonText { get { return _ServerListenButtonText; } set { Set(ref _ServerListenButtonText, value); } }

        private string _ServerReceivedMessage = "";
        public string ServerReceivedMessage { get { return _ServerReceivedMessage; } set { Set(ref _ServerReceivedMessage, value); } }

        private string _ServerSendMessage = "";
        public string ServerSendMessage { get { return _ServerSendMessage; } set { Set(ref _ServerSendMessage, value); } }

        public ICommand ServerListenCommand { get; set; }
        public ICommand ServerClearCommand { get; set; }
        public ICommand ServerSendCommand { get; set; }

        private UdpServer Server;

        private void ServerListen()
        {
            if(ServerListenButtonText=="Listen")
            {
                IPAddress localHost;
                IPAddress.TryParse(ServerLocalIP, out localHost);
                var port = Convert.ToInt32(ServerLocalPort);
                Server = new UdpServer(port);
                Server.DataReceived += Server_DataReceived;
                Server.Start();
                this.ServerListenButtonText = "Stop";
            }
            else
            {
                Server.Dispose();
                Server = null;
                this.ServerListenButtonText = "Listen";
            }
        }

        private void Server_DataReceived(object sender, UdpServerDataReceivedArgs e)
        {
            // throw new NotImplementedException();
            var client = e.RemoteHost.ToString() + ":" + e.RemotePort.ToString();
            var message = System.Text.Encoding.UTF8.GetString(e.ReceivedBuffer);
            message = client + ":" + message + "\n";
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                var c = ClientList.FirstOrDefault(f => f.Value == client);
                if(c==null)
                {
                    ClientList.Add(new ComboBoxItem(client, client));
                    SelectedValue = client;
                }

                ServerReceivedMessage += message;
            });
        }

        private void ServerClear()
        {
            this.ServerReceivedMessage = "";
            this.ServerSendMessage = "";
        }

        private void ServerSend()
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(ServerSendMessage);
            if(Server!=null)
            {
                string[] paras = SelectedValue.Split(':');
                IPAddress remoteHost;
                IPAddress.TryParse(paras[0], out remoteHost);
                var remotePort = Convert.ToInt32(paras[1]);
                IPEndPoint iPEndPoint = new IPEndPoint(remoteHost, remotePort);
                Server.Send(buffer, iPEndPoint);
            }
        }
        #endregion
    }
}