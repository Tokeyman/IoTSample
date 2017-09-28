using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using DataModelStandard;
using GLibrary.Windows.Net;
using MarkDbModel;
using DataModelStandard.MessageModel;
using Newtonsoft.Json;
using System.Net;
using System.Timers;

namespace MarkHost
{
    public partial class MarHost : ServiceBase
    {
        public MarHost()
        {
            InitializeComponent();
        }

        //WCF
        //private ServiceHost _host = new ServiceHost(typeof(MarkService));


        private List<ConnectedClient> ConnectedClients;
        private List<RegisterdClient> RegisterdClients;
        private IServerService service;

        private TcpServer Socket;
        private MarkServer Server;

        private JsonSerializerSettings jSetting = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

        private string ServerGuid = "9000";
        private string ServerName = "Server";
        private int LocalPort = 21890;
        private Timer ReadTimer;


        protected override void OnStart(string[] args)
        {
            //WCF
            //_host.Open();

            ConnectedClients = new List<ConnectedClient>();
            RegisterdClients = new List<RegisterdClient>();
            service = new ServerService();

            Server = new MarkServer(ServerGuid, ServerName);
            Server.ClientConnected += Server_ClientConnected;
            Server.ClientRegisterd += Server_ClientRegisterd;
            Server.ClientClosed += Server_ClientClosed;
            Server.SendToDb += Server_SendToDb;
            Server.SendToClient += Server_SendToClient;

            Socket = new TcpServer(LocalPort);
            Socket.ClientConnected += Socket_ClientConnected;
            Socket.ClientDisconnected += Socket_ClientDisconnected;
            Socket.DataReceived += Socket_DataReceived;

            Socket.Start();
            ReadTimer = new Timer(500);
            ReadTimer.AutoReset = true;
            ReadTimer.Elapsed += ReadTimer_Elapsed;
            ReadTimer.Start();
        }



        protected override void OnStop()
        {
            //WCF
            //_host.Close();
            Socket.Dispose();
            Server = null;
        }

        private void Server_ClientConnected(object sender, ClientConnectedArgs e)
        {
            var client = ConnectedClients.FirstOrDefault(f => f.Ip == e.Ip && f.Port == e.Port);
            if (client == null) ConnectedClients.Add(new ConnectedClient(e.Ip, e.Port));
        }
        private void Server_ClientRegisterd(object sender, ClientRegisterdArgs e)
        {
            var client = RegisterdClients.FirstOrDefault(f => f.Ip == e.Ip && f.Port == e.Port && f.Guid == e.Guid);
            if (client == null) RegisterdClients.Add(new RegisterdClient(e.Guid, e.Ip, e.Port));
        }

        private void Server_ClientClosed(object sender, ClientClosedArgs e)
        {
            var c = ConnectedClients.FirstOrDefault(f => f.Ip == e.Ip && f.Port == e.Port);
            if (c != null) ConnectedClients.Remove(c);
            var cc = RegisterdClients.FirstOrDefault(f => f.Ip == e.Ip && f.Port == e.Port);
            if (cc != null) RegisterdClients.Remove(cc);
        }

        private void Server_SendToClient(object sender, ServerSocketSendArgs e)
        {
            if(Socket!=null)
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
            var senderGuid = (string)e.Message[PropertyString.SenderGuid];
            var action = (string)e.Message[PropertyString.Action];
            //TODO 处理数据库Register Push Pull Close 请求 
            //调用IServerService
           
        }

        private void ReadTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //TODO DequeueAction
            throw new NotImplementedException();
        }

        private void Socket_DataReceived(object sender, TcpServerDataReceivedArgs e)
        {
            var json = System.Text.Encoding.UTF8.GetString(e.ReceivedBuffer);
            var model = JsonConvert.DeserializeObject<MarkMessage>(json, jSetting);
            Server.Upload(e.RemoteHost.ToString(), e.RemotePort, model);
        }

        private void Socket_ClientDisconnected(object sender, TcpServerClientDisconnectdArgs e)
        {
            Server.ClientClose(e.RemoteHost.ToString(), e.RemotePort);
        }

        private void Socket_ClientConnected(object sender, TcpServerClientConnectdArgs e)
        {
            Server.ClientConnect(e.RemoteHost.ToString(), e.RemotePort);
        }


        #region 数据库操作

        #endregion
    }

}
