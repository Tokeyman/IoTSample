using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;

namespace MarkClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public string Copyright = "Designed by GuanShiyang";
        public string Github = "https://github.com/Tokeyman/IoTSample/";
        public string Power = "Powered by Win10 IoT Core & RPi3b";


        public MainViewModel()
        {

        }
    }
}
