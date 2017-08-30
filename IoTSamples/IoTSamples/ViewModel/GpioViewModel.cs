using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using System.Windows.Input;
using GLibrary.Device.GPIO;
using Windows.Devices.Gpio;


namespace IoTSamples.ViewModel
{
    public class GpioViewModel:ViewModelBase
    {
        private INavigationService navigationService;
        public GpioViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            ShowMain = new RelayCommand(GotoMain);
            ShowGpio = new RelayCommand(GotoGpio);
            ShowSerial = new RelayCommand(GotoSerial);
            ShowUdp = new RelayCommand(GotoUdp);
            ShowTcp = new RelayCommand(GotoTcp);

            OpenCommand = new RelayCommand(Open);
            SetHighCommand = new RelayCommand(SetHigh);
            SetLowCommand = new RelayCommand(SetLow);
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

        private GPIO Io { get; set; }

        private string _OpenButtonText = "Open";
        public string OpenButtonText { get { return _OpenButtonText; } set { Set(ref _OpenButtonText, value); } }

        private string _PinNumber;
        public string PinNumber { get { return _PinNumber; } set { Set(ref _PinNumber, value); } }

        public ICommand OpenCommand { get; set; }
        public ICommand SetHighCommand { get; set; }
        public ICommand SetLowCommand { get; set; }

        private void Open()
        {
            if (this.OpenButtonText == "Open")
            {
                int pinNumber = Convert.ToInt32(PinNumber);
                Io = new GPIO(pinNumber);
                Io.SetDriveMode(GpioPinDriveMode.Output);
                Io.Write(GpioPinValue.Low);
                this.OpenButtonText = "Close";
            }
            else
            {
                Io.SetDriveMode(GpioPinDriveMode.Input);
                Io.Dispose();
                Io = null;
                this.OpenButtonText = "Open";
            }
        }

        private void SetHigh() => Io?.Write(GpioPinValue.High);
        private void SetLow() => Io?.Write(GpioPinValue.Low);
    }
}
