using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataModel.MessageModel
{
    /// <summary>
    /// 将客户端和服务器端的所有指令操作进行封装
    /// </summary>
    public class ClientModel
    {
        public string Guid { get; private set; }
        public string ClientName { get; private set; }

        public WorkFlowModel WorkFlow { get; set; }

        public ClientModel(string Guid)
        {
            this.Guid = Guid;
            this.ClientName = "Client" + Guid;
            this.WorkFlow = new WorkFlowModel();
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <returns>消息实体类</returns>
        public MessageModel Register()
        {
            var model = new MessageModel
            {
                Guid = this.Guid,
                ClientName = this.ClientName,
                Command = "Register",
                Data = ""
            };
            return model;
        }

        /// <summary>
        /// 拉取工作程序
        /// </summary>
        /// <returns>消息实体类</returns>
        public MessageModel Pull()
        {
            var model = new MessageModel
            {
                Guid = this.Guid,
                ClientName = this.ClientName,
                Command = "Pull",
                Data = ""
            };
            return model;
        }

        /// <summary>
        /// 推送消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns>消息实体类</returns>
        public MessageModel Push(string message)
        {
            var model = new MessageModel
            {
                Guid = this.Guid,
                ClientName = this.ClientName,
                Command = "Pull",
                Data = message
            };
            return model;
        }

        /// <summary>
        /// 更新工作程序
        /// </summary>
        public void Update(MessageModel message)
        {
            var json = message.Data;
            MessageWorkFlowModel messageModel = JsonConvert.DeserializeObject<MessageWorkFlowModel>(json);
            this.WorkFlow = messageModel.ToModel();
        }
    }

    public class ServerModel
    {
        private string Guid;
        private string ClientName;

        public ServerModel()
        {
            this.Guid = "0000";
            this.ClientName = "Server" + Guid;
        }

        public MessageModel Start()
        {
            var model = new MessageModel()
            {
                Guid = this.Guid,
                ClientName = this.ClientName,
                Command = "Start",
                Data = ""
            };
            return model;
        }

        public MessageModel Pause()
        {
            var model = new MessageModel()
            {
                Guid = this.Guid,
                ClientName = this.ClientName,
                Command = "Pause",
                Data = ""
            };
            return model;
        }

        public MessageModel Stop()
        {
            var model = new MessageModel()
            {
                Guid = this.Guid,
                ClientName = this.ClientName,
                Command = "Stop",
                Data = ""
            };
            return model;
        }

        public MessageModel Update(WorkFlowModel workFlow)
        {
            MessageWorkFlowModel messageFlow = workFlow.ToMessage();
            string json = JsonConvert.SerializeObject(messageFlow);
            var model = new MessageModel()
            {
                Guid = this.Guid,
                ClientName=this.ClientName,
                Command="Update",
                Data=json
            };
            return model;
        }
    }
}
