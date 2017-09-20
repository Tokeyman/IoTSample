using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using MarkDbModel.Entity;

namespace MarkDbModel.Service
{
    public class MarkService : IMarkService
    {
        private ApplicationDbContext db;

        public MarkService() : this(new ApplicationDbContext()) { }
        public MarkService(ApplicationDbContext context) { this.db = context; }

        public void AddClient(string Guid, string Description)
        {
            var m = db.MarkClient.FirstOrDefault(f => f.ClientGuid == Guid);
            if (m != null) return;
            var model = new MarkClient(Guid, Description);
            db.MarkClient.Add(model);
            db.SaveChanges();
        }

        public void Register(string ClientGuid, string Ip, int Port)
        {
            var m = db.MarkClient.FirstOrDefault(f => f.ClientGuid == ClientGuid);
            if (m == null) return;
            m.Online = true;
            m.Ip = Ip;
            m.Port = Port;
            db.SaveChanges();
        }

        public void Close(string Ip, int Port)
        {
            var m = db.MarkClient.FirstOrDefault(f => f.Ip == Ip && f.Port == Port);
            if (m == null) return;
            m.Online = false;
            db.SaveChanges();
        }

        public void Push(string ClientGuid, string Data, string Status)
        {
            var m = db.MarkClient.FirstOrDefault(f => f.ClientGuid == ClientGuid);
            if (m == null) return;
            m.DataCache = Data;
            m.Status = Status;
            db.SaveChanges();
        }

        public CommandGroup Pull(string ClientGuid)
        {
            var m = db.MarkClient.Include(i => i.CommandGroup).FirstOrDefault(f => f.ClientGuid == ClientGuid);
            if (m == null) return null;
            if (m.CommandGroup == null) return null;

            var model = db.CommandGroup.Include(i => i.CommandContexts).FirstOrDefault(f => f.Id == m.CommandGroupId);
            return model;
        }
    }
}
