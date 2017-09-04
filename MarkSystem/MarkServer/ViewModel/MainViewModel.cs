using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using System.Windows.Input;
using GLibrary.Windows.Net;
using System.Net;
using System;

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
        }


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

                if(!string.IsNullOrWhiteSpace(ClientLocalIP))
                {
                    IPAddress localHost;
                    IPAddress.TryParse(ClientLocalIP, out localHost);
                    var localport = Convert.ToInt32(ClientLocalPort);
                    Client = new UdpClient(localHost,localport,remoteHost,port);
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


    }
}