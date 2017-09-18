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
    public static class ActionType  //Could also named ActionType
    {
        public const string Register = "Register";
        public const string Close = "Close"; //断开连接
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
        public const string SenderGuid = "Guid";
        public const string Sender = "Sender";

        public const string TargetGuid = "TargetGuid";
        public const string Target = "Target";

        public const string Action = "Action";
        public const string WorkFlow = "WorkFlow";
        public const string Status = "Status";
        public const string Data = "Data";
    }

    /// <summary>
    /// 继承一下类型，为了封装类型
    /// </summary>
    public class MarkMessage : Dictionary<string, object>
    {
        /// <summary>
        /// 默认构造函数，自带属性SenderGuid,Sender,Action;其余属性自行添加
        /// </summary>
        public MarkMessage()
        {
            Add(PropertyString.SenderGuid, null);
            Add(PropertyString.Sender, null);
            Add(PropertyString.Action, null);
        }

        /// <summary>
        /// 具备参数的初始化
        /// </summary>
        /// <param name="SenderGuid">发送者GUID</param>
        /// <param name="Sender">发送者名字</param>
        /// <param name="Action">动作类型</param>
        public MarkMessage(object SenderGuid, object Sender,object Action) : this()
        {
            base[PropertyString.SenderGuid] = SenderGuid;
            base[PropertyString.Sender] = Sender;
            base[PropertyString.Action] = Action;
        }
    }
}
