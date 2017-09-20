using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;

namespace XField
{
    public class MessageModel
    {
        public string Guid { get; set; }
        public string Sender { get; set; }
        public string Command { get; set; }
        public Dictionary<string, object> Data { get; set; }

        public MessageModel()
        {
            Data = new Dictionary<string, object>();
        }
    }

    public class WorkFlow
    {
        public List<Flow> Timing { get; set; }
        public List<Flow> Repeat { get; set; }
        public WorkFlow()
        {
            this.Timing = new List<Flow>();
            this.Repeat = new List<Flow>();
        }
    }

    public class Flow
    {
        public int Index { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public byte[] Command { get; set; }

        public Flow() { }
        public Flow(int Index, TimeSpan TimeSpan, byte[] Command)
        {
            this.Index = Index;
            this.TimeSpan = TimeSpan;
            this.Command = Command;
        }
    }
    public static class PropertyString
    {
        public const string Guid = "Guid";
        public const string Sender = "Sender";
        public const string Command = "Command";
        public const string Data = "Data";
    }
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
        public Message(object Guid, object Sender, object Command, object Data) : this()
        {
            base[PropertyString.Guid] = Guid;
            base[PropertyString.Sender] = Sender;
            base[PropertyString.Command] = Command;
            base[PropertyString.Data] = Data;
        }
    }

    /* 
    class Program
    {
       
        static void Main(string[] args)
        {
            WorkFlow workFlow = new WorkFlow();

            workFlow.Timing.Add(new Flow(1, TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x10, 0xfe }));
            workFlow.Timing.Add(new Flow(2, TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x11, 0xfe }));
            workFlow.Timing.Add(new Flow(3, TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x12, 0xfe }));
            workFlow.Timing.Add(new Flow(4, TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x13, 0xfe }));
            workFlow.Timing.Add(new Flow(5, TimeSpan.FromSeconds(5), new byte[] { 0x7f, 0xef, 0x14, 0xfe }));

            workFlow.Repeat.Add(new Flow(1, TimeSpan.FromSeconds(2), new byte[] { 0x7f, 0xef, 0x30, 0xfe }));
            workFlow.Repeat.Add(new Flow(2, TimeSpan.FromSeconds(2), new byte[] { 0x7f, 0xef, 0x31, 0xfe }));


            MessageModel message = new MessageModel();
            message.Guid = "0001";
            message.Sender = "Client";
            message.Command = "Update";
            message.Data.Add("WorkFlow", workFlow);

            Dictionary<string, object> mess = new Dictionary<string, object>();
            mess.Add("Guid", "0001");
            mess.Add("Sender", "Client");
            mess.Add("Command", "Update");
            mess.Add("Data", workFlow);

            Message a = new Message("0006", "Sender7", "Update", "Data");

            JsonSerializerSettings jSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };

            var json = JsonConvert.SerializeObject(a,jSettings);
            Console.ReadLine();

            var newModel = JsonConvert.DeserializeObject<Message>(json,jSettings);

            //var newFlow = (WorkFlow)newModel["Data"];
            
            Console.ReadLine();
        }



}*/
}
