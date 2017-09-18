using System;
using System.Collections.Generic;

namespace DataModelStandard.MessageModel
{
    public class WorkFlow
    {
        /// <summary>
        /// 时序命令
        /// </summary>
        public List<Flow> TimingCommand { get; set; }

        /// <summary>
        /// 重复命令
        /// </summary>
        public List<Flow> RepeatCommand { get; set; }

        public WorkFlow()
        {
            this.TimingCommand = new List<Flow>();
            this.RepeatCommand = new List<Flow>();
        }

        /// <summary>
        /// 整理命令序号，保证无错误
        /// </summary>
        private void ReSort()
        {
            for (int i = 0; i < TimingCommand.Count; i++)
            {
                TimingCommand[i].Index = i;
            }

            for (int i = 0; i < RepeatCommand.Count; i++)
            {
                RepeatCommand[i].Index = i;
            }
        }

        public void AddTimingCommand(TimeSpan TimeSpan, byte[] Command)
        {
            TimingCommand.Add(new Flow(TimingCommand.Count, TimeSpan, Command));
        }

        public void AddRepeatCommand(byte[] Command)
        {
            RepeatCommand.Add(new Flow(RepeatCommand.Count, TimeSpan.MinValue, Command));
        }
    }

    public class Flow
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 事件间隔,轮询指令此位无意义
        /// </summary>
        public TimeSpan TimeSpan { get; set; }
        /// <summary>
        /// 指令格式
        /// </summary>
        public byte[] Command { get; set; }

        public Flow() { }
        public Flow(int Index, TimeSpan TimeSpan, byte[] Command)
        {
            this.Index = Index;
            this.TimeSpan = TimeSpan;
            this.Command = Command;
        }

    }


}
