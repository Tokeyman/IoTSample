using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MarkDbModel.Entity
{
    public class OnlineCache
    {
        [Key]
        public string ClientGuid { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public string Status { get; set; } //客户端状态

        public string DataCache { get; set; }

        public OnlineCache() { }
        public OnlineCache(string ClientGuid,string Ip,int Port)
        {
            this.ClientGuid = ClientGuid;
            this.Ip = Ip;
            this.Port = Port;
        }
    }
}
