using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModelStandard.MessageModel
{
    /// <summary>
    /// 通信消息实体类
    /// </summary>
    public class MessageModel
    {
        /// <summary>
        /// 唯一ID，客户端上地址
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// 客户端名称 Client+Guid组成  消息来源 
        /// </summary>
        public string SenderName { get; set; }


        /// <summary>
        /// 命令，数据类型
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// 数据Json
        /// </summary>
        public string Data { get; set; }

    }


    internal class MessageWorkFlowModel
    {
        public List<MessageFlowModel> TimingCommand { get; set; }
        public List<MessageFlowModel> RepeatCommand { get; set; }

        public MessageWorkFlowModel()
        {
            this.TimingCommand = new List<MessageFlowModel>();
            this.RepeatCommand = new List<MessageFlowModel>();
        }

        internal WorkFlowModel ToModel()
        {
            WorkFlowModel model = new WorkFlowModel();
            foreach (var item in TimingCommand)
            {
                model.TimingCommand.Add(item.ToModel());
            }

            foreach (var item in RepeatCommand)
            {
                model.RepeatCommand.Add(item.ToModel());
            }
            return model;
        }
    }

    /// <summary>
    /// 用作数据传输
    /// </summary>
    internal class MessageFlowModel
    {
        public string Index { get; set; }
        public string TimeSpan { get; set; }
        public string Command { get; set; }

        public MessageFlowModel() { }
        public MessageFlowModel(string Index, string TimeSpan, string Command)
        {
            this.Index = Index;
            this.TimeSpan = TimeSpan;
            this.Command = Command;
        }

        internal FlowModel ToModel()
        {
            FlowModel model = new FlowModel();
            model.Index = Convert.ToInt32(this.Index);
            model.TimeSpan = System.TimeSpan.Parse(this.TimeSpan);
            model.Command = Transform.StringToBytes(Command);
            return model;
        }
    }

    public static class CommandString
    {
        public const string 注册 = "Register";
        public const string 推送 = "Push";
        public const string 拉取 = "Pull";

        public const string 开始 = "Start";
        public const string 暂停 = "Pause";
        public const string 恢复 = "Resume";
        public const string 结束 = "Stop";
        public const string 更新 = "Update";
    }
}
