using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataModel.MessageModel
{
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
                ClientName = this.ClientName,
                Command = "Update",
                Data = json
            };
            return model;
        }
    }
}
