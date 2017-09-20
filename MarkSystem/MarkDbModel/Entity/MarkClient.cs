using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarkDbModel.Entity
{
    public class MarkClient  //Remove IP and Port message
    {
        public string Id { get; set; }
        public string ClientGuid { get; set;}
        public string Description { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public bool Online { get; set; }
        public string Status { get; set; }
        public string DataCache { get; set; }

        public string CommandGroupId { get; set; }

        [ForeignKey("CommandGroupId")]
        public virtual CommandGroup CommandGroup { get; set; }

        #region 构造函数
        public MarkClient() { }
        public MarkClient(string ClientGuid,string Description)
        {
            this.Id = Guid.NewGuid().ToString();
            this.ClientGuid = ClientGuid;
            this.Description=Description;
            this.Ip = "";
            this.Port = 0;
            this.Online = false;
            this.Status = "Idle";
        }


        public static MarkClient Create(string ClientGuid,string Description) => new MarkClient(ClientGuid,Description);

        #endregion 构造函数

        public void AddToGroup(string GroupId) => CommandGroupId = GroupId;
        public void RemoveGroup() => CommandGroupId = null;
        public void Modify(string Ip,int Port)
        {
            this.Ip = Ip;
            this.Port = Port;
        }

        public void Cache(string Data)
        {
            this.DataCache = Data;
        }

    }
}
