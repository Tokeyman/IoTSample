using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using GLibrary.Device.Net;
using GLibrary.Device.UART;
using Newtonsoft.Json;
using DataModelStandard.MessageModel;
using Windows.Networking;
using System.Diagnostics;
using Windows.UI.Xaml;
using System.Windows.Input;
using System.Threading;

namespace MarkClient.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public string Copyright = "Designed by GuanShiyang";
        public string Github = "https://github.com/Tokeyman/IoTSample/";
        public string Power = "Powered by Win10 IoT Core & RPi3b";


        private string _TimingCount;
        public string TimingCount { get { return _TimingCount; } set { Set(ref _TimingCount, value); } }

        private string _RepeatCount;
        public string RepeatCount { get { return _RepeatCount; } set { Set(ref _RepeatCount, value); } }

        private string _RemoteIp = "192.168.0.5";
        public string RemoteIp { get { return _RemoteIp; } set { Set(ref _RemoteIp, value); } }

        private int _RemotePort = 21890;
        public int RemotePort { get { return _RemotePort; } set { Set(ref _RemotePort, value); } }

        private string _InfoMessage = "";
        public string InfoMessage { get { return _InfoMessage; } set { Set(ref _InfoMessage, value); } }

        private string Guid = "0010";

        private JsonSerializerSettings jSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        private TcpClient Socket;
        private MarkClinet Client;
        private SerialPort Sp;

        public MainViewModel()
        {
            GoCommand = new RelayCommand(Go);
            // DispatcherHelper.CheckBeginInvokeOnUI(Go);
            Go();
        }

        public ICommand GoCommand { get; set; }

        private void Go()
        {
            Socket = new TcpClient(new HostName(RemoteIp), RemotePort);
            Socket.DataReceived += Socket_DataReceived;
            Socket.OnException += Socket_OnException;
            Socket.Connected += Socket_Connected;
            Socket.Disconnected += Socket_Disconnected;
            Socket.Connect();
        }

        private DispatcherTimer ReconnectTimer;

        private void Socket_Disconnected(TcpClient sender, TcpClientConnectionStateArgs args)
        {
            if(Client!=null) Client.Dispose();
            if(Sp!=null) Sp.Dispose();
            WriteLineInfo("Socket Disconnected.");
            WriteLineInfo("Reconnecting in 5 seconds.");
            ReconnectTimer = new DispatcherTimer();
            ReconnectTimer.Interval = TimeSpan.FromSeconds(5);
            ReconnectTimer.Tick += ReconnectTimer_Tick;
            ReconnectTimer.Start();

        }
        private void Socket_OnException(object sender, GLibrary.Device.ExceptionArgs args)
        {
            WriteLineInfo("Socket Error");
            WriteLineInfo("Reconnecting in 5 seconds.");
            ReconnectTimer = new DispatcherTimer();
            ReconnectTimer.Interval = TimeSpan.FromSeconds(5);
            ReconnectTimer.Tick += ReconnectTimer_Tick;
            ReconnectTimer.Start();
        }

        private void ReconnectTimer_Tick(object sender, object e)
        {
            Socket.Connect();
            ReconnectTimer.Stop();
        }

        private void Socket_Connected(TcpClient sender, TcpClientConnectionStateArgs args)
        {
            //TODO: Test RPi3b need to uncomment next two lines.
            //Sp = new SerialPort("UART0", 115200);
            //Sp.DataReceived += Sp_DataReceived;

            WriteLineInfo("Socket Connected.");
            Client = new MarkClinet(this.Guid);
            Client.SendToServer += Client_SendToServer;
            Client.SendToUart += Client_SendToUart;
            Client.Go();
        }

        private void Client_SendToUart(object sender, ClientSendToUartArgs e)
        {
            if (Sp != null) Sp.Send(e.Buffer);
            var str = "";
            for (int i = 0; i < e.Buffer.Length; i++)
            {
                str += e.Buffer[i].ToString("X2") + " ";
            }

            WriteLineInfo("SEND TO UART:" + str);
        }

        private void Client_SendToServer(object sender, ClientSendToServerArgs e)
        {
            var message = JsonConvert.SerializeObject(e.Message, jSettings);
            var buffer = System.Text.Encoding.UTF8.GetBytes(message);
            if(Socket!=null)
            {
                if(Socket.IsRunning)
                {
                    Socket.Send(DataConverter.Pack(buffer));
                }
            }
        }

        private void Socket_DataReceived(TcpClient sender, TcpClientDataReceivedArgs args)
        {
            var data = DataConverter.Unpack(args.ReceivedBuffer);
            foreach (var item in data)
            {
                var json = System.Text.Encoding.UTF8.GetString(item);
                var message = JsonConvert.DeserializeObject<MarkMessage>(json, jSettings);
                Client.Process(message);
                if((string)message[PropertyString.Action]==ActionType.Update)
                {
                    TimingCount ="TimingCommand Count:"+ Client.WorkFlow.TimingCommand.Count.ToString();
                    RepeatCount = "RepeatCommand Count:" + Client.WorkFlow.RepeatCommand.Count.ToString();
                }
            }
        }

        private void Sp_DataReceived(SerialPort sender, SerialDataReceivedArgs args)
        {
            if(Client!=null)
            {
                Client.Upload(args.ReceivedBuffer);
            }
        }

        #region Info Message 处理程序

        private void WriteLineInfo(string Info)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (InfoMessage.Split('\n').Count() > 5) { InfoMessage = ""; }
                InfoMessage += Info + "\n";
            });
        }

        #endregion   Info Message 处理程序
    }
}
