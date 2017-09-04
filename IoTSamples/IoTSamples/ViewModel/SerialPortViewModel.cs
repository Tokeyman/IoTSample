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
using GLibrary.Device.UART;
using GalaSoft.MvvmLight.Threading;
using GLibrary.Device.Net;


namespace IoTSamples.ViewModel
{
    public class SerialPortViewModel : ViewModelBase
    {
        private INavigationService navigationService;

        public SerialPortViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            ShowMain = new RelayCommand(GotoMain);
            ShowGpio = new RelayCommand(GotoGpio);
            ShowSerial = new RelayCommand(GotoSerial);
            ShowUdp = new RelayCommand(GotoUdp);
            ShowTcp = new RelayCommand(GotoTcp);

            OpenCommand = new RelayCommand(Open);
            ClearCommand = new RelayCommand(Clear);
            SendCommand = new RelayCommand(Send);

            SocketOpenCommand = new RelayCommand(SocketOpen);
            SocketClearCommand = new RelayCommand(SocketClear);
            SocketSendCommand = new RelayCommand(SocketSend);

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


        #region SerialPort


        private SerialPort sp;

        private string _OpenButtonText = "Open";
        public string OpenButtonText { get { return _OpenButtonText; } set { Set(ref _OpenButtonText, value); } }

        private string _Selector = "UART0";
        public string SelectorText { get { return _Selector; } set { Set(ref _Selector, value); } }

        private string _ReceviceMessage;
        public string ReceiveMessage { get { return _ReceviceMessage; } set { Set(ref _ReceviceMessage, value); } }
        private string _SendMessage;
        public string SendMessage { get { return _SendMessage; } set { Set(ref _SendMessage, value); } }

        public ICommand OpenCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand SendCommand { get; set; }

        private void Open()
        {
            if (this.OpenButtonText == "Open")
            {
                sp = new SerialPort(SelectorText, 115200);
                sp.DataReceived += Sp_DataReceived;
                sp.Open();
                this.OpenButtonText = "Close";
            }
            else
            {
                sp.DataReceived -= Sp_DataReceived;
                sp.Dispose();
                this.OpenButtonText = "Open";
            }
        }

        private void Sp_DataReceived(SerialPort sender, SerialDataReceivedArgs args)
        {
            //TODO sp received
            //throw new NotImplementedException();
            string message = System.Text.Encoding.UTF8.GetString(args.ReceivedBuffer);
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ReceiveMessage += message + "\n";
            });
        }

        private void Clear()
        {
            this.ReceiveMessage = "";
            this.SendMessage = "";
        }

        private void Send()
        {
            if (sp != null)
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(SendMessage);
                sp.Send(buffer);
            }
        }


        #endregion


        #region WebSocket

        private string _Uri= "ws://121.40.165.18:8088";
        public string Uri { get { return _Uri; } set { Set(ref _Uri, value); } }

        private string _SocketButtonText = "Open";
        public string SocketButtonText { get { return _SocketButtonText; } set { Set(ref _SocketButtonText, value); } }

        private string _SocketReceivedMessage = "";
        public string SocketReceivedMessage { get { return _SocketReceivedMessage; } set { Set(ref _SocketReceivedMessage, value); } }

        private string _SocketSendMessage = "";
        public string SocketSendMessage { get { return _SocketSendMessage; } set { Set(ref _SocketSendMessage, value); } }

        private WebSocket WebSocket;

        public ICommand SocketOpenCommand { get; set; }
        public ICommand SocketClearCommand { get; set; }
        public ICommand SocketSendCommand { get; set; }


        private void SocketOpen()
        {
            if(SocketButtonText=="Open")
            {
                Uri uri = new Uri(Uri);
                WebSocket = new WebSocket(uri);

                WebSocket.MessageReceived += WebSocket_MessageReceived;
                WebSocket.Open();
                SocketButtonText = "Close";
            }
            else
            {
                WebSocket.Dispose();
                SocketButtonText = "Open";
            }
        }

        private void SocketClear()
        {
            this.SocketReceivedMessage = "";
            this.SocketSendMessage = "";
        }

        private void SocketSend()
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(SocketSendMessage);
            if(WebSocket!=null)
            {
                WebSocket.Send(buffer);
            }
        }
        private void WebSocket_MessageReceived(WebSocket sender, WebSocketDataReceivedArgs args)
        {
            var message = System.Text.Encoding.UTF8.GetString(args.ReceivedBuffer)+"\n";
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                SocketReceivedMessage += message;
            });
        }
        #endregion WebSocket
    }
}
