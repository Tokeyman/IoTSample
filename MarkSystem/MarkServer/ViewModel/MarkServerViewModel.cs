using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GLibrary.Windows.Net;
using System.Collections.ObjectModel;
using DataModelStandard.MessageModel;
using Newtonsoft.Json;

namespace MarkServer.ViewModel
{
    public class MarkServerViewModel : ViewModelBase
    {
        private ObservableCollection<ClientItem> _ClientList = new ObservableCollection<ClientItem>();
        public ObservableCollection<ClientItem> ClientList { get { return _ClientList; } set { Set(ref _ClientList, value); } }

        private ObservableCollection<string> _ConnectedList = new ObservableCollection<string>();
        public ObservableCollection<string> ConnectedList { get { return _ConnectedList; } set { Set(ref _ConnectedList, value); } }


        private string _ReceivedCommand;
        public string ReceivedCommand { get { return _ReceivedCommand; } set { Set(ref _ReceivedCommand, value); } }


        private TcpServer Socket;
        private WorkFlowModel workFlow;

        private ServerModel Server;
        public MarkServerViewModel()
        {
            if (IsInDesignMode)
            {

            }
            else
            {
                Server = new ServerModel();

                workFlow = new WorkFlowModel();
                workFlow.AddTimingCommand(TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x10, 0xfe });
                workFlow.AddTimingCommand(TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x11, 0xfe });
                workFlow.AddTimingCommand(TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x12, 0xfe });
                workFlow.AddTimingCommand(TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x13, 0xfe });

                workFlow.AddRepeatCommand(new byte[] { 0x7f, 0xef, 0x30, 0xfe });
                workFlow.AddRepeatCommand(new byte[] { 0x7f, 0xef, 0x31, 0xfe });


                Listen();
            }
        }

        private void Listen()
        {
            Socket = new TcpServer(21890);
            Socket.ClientConnected += Server_ClientConnected;
            Socket.ClientDisconnected += Server_ClientDisconnected;
            Socket.DataReceived += Server_DataReceived;
            Socket.Start();

        }

        private void Server_DataReceived(object sender, TcpServerDataReceivedArgs e)
        {
            var clientIp = e.RemoteHost.ToString();
            var clientPort = e.RemotePort.ToString();
            var client = clientIp + ":" + clientPort;

           
            var message = System.Text.Encoding.UTF8.GetString(e.ReceivedBuffer);

            var model = JsonConvert.DeserializeObject<MessageModel>(message);

            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ReceivedCommand += client + ":" + model.Command + "\n";
                if (model.Command == "Register")  //注册客户端
                {
                    var c = ClientList.FirstOrDefault(f => f.IP == clientIp && f.Port == clientPort);
                    if (c == null)
                    {
                        //TODO 增加
                        ClientItem item = new ClientItem(clientIp, clientPort, model.Guid);
                        ClientList.Add(item);

                    }
                }
                else if (model.Command == "Pull") //Pull Request 获取工作程序
                {
                    var json = JsonConvert.SerializeObject(Server.Update(workFlow));

                    var c = Socket.ClientList.FirstOrDefault(f => f.RemoteAddress == e.RemoteHost && f.RemotePort == e.RemotePort);
                    if(c!=null)
                    {
                        var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                        Socket.Send(buffer, c);
                    }
                }
            });

        }

        private void Server_ClientDisconnected(object sender, TcpServerClientDisconnectdArgs e)
        {
            var clientIp = e.RemoteHost.ToString();
            var clientPort = e.RemotePort.ToString();
            var client = clientIp + ":" + clientPort;

            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (ConnectedList.Contains(client))
                {
                    ConnectedList.Remove(client);
                }
                //检查ClientList中相同项目 
                var c = ClientList.FirstOrDefault(f => f.IP == clientIp && f.Port == clientPort);
                if (clientPort != null)
                {
                    ClientList.Remove(c);
                }

            });
        }

        private void Server_ClientConnected(object sender, TcpServerClientConnectdArgs e)
        {
            var client = e.RemoteHost.ToString() + ":" + e.RemotePort.ToString();
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                if (!ConnectedList.Contains(client))
                {
                    ConnectedList.Add(client);
                }
            });


        }


    }


}
