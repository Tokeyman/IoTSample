using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DataModelStandard.MessageModel
{
    public class ServerModel
    {
        private string Guid;
        private string Name;

        public List<ClientModel> ClientList { get; set; }

        public ServerModel()
        {
            this.Guid = "0000";
            this.Name = "Server" + Guid;
            ClientList = new List<ClientModel>();
        }

        /// <summary>
        /// 对从Socket传递过来的消息进行处理
        /// </summary>
        /// <param name="message">消息</param>
        public void Upload(MessageModel message)
        {
            //处理传递过来的消息
            //Register 注册
            //Pull 拉取
            //Push 推送
            if (message.Command == CommandString.Register)
            {
                var c = ClientList.FirstOrDefault(f => f.Guid == message.Guid);
                if (c == null) ClientList.Add(new ClientModel(message.Guid));
                this.ClientList.Add(c);
            }
            else if (message.Command == CommandString.Pull)
            {
                if (!ClientListContains(message.Guid)) return;
                RaiseSendToDb(message.Guid, CommandString.Pull, "");
                //委托 读取数据库 获得拉取数据 
            }
            else if (message.Command == CommandString.Push)
            {
                if (!ClientListContains(message.Guid)) return;
                RaiseSendToDb(message.Guid, CommandString.Push, message.Data);
                //委托 存入数据库 推送上来的数据
            }
        }

        /// <summary>
        /// 客户端断开连接了，客户端连接在Process中的注册中处理了
        /// </summary>
        /// <param name="Guid">客户端Guid</param>
        public void ClientDisconnect(string Guid)
        {
            var c = ClientList.FirstOrDefault(f => f.Guid == Guid);
            if (c != null) ClientList.Remove(c);
        }

        /// <summary>
        /// 判定当前客户端列表是否有指定Guid的客户端设备
        /// </summary>
        /// <param name="Guid">Guid</param>
        /// <returns></returns>
        private bool ClientListContains(string Guid)
        {
            var c = ClientList.FirstOrDefault(f => f.Guid == Guid);
            if (c != null) return true; else return false;
        }


        /// <summary>
        /// 处理上层(DataBase)发送过来的消息 非更新指令
        /// </summary>
        /// <param name="TargetGuid"></param>
        /// <param name="Command"></param>
        public void Process(string TargetGuid, string Command)
        {
            if (!ClientListContains(TargetGuid)) return;
            if (Command == CommandString.Start) RaiseSendToClient(TargetGuid, Start());
            else if (Command == CommandString.Pause) RaiseSendToClient(TargetGuid, Pause());
            else if (Command == CommandString.Resume) RaiseSendToClient(TargetGuid, Resume());
            else if (Command == CommandString.Stop) RaiseSendToClient(TargetGuid, Stop());
        }

        /// <summary>
        /// 处理上层(DataBase)发送过来的消息 更新指令
        /// </summary>
        /// <param name="TargetGuid"></param>
        /// <param name="Command"></param>
        /// <param name="WorFlowModel"></param>
        public void Process(string TargetGuid, string Command, WorkFlowModel WorFlowModel)
        {
            if (!ClientListContains(TargetGuid)) return;
            if (Command == CommandString.Update) RaiseSendToClient(TargetGuid, Update(WorFlowModel));
        }

        #region 命令生成
        private MessageModel Start()
        {
            var model = new MessageModel()
            {
                Guid = this.Guid,
                SenderName = this.Name,
                Command = "Start",
                Data = ""
            };
            return model;
        }

        private MessageModel Pause()
        {
            var model = new MessageModel()
            {
                Guid = this.Guid,
                SenderName = this.Name,
                Command = "Pause",
                Data = ""
            };
            return model;
        }

        private MessageModel Resume()
        {
            var model = new MessageModel()
            {
                Guid = this.Guid,
                SenderName = this.Name,
                Command = "Resume",
                Data = ""
            };
            return model;
        }

        private MessageModel Stop()
        {
            var model = new MessageModel()
            {
                Guid = this.Guid,
                SenderName = this.Name,
                Command = "Stop",
                Data = ""
            };
            return model;
        }

        private MessageModel Update(string Data)
        {
            var model = new MessageModel()
            {
                Guid = this.Guid,
                SenderName = this.Name,
                Command = "Update",
                Data = Data
            };
            return model;
        }

        private MessageModel Update(WorkFlowModel workFlow)
        {
            MessageWorkFlowModel messageFlow = workFlow.ToMessage();
            string json = JsonConvert.SerializeObject(messageFlow);
            return Update(json);
        }
        #endregion 命令生成

        public event EventHandler<ServerSocketSendArgs> SendToClient;
        private void RaiseSendToClient(string TargetGuid, MessageModel Message) => SendToClient?.Invoke(this, new ServerSocketSendArgs(TargetGuid, Message));

        public event EventHandler<ServerDbSendArgs> SendToDb;
        private void RaiseSendToDb(string ClientGuid, string Command, string Data) => SendToDb?.Invoke(this, new ServerDbSendArgs(Guid, Command, Data));
    }

    public class ServerSocketSendArgs
    {
        public string TargetGuid { get; set; }
        public MessageModel Message { get; set; }
        public ServerSocketSendArgs(string TargetGuid, MessageModel Message)
        {
            this.TargetGuid = TargetGuid;
            this.Message = Message;
        }
    }

    public class ServerDbSendArgs
    {
        public string Guid { get; set; }  //客户端Guid
        public string Command { get; set; } //指令 Register Push Pull
        /* Register指令，服务端收到客户端注册指令后，发送给Db处理程序，Db处理程序决定在状态列表中添加此设备，此设备可以在其他端显示
         * Push指令，客户端上传信息，Db处理程序决定是否存储进入数据库
         * Pull指令，客户端拉取信息，Db处理程序获得数据后调用Server对象的Process中Update命令，Server端发送给Client端
         * 如此封装主要是为了多少实现一下DDD设计
         * 
         */
        public string Data { get; set; }   //数据 Push
        public ServerDbSendArgs(string Guid, string Command, string Data)
        {
            this.Guid = Guid;
            this.Command = Command;
            this.Data = Data;
        }
    }
}
