using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModelStandard.MessageModel;
using MarkDbModel;

namespace ConsoleServerTest
{
    public class ServerService : IServerService
    {
        public ServerService() { }

        public void Register(string ClientGuid, string Ip, int Port)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                var client = db.MarkClient.FirstOrDefault(f => f.ClientGuid == ClientGuid);
                if (client == null) return;
                client.Online = true;
                client.Ip = Ip;
                client.Port = Port;
                db.SaveChanges();
            }
        }

        public void Close(string Ip, int Port)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                var client = db.MarkClient.FirstOrDefault(f => f.Ip == Ip && f.Port == Port);
                if (client == null) return;
                client.Online = false;
                client.Ip = "";
                client.Port = 0;
                db.SaveChanges();
            }
        }

        public void Push(string ClientGuid, byte[] Buffer, string Status)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {

                var client = db.MarkClient.FirstOrDefault(f => f.ClientGuid == ClientGuid);
                if (client == null) return;
                var data = Transform.BytesToString(Buffer);
                client.DataCache = data;
                client.Status = Status;
                db.SaveChanges();
            }
        }

        public WorkFlow Pull(string Guid)
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
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
              
        }

        public OperationAction DequeueOperation()
        {
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                var model = db.Operation.FirstOrDefault();
                if (model == null) return null;
                var op = new OperationAction() { TargetGuid = model.TargetGuid, Action = model.Action };
                db.Operation.Remove(model);
                db.SaveChanges();
                return op;
            }
              
        }
    }
}
