using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModelStandard.MessageModel;
using GLibrary.Windows.Net;
using Newtonsoft.Json;
using System.Timers;
using System.Net;

namespace ConsoleServerTest
{
    class Program
    {
        private static List<ConnectedClient> ConnectedClients;
        private static List<RegisterdClient> RegisterdClients;
        private static IServerService service;

        private static TcpServer Socket;
        private static MarkServer Server;

        private static JsonSerializerSettings jSetting = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

        private static string ServerGuid = "9000";
        private static string ServerName = "Server";
        private static int LocalPort = 21890;
        private static Timer ReadTimer;

        #region Guid
        private static UdpServer GuiSocket = new UdpServer(8896);
        #endregion

        static void Main(string[] args)
        {
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


            #region GUI 
            GuiSocket.DataReceived += GuiSocket_DataReceived;
            GuiSocket.Start();

            #endregion

            var cmd = Console.ReadLine();
            while (cmd != "quit")
            {
                cmd = Console.ReadLine();
            }
        }

        private static void Server_ClientConnected(object sender, ClientConnectedArgs e)
        {
            var client = ConnectedClients.FirstOrDefault(f => f.Ip == e.Ip && f.Port == e.Port);
            if (client == null) ConnectedClients.Add(new ConnectedClient(e.Ip, e.Port));
        }

        private static void Server_ClientRegisterd(object sender, ClientRegisterdArgs e)
        {
            var client = RegisterdClients.FirstOrDefault(f => f.Ip == e.Ip && f.Port == e.Port && f.Guid == e.Guid);
            if (client == null) RegisterdClients.Add(new RegisterdClient(e.Guid, e.Ip, e.Port));
            service.Register(e.Guid, e.Ip, e.Port);
        }

        private static void Server_ClientClosed(object sender, ClientClosedArgs e)
        {
            var c = ConnectedClients.FirstOrDefault(f => f.Ip == e.Ip && f.Port == e.Port);
            if (c != null) ConnectedClients.Remove(c);
            var cc = RegisterdClients.FirstOrDefault(f => f.Ip == e.Ip && f.Port == e.Port);
            if (cc != null) RegisterdClients.Remove(cc);
            service.Close(e.Ip, e.Port);
        }

        private static void Server_SendToClient(object sender, ServerSocketSendArgs e)
        {
            if (Socket != null)
            {
                //序列化内容
                var json = JsonConvert.SerializeObject(e.Message, jSetting);
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(e.RemoteIp), e.RemotePort);
                Socket.Send(DataConverter.Pack(buffer), iPEndPoint);
            }
        }

        private static void Server_SendToDb(object sender, ServerDbSendArgs e)
        {
            var senderGuid = (string)e.Message[PropertyString.SenderGuid];
            var action = (string)e.Message[PropertyString.Action];
            //TODO 处理数据库Push Pull 请求 
            //调用IServerService
            if (action == ActionType.Pull)
            {
                var workFlow = service.Pull(senderGuid);
                var message = new MarkMessage();
                message[PropertyString.TargetGuid] = senderGuid;
                message[PropertyString.WorkFlow] = workFlow;
                message[PropertyString.Action] = ActionType.Update;
                Server.Process(message);
            }
            else if (action == ActionType.Push)
            {
                var buffer = Transform.StringToBytes((string)e.Message[PropertyString.Data]);
                var status = (string)e.Message[PropertyString.Status];
                service.Push(senderGuid, buffer, status);
            }
        }

        private static void ReadTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //TODO DequeueAction
            var oa = service.DequeueOperation();
            if (oa != null)
            {


                MarkMessage message;
                switch (oa.Action)
                {
                    case ActionType.Start: message = new MarkMessage(null, null, ActionType.Start) { { PropertyString.TargetGuid, oa.TargetGuid } }; break;
                    case ActionType.Stop: message = new MarkMessage(null, null, ActionType.Stop) { { PropertyString.TargetGuid, oa.TargetGuid } }; break;
                    case ActionType.Pause: message = new MarkMessage(null, null, ActionType.Pause) { { PropertyString.TargetGuid, oa.TargetGuid } }; break;
                    case ActionType.Resume: message = new MarkMessage(null, null, ActionType.Resume) { { PropertyString.TargetGuid, oa.TargetGuid } }; break;
                    case ActionType.Update:
                        var workFlow = service.Pull(oa.TargetGuid);
                        message = new MarkMessage();
                        message[PropertyString.TargetGuid] = oa.TargetGuid;
                        message[PropertyString.WorkFlow] = workFlow;
                        message[PropertyString.Action] = ActionType.Update;
                        break;
                    default:
                        message = null; break;
                }
                if (message != null) Server.Process(message);
            }
        }

        private static void Socket_DataReceived(object sender, TcpServerDataReceivedArgs e)
        {
            var data = DataConverter.Unpack(e.ReceivedBuffer);
            foreach (var item in data)
            {
                var json = System.Text.Encoding.UTF8.GetString(item);
                var model = JsonConvert.DeserializeObject<MarkMessage>(json, jSetting);
                Server.Upload(e.RemoteHost.ToString(), e.RemotePort, model);
            }
        }

        private static void Socket_ClientDisconnected(object sender, TcpServerClientDisconnectdArgs e)
        {
            Server.ClientClose(e.RemoteHost.ToString(), e.RemotePort);
        }

        private static void Socket_ClientConnected(object sender, TcpServerClientConnectdArgs e)
        {
            Server.ClientConnect(e.RemoteHost.ToString(), e.RemotePort);
        }

        #region Gui
        private static void GuiSocket_DataReceived(object sender, UdpServerDataReceivedArgs e)
        {
            var json = System.Text.Encoding.UTF8.GetString(e.ReceivedBuffer);
            var model = JsonConvert.DeserializeObject<MarkMessage>(json, jSetting);

            string cmd = (string)model[PropertyString.Action];
            var message = new MarkMessage();
            if (cmd == "GetClients")
            {
                message[PropertyString.Action] = "GetClients";
                message.Add("ConnectedClients", ConnectedClients);
                message.Add("RegisterdClients", RegisterdClients);

                var messageJson = JsonConvert.SerializeObject(message, jSetting);
                var buffer = System.Text.Encoding.UTF8.GetBytes(messageJson);
                GuiSocket.Send(buffer, new IPEndPoint(e.RemoteHost, e.RemotePort));
            }
        }
        #endregion
    }
}
