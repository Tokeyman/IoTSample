using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModelStandard.MessageModel
{
    /// <summary>
    /// 命令类型
    /// </summary>
    public static class CommandString
    {
        public const string Register = "Register";
        public const string Push = "Push";
        public const string Pull = "Pull";

        public const string Start = "Start";
        public const string Pause = "Pause";
        public const string Resume = "Resume";
        public const string Stop = "Stop";
        public const string Update = "Update";
    }

    /// <summary>
    /// 数据传输属性说明
    /// </summary>
    public static class PropertyString
    {
        public const string Guid = "Guid";
        public const string Sender = "Sender";
        public const string Command = "Command";
        public const string Data = "Data";
    }

    /// <summary>
    /// 继承一下类型，为了封装类型
    /// </summary>
    public class Message : Dictionary<string, object>
    {
        /// <summary>
        /// 初始化自带4个属性，其余属性可自有扩展
        /// </summary>
        public Message()
        {
            Add(PropertyString.Guid, null);
            Add(PropertyString.Sender, null);
            Add(PropertyString.Command, null);
            Add(PropertyString.Data, null);
        }

        /// <summary>
        /// 具备参数的初始化
        /// </summary>
        /// <param name="Guid"></param>
        /// <param name="Sender"></param>
        /// <param name="Command"></param>
        /// <param name="Data">无数据为null</param>
        public Message(object Guid,object Sender,object Command,object Data):this()
        {
            base[PropertyString.Guid] = Guid;
            base[PropertyString.Sender] = Sender;
            base[PropertyString.Command] = Command;
            base[PropertyString.Data] = Data;
        }
    }
}
