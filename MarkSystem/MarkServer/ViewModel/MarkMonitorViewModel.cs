using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight.Threading;
using System.Collections.ObjectModel;
using System.Timers;
using GLibrary.Windows.Net;
using System.Net;
using DataModelStandard.MessageModel;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Windows.Input;

namespace MarkServer.ViewModel
{
    public class MarkMonitorViewModel:ViewModelBase
    {
        private ObservableCollection<ClientItem> _ClientList = new ObservableCollection<ClientItem>();
        public ObservableCollection<ClientItem> ClientList { get { return _ClientList; } set { Set(ref _ClientList, value); } }

        private ObservableCollection<string> _ConnectedList = new ObservableCollection<string>();
        public ObservableCollection<string> ConnectedList { get { return _ConnectedList; } set { Set(ref _ConnectedList, value); } }

        private JsonSerializerSettings jSetting = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
        private Timer GeneralTimer;
        private UdpClient Socket;

        private string _ServerIp="192.168.0.5";
        public string ServerIp { get { return _ServerIp; } set { Set(ref _ServerIp, value); } }


        public MarkMonitorViewModel()
        {
            if (IsInDesignMode) { }

            GoCommand = new RelayCommand(Go);
        }

        public ICommand GoCommand { get; set; }

        private void Go()
        {
            Socket = new UdpClient(IPAddress.Parse(ServerIp), 8896);
            Socket.DataReceived += Socket_DataReceived;
            Socket.Connect();

            GeneralTimer = new Timer(2000);
            GeneralTimer.AutoReset = true;
            GeneralTimer.Elapsed += GeneralTimer_Elapsed;
            GeneralTimer.Start();
        }

        private void Socket_DataReceived(object sender, UdpClientDataReceivedArgs e)
        {
            try
            {
                var json = System.Text.Encoding.UTF8.GetString(e.ReceivedBuffer);
                var message = JsonConvert.DeserializeObject<MarkMessage>(json, jSetting);
                var cc = (List<ConnectedClient>)message["ConnectedClients"];
                var rc = (List<RegisterdClient>)message["RegisterdClients"];
                DispatcherHelper.CheckBeginInvokeOnUI(()=> {
                    ClientList.Clear();
                    foreach (var item in rc)
                    {
                        ClientList.Add(new ClientItem(item.Ip, item.Port.ToString(), item.Guid));
                    }
                    ConnectedList.Clear();
                    foreach (var item in cc)
                    {
                        ConnectedList.Add(item.Ip + ":" + item.Port.ToString());
                    }

                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Gui Socket received error.");
            }
        }

        private void GeneralTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
           if(Socket!=null)
            {
                var message = new MarkMessage() ;
                message[PropertyString.Action] = "GetClients";
                var json = JsonConvert.SerializeObject(message, jSetting);
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                Socket.Send(buffer);
            }
        }
    }
}
