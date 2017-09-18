using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DataModelStandard.MessageModel
{
    /// <summary>
    /// 将客户端的所有指令操作进行封装
    /// </summary>
    public class MarkClinet
    {
        #region 属性
        public string Guid { get; private set; }
        public string Name { get; private set; }
        public WorkFlow WorkFlow { get; set; }
        public string Status { get; set; }
        #endregion 属性

        public MarkClinet(string Guid)
        {
            this.Guid = Guid;
            this.Name = "Client" + Guid;
            this.WorkFlow = new WorkFlow();
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
                    RaiseSendToServer(Push(""));
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
        public event EventHandler<ClientSendToUartArgs> SendToUart;
        private void RaiseUartSend(string message) => SendToUart?.Invoke(this, new ClientSendToUartArgs(message));
        private void RaiseUartSend(byte[] buffer) => SendToUart?.Invoke(this, new ClientSendToUartArgs(buffer));

        /// <summary>
        /// 通知Socket发送命令
        /// </summary>
        public event EventHandler<ClientSendToServerArgs> SendToServer;
        private void RaiseSendToServer(MarkMessage message) => SendToServer?.Invoke(this, new ClientSendToServerArgs(message));
        #endregion 事件

        #region 方法
        /// <summary>
        /// 开始实例
        /// </summary>
        public void Go()
        {
            //注册本机
            RaiseSendToServer(Register);

            //拉取工作程序
            RaiseSendToServer(Pull);

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
        public void Process(MarkMessage message)
        {
            switch ((string)message[PropertyString.Action])
            {
                case ActionType.Start: Start(); break;
                case ActionType.Pause: Pause(); break;
                case ActionType.Resume: Resume(); break;
                case ActionType.Stop: Stop(); break;
                case ActionType.Update: Update((WorkFlow)message[PropertyString.WorkFlow]); break;
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
            var message =Transform.BytesToString(buffer);
            RaiseSendToServer(Push(message));
        }

        /// <summary>
        /// 注销实例
        /// </summary>
        public void Dispose()
        {
            SendingTimer.Dispose();
        }

        #endregion 方法

        #region 命令处理

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
        private void Update(WorkFlow WorkFlow)
        {
            this.WorkFlow = WorkFlow;
        }
        #endregion 上层命令处理

        #region 命令生成

        /// <summary>
        /// 注册
        /// </summary>
        private MarkMessage Register { get { return new MarkMessage(this.Guid, this.Name, ActionType.Register); } }

        /// <summary>
        /// 拉取工作程序
        /// </summary>
        private MarkMessage Pull { get { return new MarkMessage(this.Guid, this.Name, ActionType.Pull); } }

        /// <summary>
        /// 推送消息
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private MarkMessage Push(string message)
        {
            var model = new MarkMessage(this.Guid, this.Name,ActionType.Push);
            model.Add(PropertyString.Status, this.Status);
            model.Add(PropertyString.Data, message);
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

    public class ClientSendToUartArgs
    {
        public byte[] Buffer { get; set; }
        public ClientSendToUartArgs(string message) => this.Buffer =Transform.StringToBytes(message);
        public ClientSendToUartArgs(byte[] Buffer) => this.Buffer = Buffer;
    }

    public class ClientSendToServerArgs
    {
        public MarkMessage Message { get; set; }
        public ClientSendToServerArgs(MarkMessage Message) => this.Message = Message;
    }
}
