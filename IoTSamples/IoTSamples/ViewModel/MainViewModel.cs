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

namespace IoTSamples.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private INavigationService navigationService;

        public MainViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            ShowMain = new RelayCommand(GotoMain);
            ShowGpio = new RelayCommand(GotoGpio);
            ShowSerial = new RelayCommand(GotoSerial);
            ShowUdp = new RelayCommand(GotoUdp);
            ShowTcp = new RelayCommand(GotoTcp);


        }

        public ICommand ShowMain { get; set; }
        public ICommand ShowGpio { get; set; }
        public ICommand ShowSerial { get; set; }
        public ICommand ShowUdp { get; set; }
        public ICommand ShowTcp { get; set; }

        #region Navigation Button
        private void GotoMain() => navigationService.NavigateTo(NavigationString.MainPage);
        private void GotoGpio() => navigationService.NavigateTo(NavigationString.Gpio);
        private void GotoSerial() => navigationService.NavigateTo(NavigationString.SerialPort);
        private void GotoUdp() => navigationService.NavigateTo(NavigationString.Udp);
        private void GotoTcp() => navigationService.NavigateTo(NavigationString.Tcp);

        #endregion Navigation Button


    }
}
