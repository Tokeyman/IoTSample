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


namespace IoTSamples.ViewModel
{
    public class SerialPortViewModel:ViewModelBase
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
            if(this.OpenButtonText=="Open")
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
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
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
            if(sp!=null)
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(SendMessage);
                sp.Send(buffer);
            }
        }

    }
}
