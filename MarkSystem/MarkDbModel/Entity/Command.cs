using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarkDbModel.Entity
{
    public class Command
    {
        [Key]
        public string Id { get; set; }
        public int Index { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public string CommandContext { get; set; }
        public bool IsRepeat { get; set; }

        public string CommandGroupId { get; set; }

        [ForeignKey("CommandGroupId")]
        public virtual CommandGroup CommandGroup { get; set; }

        #region 构造函数
        public Command()
        {
            this.Id = Guid.NewGuid().ToString();
            this.Index = 0;
            this.TimeSpan = TimeSpan.Zero;
            this.CommandContext = "";
            this.IsRepeat = false;
            this.CommandGroup = null;
        }

        public Command(int Index, TimeSpan TimeSpan, string CommandContext, bool IsRepeat)
        {
            this.Id = Guid.NewGuid().ToString();
            this.Index = Index;
            this.TimeSpan = TimeSpan;
            this.CommandContext = CommandContext;
            this.IsRepeat = IsRepeat;
            this.CommandGroup = null;
        }

        public static Command Create() => new Command();

        public static Command Create(int Index, TimeSpan TimeSpan, string CommandContext, bool IsRepeat) => new Command(Index, TimeSpan, CommandContext, IsRepeat);
        #endregion 构造函数

        public void AddToGroup(string GroupId) => CommandGroupId = GroupId;
        public void RemoveGroup() => CommandGroupId = null;


        public override string ToString()
        {
            string s = "";
            s += Index.ToString() + " ";
            s += Convert.ToInt32(TimeSpan.TotalSeconds).ToString() + " ";
            s += CommandContext;
            return s;

        }

        public static Command Parse(string s)
        {
            Command c = new Command();
            string[] paras = s.Split(' ');
            c.Index = int.Parse(paras[0]);
            double seconds = double.Parse(paras[1]);
            c.TimeSpan = TimeSpan.FromSeconds(seconds);
            string cc = "";
            for (int i = 2; i < paras.Length; i++)
            {
                cc += paras[i] + " ";
            }
            cc = cc.Substring(0, cc.Length - 1);
            c.CommandContext = cc;
            return c;
        }
    }

    public class CommandGroup
    {
        public string Id { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Command> CommandContexts { get; set; }
        public virtual ICollection<MarkClient> MarkClients { get; set; }

        public CommandGroup() { this.Id = Guid.NewGuid().ToString(); this.Description = ""; }
        public CommandGroup(string Description) { this.Id = Guid.NewGuid().ToString(); this.Description = Description; }
        public static CommandGroup Create() => new CommandGroup();
        public static CommandGroup Create(string Description) => new CommandGroup(Description);
    }
}
