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

        private void Socket_DataReceived(object sender, TcpServerDataReceivedArgs e)
        {
            var clientIp = e.RemoteHost.ToString();
            var clientPort = e.RemotePort.ToString();
            var client = clientIp + ":" + clientPort;


            var message = System.Text.Encoding.UTF8.GetString(e.ReceivedBuffer);

            var model = JsonConvert.DeserializeObject<MessageModel>(message);

            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ReceivedCommand += client + ":" + model.Command + "\n";
                Server.Upload(model); //交给服务端程序处理

                if (model.Command == CommandString.Register)  //同步客户端--也可以从Server的ClientList中获取
                {
                    var c = ClientList.FirstOrDefault(f => f.IP == clientIp && f.Port == clientPort);
                    if (c == null)
                    {
                        //TODO 增加
                        ClientItem item = new ClientItem(clientIp, clientPort, model.Guid);
                        ClientList.Add(item);

                    }
                }
            });

        }


        /// <summary>
        /// 维护连接客户端列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Socket_ClientDisconnected(object sender, TcpServerClientDisconnectdArgs e)
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

        /// <summary>
        /// 维护连接客户端列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Socket_ClientConnected(object sender, TcpServerClientConnectdArgs e)
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

        private void Server_SendToClient(object sender, ServerSocketSendArgs e)
        {
            if (Socket != null)
            {
                //从当前的连接列表中根据Guid获取 目标的IP和Port
                var c = ClientList.FirstOrDefault(f => f.Guid == e.TargetGuid);
                if (c != null)
                {
                    //序列化内容
                    var json = JsonConvert.SerializeObject(e.Message);
                    var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                    IPAddress remoteHost;
                    IPAddress.TryParse(c.IP, out remoteHost);
                    //从当前的客户端列表中获得IP和Port后发送到目标

                    IPEndPoint iPEndPoint = new IPEndPoint(remoteHost, Convert.ToInt32(c.Port));
                    Socket.Send(buffer, iPEndPoint);
                }
            }
        }

        private void Server_SendToDb(object sender, ServerDbSendArgs e)
        {
            //TODO 连接数据库处理程序
            //目前手动处理提示信息

            var message = e.Guid + ":" + e.Command + "\n";
            SendToDbMessage += message;
            //throw new NotImplementedException();
        }


        #region Testing Methord
        private string TargetGuid = "0001";
        private void Update()
        {
            workFlow = new WorkFlowModel();
            workFlow.AddTimingCommand(TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x10, 0xfe });
            workFlow.AddTimingCommand(TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x11, 0xfe });
            workFlow.AddTimingCommand(TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x12, 0xfe });
            workFlow.AddTimingCommand(TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x13, 0xfe });

            workFlow.AddRepeatCommand(new byte[] { 0x7f, 0xef, 0x30, 0xfe });
            workFlow.AddRepeatCommand(new byte[] { 0x7f, 0xef, 0x31, 0xfe });

            Server.Process(TargetGuid, CommandString.Update, workFlow);
        }
        private void Start() => Server.Process(TargetGuid, CommandString.Start);


        private void Pause() => Server.Process(TargetGuid, CommandString.Pause);
        private void Resume() => Server.Process(TargetGuid, CommandString.Resume);
        private void Stop() => Server.Process(TargetGuid, CommandString.Stop);
        #endregion
    }


}
