using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DataModelStandard.MessageModel
{
    /// <summary>
    /// 将客户端的所有指令操作进行封装
    /// </summary>
    public class ClientModel
    {
        #region 属性
        public string Guid { get; private set; }
        public string Name { get; private set; }
        public WorkFlowModel WorkFlow { get; set; }
        public string Status { get; set; }
        #endregion 属性

        public ClientModel(string Guid)
        {
            this.Guid = Guid;
            this.Name = "Client" + Guid;
            this.WorkFlow = new WorkFlowModel();
            this.Status = ClientStatus.Idle;
        }

        #region 私有属性
        private int TimingCommandPointer = 0;  //时序命令执行位置记录
        private int RepeatCommandPointer = 0;  //重复命令执行位置记录
        private TimeSpan TimeCount = TimeSpan.Zero;  //时间计时器
        private Timer SendingTimer;

        private void SendingTimerCallback(object state)
        {
            //空闲状态，每5秒发送一次Socket记录
            //运行状态，检查当前timespan是否达到TimingCommand要求，未达到则发送RepeatCommand

            if ((this.Status == ClientStatus.Idle) || (this.Status == ClientStatus.Suspend)) //空闲状态
            {
                //检查是否到5s
                if (TimeCount >= TimeSpan.FromSeconds(5))
                {
                    RaiseSocketSend(Push(""));
                    TimeCount = TimeSpan.Zero;
                }
                else  //未达到继续计数
                {
                    TimeCount += TimeSpan.FromSeconds(1);
                }
            }
            else if (this.Status == ClientStatus.Running)
            {
                if (TimeCount >= WorkFlow.TimingCommand[TimingCommandPointer].TimeSpan)  //达到TimingCommand记录的时间要求，发送数据到UART，清空计时
                {
                    RaiseUartSend(WorkFlow.TimingCommand[TimingCommandPointer].Command);
                    TimeCount = TimeSpan.Zero;
                    TimingCommandPointer++;
                    if (TimingCommandPointer >= WorkFlow.TimingCommand.Count)
                    {
                        Stop(); //结束了
                    }
                }
                else //未达到TimingCommand时间要求，发送RepeatCommand，继续计时
                {
                    RaiseUartSend(WorkFlow.RepeatCommand[RepeatCommandPointer].Command);
                    RepeatCommandPointer++;
                    if (RepeatCommandPointer >= WorkFlow.RepeatCommand.Count) RepeatCommandPointer = 0; //检测发送Repeat是否超出
                    TimeCount += TimeSpan.FromSeconds(1);
                }
            }
        }
        #endregion 私有属性

        #region 事件
        /// <summary>
        /// 通知UART发送命令
        /// </summary>
        public event EventHandler<UartSendArgs> UartSend;
        private void RaiseUartSend(string message) => UartSend?.Invoke(this, new UartSendArgs(message));
        private void RaiseUartSend(byte[] buffer) => UartSend?.Invoke(this, new UartSendArgs(buffer));

        /// <summary>
        /// 通知Socket发送命令
        /// </summary>
        public event EventHandler<ClientSocketSendArgs> SocketSend;
        private void RaiseSocketSend(MessageModel message) => SocketSend?.Invoke(this, new ClientSocketSendArgs(message));
        #endregion 事件

        #region 方法
        /// <summary>
        /// 开始实例
        /// </summary>
        public void Go()
        {
            //注册本机
            RaiseSocketSend(Register());

            //拉取工作程序
            RaiseSocketSend(Pull());

            //开启发送定时器

            TimingCommandPointer = 0;
            RepeatCommandPointer = 0;
            TimeCount = TimeSpan.Zero;
            SendingTimer = new Timer(new TimerCallback(SendingTimerCallback), null, 0, 1000);
        }

        /// <summary>
        /// 处理从Socket过来的数据，对数据进行处理
        /// </summary>
        /// <param name=""></param>
        public void Process(MessageModel message)
        {
            if (message.Command == CommandString.开始)
            {
                Start();
            }
            switch (message.Command)
            {
                case CommandString.开始: Start(); break;
                case CommandString.暂停: Pause(); break;
                case CommandString.恢复: Resume(); break;
                case CommandString.结束: Stop(); break;
                case CommandString.更新: Update(message.Data); break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 处理从UART RXD传递过来的数据
        /// </summary>
        /// <param name="buffer"></param>
        public void Upload(byte[] buffer) //UART传递上来的数据
        {
            var message = System.Text.Encoding.UTF8.GetString(buffer);
            RaiseSocketSend(Push(message));
        }

        /// <summary>
        /// 注销实例
        /// </summary>
        public void Dispose()
        {
            SendingTimer.Dispose();
        }

        #endregion 方法

        #region 上层命令处理



        /// <summary>
        /// 开始执行程序
        /// </summary>
        private void Start()
        {
            TimingCommandPointer = 0;
            RepeatCommandPointer = 0;
            TimeCount = TimeSpan.Zero;
            //设定Status为Running
            this.Status = ClientStatus.Running;
            //按照工作程序执行 ，给外部UART Event RaiseUartSend

        }

        /// <summary>
        /// 暂停程序执行
        /// </summary>
        private void Pause()
        {
            this.Status = ClientStatus.Suspend;
            //暂停工作程序,但是持续Push状态
        }

        /// <summary>
        /// 恢复程序执行
        /// </summary>
        private void Resume()
        {
            this.Status = ClientStatus.Running;
        }
        /// <summary>
        /// 停止程序执行
        /// </summary>
        private void Stop()
        {
            this.Status = ClientStatus.Idle;
            //停止工作程序
            TimingCommandPointer = 0;
            RepeatCommandPointer = 0;
            TimeCount = TimeSpan.Zero;
        }


        /// <summary>
        /// 更新工作程序
        /// </summary>
        private void Update(string MessageModelData)
        {
            var json = MessageModelData;
            MessageWorkFlowModel messageModel = JsonConvert.DeserializeObject<MessageWorkFlowModel>(json);
            this.WorkFlow = messageModel.ToModel();
        }
        #endregion 上层命令处理

        #region 命令生成

        /// <summary>
        /// 注册
        /// </summary>
        /// <returns>消息实体类</returns>
        private MessageModel Register()
        {
            var model = new MessageModel
            {
                Guid = this.Guid,
                SenderName = this.Name,
                Command = CommandString.注册,
                Data = ""
            };
            return model;
        }

        /// <summary>
        /// 拉取工作程序
        /// </summary>
        /// <returns>消息实体类</returns>
        private MessageModel Pull()
        {
            var model = new MessageModel
            {
                Guid = this.Guid,
                SenderName = this.Name,
                Command = CommandString.拉取,
                Data = ""
            };
            return model;
        }

        /// <summary>
        /// 推送消息
        /// </summary>
        /// <param name="message">消息</param>
        /// <returns>消息实体类</returns>
        private MessageModel Push(string message)
        {
            var model = new MessageModel
            {
                Guid = this.Guid,
                SenderName = this.Name,
                Command = CommandString.推送,
                Data = message
            };
            return model;
        }

        #endregion 命令生成
    }

    public static class ClientStatus
    {
        public const string Idle = "Idle";
        public const string Suspend = "Suspend";
        public const string Running = "Runnning";
    }

    public class PushDataModel
    {
        public string Status { get; set; }
        public string RxdData { get; set; }

        public PushDataModel() { }
        public PushDataModel(string ClientStatus, string RxdData)
        {
            this.Status = ClientStatus;
            this.RxdData = RxdData;
        }

        public PushDataModel(string ClientStatus, byte[] buffer)
        {
            this.Status = ClientStatus;
            this.RxdData = System.Text.Encoding.UTF8.GetString(buffer);
        }
    }

    public class UartSendArgs
    {
        public byte[] Buffer { get; set; }
        public UartSendArgs(string message) => this.Buffer = System.Text.Encoding.UTF8.GetBytes(message);
        public UartSendArgs(byte[] Buffer) => this.Buffer = Buffer;
    }

    public class ClientSocketSendArgs
    {
        public MessageModel MessageModel { get; set; }
        public ClientSocketSendArgs(MessageModel MessageModel) => this.MessageModel = MessageModel;
    }
}
