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

    public class ClientStatus
    {
        public const string Idle = "Idle";
        public const string Suspend = "Suspend";
        public const string Running = "Runnning";
    }

    public class PushDataModel
    {
        public string ClientStatus { get; set; }
        public string RxdData { get; set; }

        public PushDataModel() { }
        public PushDataModel(string ClientStatus,string RxdData)
        {
            this.ClientStatus = ClientStatus;
            this.RxdData = RxdData;
        }

        public PushDataModel(string ClientStatus,byte[] buffer)
        {
            this.ClientStatus = ClientStatus;
            this.RxdData = System.Text.Encoding.UTF8.GetString(buffer);
        }
    }
}
