using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DataModelStandard.MessageModel
{
    public class ConnectedClient
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public ConnectedClient() { }
        public ConnectedClient(string Ip, int Port) { this.Ip = Ip; this.Port = Port; }
    }
    public class RegisterdClient
    {
        public string Guid { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public RegisterdClient() { }
        public RegisterdClient(string Guid, string Ip, int Port) { this.Guid = Guid; this.Ip = Ip; this.Port = Port; }
    }

    public class MarkServer
    {
        public string Guid { get; private set; }
        public string Name { get; private set; }

        public List<ConnectedClient> ConnectedList { get; private set; }
        public List<RegisterdClient> ClientList { get; private set; }

        public MarkServer(string Guid, string Name)
        {
            this.Guid = Guid;
            this.Name = Name;
            ConnectedList = new List<ConnectedClient>();
            ClientList = new List<RegisterdClient>();
        }


        #region 方法
        /// <summary>
        /// 客户端连接
        /// </summary>
        /// <param name="Ip"></param>
        /// <param name="Port"></param>
        public void ClientConnect(string Ip, int Port)
        {
            var c = ConnectedList.FirstOrDefault(f => f.Ip == Ip && f.Port == Port);
            if (c == null) ConnectedList.Add(new ConnectedClient(Ip, Port));
            RaiseClientConnected(Ip, Port);
        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="Ip"></param>
        /// <param name="Port"></param>
        public void ClientClose(string Ip, int Port)
        {
            var c = ConnectedList.FirstOrDefault(f => f.Ip == Ip && f.Port == Port);
            if (c != null) ConnectedList.Remove(c);
            var cc = ClientList.FirstOrDefault(f => f.Ip == Ip && f.Port == Port);
            if (cc != null) ClientList.Remove(cc);
            RaiseClientClosed(Ip, Port);
        }


        /// <summary>
        /// 处理从Socket接收的信息
        /// </summary>
        /// <param name="Ip"></param>
        /// <param name="Port"></param>
        /// <param name="Message"></param>
        public void Upload(string Ip, int Port, MarkMessage Message)
        {
            var cmd = (string)Message[PropertyString.Action];
            var guid = (string)Message[PropertyString.SenderGuid];
            if (cmd == ActionType.Register)
            {
                var c = ClientList.FirstOrDefault(f => f.Guid == guid);
                if (c == null) ClientList.Add(new RegisterdClient(guid,Ip,Port));
                RaiseClientRegisterd(guid, Ip, Port);
            }
            else
            {
                if (!ClientListContains(guid)) return;  //客户端列表无对象 返回
                if (cmd == ActionType.Push)
                {
                    RaiseSendToDb(Message);
                }
                else if (cmd == ActionType.Pull)
                {
                    RaiseSendToDb(Message);
                }
            }
        }

        /// <summary>
        /// Db层消息处理
        /// </summary>
        /// <param name="Message"></param>
        public void Process(MarkMessage Message)  //Message 消息解析 处理Start Stop Pause Resume Update
        {
            /* 传入的Message遵循以下格式
             * TargetGuid Required
             * Action Required
             * WorkFlow Depends on Action
             */
            var cmd = (string)Message[PropertyString.Action];
            var guid = (string)Message[PropertyString.TargetGuid];
            if (!ClientListContains(guid)) return;

            if (cmd == ActionType.Start)
            {
                Message = new MarkMessage(this.Guid, this.Name, ActionType.Start);
            }

            MarkMessage model = new MarkMessage();
            switch (cmd)
            {
                case ActionType.Start: model = Start; break;
                case ActionType.Pause: model = Pause; break;
                case ActionType.Resume: model = Resume; break;
                case ActionType.Stop: model = Stop; break;
                case ActionType.Update: model = Update((WorkFlow)Message[PropertyString.WorkFlow]); break;
                default:
                    break;
            }
            RaiseSendToClient(guid, model);
        }


        #endregion 方法

        #region 私有方法
        private bool ClientListContains(string Guid)
        {
            var c = ClientList.FirstOrDefault(f => f.Guid == Guid);
            if (c == null) return false; else return true;
        }

        #endregion 私有方法

        #region 命令生成

        private MarkMessage Start { get { return new MarkMessage(this.Guid, this.Name, ActionType.Start); } }

        private MarkMessage Pause { get { return new MarkMessage(this.Guid, this.Name, ActionType.Pause); } }

        private MarkMessage Resume { get { return new MarkMessage(this.Guid, this.Name, ActionType.Resume); } }

        private MarkMessage Stop { get { return new MarkMessage(this.Guid, this.Name, ActionType.Stop); } }

        private MarkMessage Update(WorkFlow workFlow)
        {
            var model = new MarkMessage(this.Guid, this.Name, ActionType.Update)
            {
                { PropertyString.WorkFlow, workFlow }
            };
            return model;
        }

        #endregion 命令生成

        #region 事件
        public event EventHandler<ClientConnectedArgs> ClientConnected; //客户端连接
        private void RaiseClientConnected(string Ip, int Port) => ClientConnected?.Invoke(this, new ClientConnectedArgs(Ip, Port));

        public event EventHandler<ClientRegisterdArgs> ClientRegisterd; //客户端注册
        private void RaiseClientRegisterd(string Guid, string Ip, int Port) => ClientRegisterd?.Invoke(this, new ClientRegisterdArgs(Guid, Ip, Port));


        public event EventHandler<ClientClosedArgs> ClientClosed;    //客户端断开连接  
        private void RaiseClientClosed(string Ip, int Port) => ClientClosed?.Invoke(this, new ClientClosedArgs(Ip, Port));

        public event EventHandler<ServerSocketSendArgs> SendToClient;
        private void RaiseSendToClient(string TargetGuid, MarkMessage Message)
        {
            //重整一下
            Message[PropertyString.SenderGuid] = this.Guid;
            Message[PropertyString.Sender] = this.Name;
            Message[PropertyString.TargetGuid] = TargetGuid;

            //从当前的ClientList中获取RemoteIp和RemotePort
            var c = ClientList.FirstOrDefault(f => f.Guid == TargetGuid);
            if (c == null) return;
            var remoteIp = c.Ip;
            var remotePort = c.Port;
            //Invoke the EventHandler to do it's job.
            SendToClient?.Invoke(this, new ServerSocketSendArgs(remoteIp, remotePort, Message));
        }

        public event EventHandler<ServerDbSendArgs> SendToDb;//接收到数据要存储到数据库或者需要从数据库获取数据 Pull and Push
        private void RaiseSendToDb(MarkMessage Message) => SendToDb?.Invoke(this, new ServerDbSendArgs(Message));

        #endregion 事件

    }

    public class ClientConnectedArgs
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public ClientConnectedArgs(string Ip, int Port)
        {
            this.Ip = Ip;
            this.Port = Port;
        }
    }
    public class ClientRegisterdArgs
    {
        public string Guid { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public ClientRegisterdArgs(string Guid, string Ip, int Port) { this.Guid = Guid; this.Ip = Ip; this.Port = Port; }
    }
    public class ClientClosedArgs
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public ClientClosedArgs(string Ip, int Port) { this.Ip = Ip; this.Port = Port; }
    }

    public class ServerSocketSendArgs
    {
        public string RemoteIp { get; set; }
        public int RemotePort { get; set; }
        public MarkMessage Message { get; set; }
        public ServerSocketSendArgs(string RemoteIp, int RemotePort, MarkMessage Message)
        {
            this.RemoteIp = RemoteIp;
            this.RemotePort = RemotePort;
            this.Message = Message;
        }
    }

    public class ServerDbSendArgs
    {
        public MarkMessage Message { get; set; }

        //客户端Guid
        //指令  Push Pull
        /* 
         * Push指令，客户端上传信息，Db处理程序决定是否存储进入数据库
         * Pull指令，客户端拉取信息，Db处理程序获得数据后调用Server对象的Process中Update命令，Server端发送给Client端
         */
        public ServerDbSendArgs(MarkMessage Message)
        {
            this.Message = Message;
        }


    }
}
