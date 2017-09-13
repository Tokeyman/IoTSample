using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarkDbModel.Entity
{
    public class MarkClient
    {
        public string Id { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }

        public string CommandGroupId { get; set; }

        [ForeignKey("CommandGroupId")]
        public virtual CommandGroup CommandGroup { get; set; }

        #region 构造函数
        public MarkClient()
        {
            this.Id = Guid.NewGuid().ToString();
            Ip = "";
            Port = 0;
        }

        public MarkClient(string Ip, int Port)
        {
            this.Id = Guid.NewGuid().ToString();
            this.Ip = Ip;
            this.Port = Port;
        }

        public MarkClient Create() => new MarkClient();

        public MarkClient Create(string Ip, int Port) => new MarkClient(Ip, Port);
        #endregion 构造函数

        public void AddToGroup(string GroupId) => CommandGroupId = GroupId;
        public void RemoveGroup() => CommandGroupId = null;

    }
}
