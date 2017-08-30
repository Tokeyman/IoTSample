using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using IoTSamples.View;

namespace IoTSamples.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<GpioViewModel>();
            SimpleIoc.Default.Register<SerialPortViewModel>();
            SimpleIoc.Default.Register<UdpViewModel>();
            SimpleIoc.Default.Register<TcpViewModel>();

            var navigationService = this.CreateNavigationService();
            SimpleIoc.Default.Register<INavigationService>(() => navigationService);
        }


        private INavigationService CreateNavigationService()
        {
            var navigationService = new NavigationService();
            navigationService.Configure(NavigationString.MainPage, typeof(MainPage));
            navigationService.Configure(NavigationString.Gpio, typeof(GpioPage));
            navigationService.Configure(NavigationString.SerialPort, typeof(SerialPortPage));
            navigationService.Configure(NavigationString.Udp, typeof(UdpPage));
            navigationService.Configure(NavigationString.Tcp, typeof(TcpPage));

            return navigationService;
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public GpioViewModel Gpio => ServiceLocator.Current.GetInstance<GpioViewModel>();
        public SerialPortViewModel SerialPort => ServiceLocator.Current.GetInstance<SerialPortViewModel>();
        public UdpViewModel Udp => ServiceLocator.Current.GetInstance<UdpViewModel>();
        public TcpViewModel Tcp => ServiceLocator.Current.GetInstance<TcpViewModel>();
    }
}
