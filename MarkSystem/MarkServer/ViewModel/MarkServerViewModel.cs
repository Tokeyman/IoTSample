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
using DataModel.MessageModel;
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


        private TcpServer Server;

        public MarkServerViewModel()
        {
            if (IsInDesignMode)
            {

            }
            else
            {
                Listen();
            }
        }

        private void Listen()
        {
            Server = new TcpServer(21890);
            Server.ClientConnected += Server_ClientConnected;
            Server.ClientDisconnected += Server_ClientDisconnected;
            Server.DataReceived += Server_DataReceived;
            Server.Start();

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
