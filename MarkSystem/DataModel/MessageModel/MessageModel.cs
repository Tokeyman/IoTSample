using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModel.MessageModel
{
    public class MessageModel
    {
        /// <summary>
        /// 唯一ID，客户端上地址
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// 客户端名称 Client+Guid组成  消息来源 
        /// </summary>
        public string ClientName { get; set; }


        /// <summary>
        /// 命令，数据类型
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// 数据Json
        /// </summary>
        public string Data { get; set; }
    }
}
