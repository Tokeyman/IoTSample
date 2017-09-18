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
using System.Net;
using System.Windows.Input;
using MarkDbModel;
using MarkDbModel.Entity;

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

        private string _SendToDbMessage;
        public string SendToDbMessage { get { return _SendToDbMessage; } set { Set(ref _SendToDbMessage, value); } }

        public ICommand UpdateCommand { get; set; }
        public ICommand StartCommand { get; set; }
        public ICommand PauseCommand { get; set; }
        public ICommand ResumeCommand { get; set; }
        public ICommand StopCommand { get; set; }

        private TcpServer Socket;
        private WorkFlow workFlow;

        private DataModelStandard.MessageModel.MarkServer Server;
        private JsonSerializerSettings jSetting = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public MarkServerViewModel()
        {
            if (IsInDesignMode)
            {

            }
            else
            {
                Server = new DataModelStandard.MessageModel.MarkServer("9000", "Server");
                Server.ClientConnected += Server_ClientConnected;
                Server.ClientRegisterd += Server_ClientRegisterd;
                Server.ClientClosed += Server_ClientClosed;
                Server.SendToDb += Server_SendToDb;
                Server.SendToClient += Server_SendToClient;

                Socket = new TcpServer(21890);
                Socket.ClientConnected += Socket_ClientConnected;
                Socket.ClientDisconnected += Socket_ClientDisconnected;
                Socket.DataReceived += Socket_DataReceived;
                Socket.Start();


                #region  UI Command
                UpdateCommand = new RelayCommand(Update);
                StartCommand = new RelayCommand(Start);
                PauseCommand = new RelayCommand(Pause);
                ResumeCommand = new RelayCommand(Resume);
                StopCommand = new RelayCommand(Stop);
                #endregion
            }
        }
        private void Server_ClientConnected(object sender, ClientConnectedArgs e)
        {
            var client = e.Ip + ":" + e.Port.ToString();
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                var c = ConnectedList.FirstOrDefault(f => f == client);
                if (c == null) ConnectedList.Add(client);
            });

        }

        private void Server_ClientRegisterd(object sender, ClientRegisterdArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                var c = ClientList.FirstOrDefault(f => f.Guid == e.Guid);
                if (c == null) ClientList.Add(new ClientItem(e.Ip, e.Port.ToString(), e.Guid));
            });
        }

        private void Server_ClientClosed(object sender, ClientClosedArgs e)
        {
            var client = e.Ip + ":" + e.Port.ToString();
            var clientIp = e.Ip;
            var clientPort = e.Port.ToString();
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                var c = ClientList.FirstOrDefault(f => f.IP == clientIp && f.Port == clientPort);
                if (c != null) ClientList.Remove(c);
                var cc = ConnectedList.FirstOrDefault(f => f == client);
                if (cc != null) ConnectedList.Remove(cc);
            });
        }

        private void Server_SendToClient(object sender, ServerSocketSendArgs e)
        {
            if (Socket != null)
            {
                //序列化内容
                var json = JsonConvert.SerializeObject(e.Message, jSetting);
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(e.RemoteIp), e.RemotePort);
                Socket.Send(buffer, iPEndPoint);
            }

        }

        private void Server_SendToDb(object sender, ServerDbSendArgs e)
        {
            //TODO 连接数据库处理程序
            //目前手动处理提示信息
            var senderGuid = (string)e.Message[PropertyString.SenderGuid];
            var action = (string)e.Message[PropertyString.Action];

            var message = senderGuid + ":" + action + "\n";
            SendToDbMessage += message;



            //处理Register Push Pull和Close请求
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                if (action == ActionType.Pull)
                {
                    //TODO Generate WorkFlow and RaiseToUpdate
                }
                else if (action == ActionType.Push)
                {
                    //TODO Push Data
                }

                db.SaveChanges();
            }
        }


        private void Socket_DataReceived(object sender, TcpServerDataReceivedArgs e)
        {
            var clientIp = e.RemoteHost.ToString();
            var clientPort = e.RemotePort.ToString();
            var client = clientIp + ":" + clientPort;


            var json = System.Text.Encoding.UTF8.GetString(e.ReceivedBuffer);

            var model = JsonConvert.DeserializeObject<MarkMessage>(json, jSetting);
            Server.Upload(e.RemoteHost.ToString(), e.RemotePort, model);
        }


        /// <summary>
        /// 维护连接客户端列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Socket_ClientDisconnected(object sender, TcpServerClientDisconnectdArgs e)
        {
            Server.ClientClose(e.RemoteHost.ToString(), e.RemotePort);
        }

        /// <summary>
        /// 维护连接客户端列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Socket_ClientConnected(object sender, TcpServerClientConnectdArgs e)
        {
            Server.ClientConnect(e.RemoteHost.ToString(), e.RemotePort);
        }

        #region Testing Methord
        private string TargetGuid = "0001";
        private void Update()
        {
            workFlow = new WorkFlow();
            workFlow.AddTimingCommand(TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x10, 0xfe });
            workFlow.AddTimingCommand(TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x11, 0xfe });
            workFlow.AddTimingCommand(TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x12, 0xfe });
            workFlow.AddTimingCommand(TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x13, 0xfe });

            workFlow.AddRepeatCommand(new byte[] { 0x7f, 0xef, 0x30, 0xfe });
            workFlow.AddRepeatCommand(new byte[] { 0x7f, 0xef, 0x31, 0xfe });

            var model = new MarkMessage(null, null, ActionType.Update)
            {
                { PropertyString.TargetGuid, TargetGuid },
                { PropertyString.WorkFlow, workFlow }
            };
            Server.Process(model);
        }

        private void Start()
        {
            var model = new MarkMessage(null, null, ActionType.Start)
            {
                { PropertyString.TargetGuid, TargetGuid }
            };
            Server.Process(model);
        } 

        private void Pause()
        {
            var model = new MarkMessage(null, null, ActionType.Pause)
            {
                { PropertyString.TargetGuid, TargetGuid }
            };
            Server.Process(model);
        }

        private void Resume()
        {
            var model = new MarkMessage(null, null, ActionType.Resume)
            {
                { PropertyString.TargetGuid, TargetGuid }
            };
            Server.Process(model);
        }

        private void Stop()
        {
            var model = new MarkMessage(null, null, ActionType.Stop)
            {
                { PropertyString.TargetGuid, TargetGuid }
            };
            Server.Process(model);
        }
        #endregion
    }
}

