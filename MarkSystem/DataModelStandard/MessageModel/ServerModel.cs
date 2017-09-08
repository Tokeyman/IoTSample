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

        public List<ClientModel> ClientList;

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
            if (message.Command == CommandString.注册)
            {
                var c = new ClientModel(message.Guid);
                this.ClientList.Add(c);
            }
            else if (message.Command == CommandString.拉取)
            {
                if (!ClientListContains(message.Guid)) return;

                //TODO委托 读取数据库 获得拉取数据 
            }
            else if (message.Command == CommandString.推送)
            {
                if (!ClientListContains(message.Guid)) return;

                //TODO委托 存入数据库 推送上来的数据
            }
        }

        /// <summary>
        /// 客户端断开连接了
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


        //TODO Process
        /// <summary>
        /// 处理上层(DataBase)发送过来的消息
        /// </summary>
        /// <param name="TargetGuid"></param>
        /// <param name="Command"></param>
        /// <param name="Data"></param>
        public void Process(string TargetGuid, string Command, string Data)
        {
            if (!ClientListContains(TargetGuid)) return;
            if (Command == CommandString.开始) RaiseSendToClient(TargetGuid, Start());
            else if (Command == CommandString.暂停) RaiseSendToClient(TargetGuid, Pause());
            else if (Command == CommandString.恢复) RaiseSendToClient(TargetGuid, Resume());
            else if (Command == CommandString.结束) RaiseSendToClient(TargetGuid, Stop());
            else if (Command == CommandString.更新) RaiseSendToClient(TargetGuid, Update(Data));

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

        public event EventHandler SendToDb;
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
        public string Data { get; set; }   //数据 Push

    }
}
