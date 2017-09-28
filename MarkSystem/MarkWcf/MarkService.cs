using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using DataModelStandard.MessageModel;
using MarkDbModel;
using MarkDbModel.Entity;
using GLibrary.Windows.Net;
using System.Diagnostics;

namespace MarkWcf
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的类名“Service1”。
    public class MarkService : IMarkService
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        //存储已经连接的客户端和已经注册的客户端信息供监控程序调用
        public List<ConnectedClient> ConnectedList { get; set; }
        public List<RegisteredClient> RegisterdList { get; set; }

        private TcpServer Socket;
        public MarkService()
        {
            this.ConnectedList = new List<ConnectedClient>();
            this.RegisterdList = new List<RegisteredClient>();
            Socket = new TcpServer(21890);
            Socket.ClientConnected += Socket_ClientConnected;
            Socket.ClientDisconnected += Socket_ClientDisconnected;
            Socket.DataReceived += Socket_DataReceived;
            Socket.Start();
        }

        private void Socket_DataReceived(object sender, TcpServerDataReceivedArgs e)
        {
            Debug.WriteLine("Data:" + e.BytesToRead.ToString());
        }

        private void Socket_ClientDisconnected(object sender, TcpServerClientDisconnectdArgs e)
        {
            Debug.WriteLine("Disconnected" + e.RemoteHost.ToString() + e.RemotePort.ToString());
        }

        private void Socket_ClientConnected(object sender, TcpServerClientConnectdArgs e)
        {
            Debug.WriteLine("Connected:" + e.RemoteHost.ToString() + e.RemotePort.ToString());
        }

        public string GetDb()
        {
            var a = db.MarkClient.FirstOrDefault();
            return a.ClientGuid;
        }

        #region Wcf

        public void Register(string ClientGuid, string Ip, int Port)
        {
            var client = db.MarkClient.FirstOrDefault(f => f.ClientGuid == ClientGuid);
            if (client == null) return;
            client.Online = true;
            client.Ip = Ip;
            client.Port = Port;
            db.SaveChanges();
        }

        public void Close(string Ip, int Port)
        {
            var client = db.MarkClient.FirstOrDefault(f => f.Ip == Ip && f.Port == Port);
            if (client == null) return;
            client.Online = false;
            client.Ip = "";
            client.Port = 0;
            db.SaveChanges();
        }

        public WorkFlow Pull(string Guid)
        {
            var client = db.MarkClient.FirstOrDefault(f => f.ClientGuid == Guid);
            if (client == null) return null;
            if (client.CommandGroup == null) return null;
            var cmds = client.CommandGroup.CommandContexts;

            WorkFlow workFlow = new WorkFlow();

            var timingCmd = cmds.Where(w => w.IsRepeat == false).OrderBy(o => o.Index);
            var repeatCmd = cmds.Where(w => w.IsRepeat == true).OrderBy(o => o.Index);
            foreach (var item in timingCmd)
            {
                workFlow.AddTimingCommand(item.TimeSpan, Transform.StringToBytes(item.CommandContext));
            }
            foreach (var item in repeatCmd)
            {
                workFlow.AddRepeatCommand(Transform.StringToBytes(item.CommandContext));
            }
            return workFlow;
        }

        public void Push(string ClientGuid, byte[] Buffer, string Status)
        {
            var client = db.MarkClient.FirstOrDefault(f => f.ClientGuid == ClientGuid);
            if (client == null) return;
            var data = Transform.BytesToString(Buffer);
            client.DataCache = data;
            client.Status = Status;
            db.SaveChanges();
        }

        public OperationAction DequeueOperation()
        {
            var model = db.Operation.FirstOrDefault();
            if (model == null) return null;
            var op = new OperationAction() { TargetGuid = model.TargetGuid, Action = model.Action };
            db.Operation.Remove(model);
            db.SaveChanges();
            return op;
        }

        public string Test(string Name)
        {
            return "1111 " + Name;
        }

        #endregion Wcf

    }
}
