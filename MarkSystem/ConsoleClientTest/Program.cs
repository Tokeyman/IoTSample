using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLibrary.Windows.Net;
using System.Net;
using DataModel.MessageModel;
using Newtonsoft.Json;

namespace ConsoleClientTest
{
    class Program
    {
        private static ClientModel MessageClient;
        static void Main(string[] args)
        {
            IPAddress.TryParse("127.0.0.1", out IPAddress remoteHost);

            TcpClient Client = new TcpClient(remoteHost, 21890);

            Client.DataReceived += Client_DataReceived;

            Client.Connect();

            string cmd = Console.ReadLine();

            MessageClient = new ClientModel("0001");

            while (cmd != "quit")
            {
                if (cmd == "Register")
                {
                    var json = JsonConvert.SerializeObject(MessageClient.Register());

                    var buffer = System.Text.Encoding.UTF8.GetBytes(json);

                    Client.Send(buffer);
                }
                else if (cmd == "Pull")
                {

                    var json = JsonConvert.SerializeObject(MessageClient.Pull());
                    var buffer = System.Text.Encoding.UTF8.GetBytes(json);

                    Client.Send(buffer);
                }
                else if(cmd=="Push")
                {
                    MessageClient.Push("Go");
                }
                else
                {
                    Console.WriteLine("Input Again");
                }
                cmd = Console.ReadLine();
            }


            Client.Dispose();
        }

        private static void Client_DataReceived(object sender, TcpClientDataReceivedArgs e)
        {
            var message = System.Text.Encoding.UTF8.GetString(e.ReceivedBuffer);
            //Console.WriteLine("Data Received:" + message);

            var messageFlow = JsonConvert.DeserializeObject<MessageModel>(message);
            if(messageFlow.Command=="Update")
            {
                MessageClient.Update(messageFlow);
            }
           
            Console.WriteLine("TimingCommand Count:" + MessageClient.WorkFlow.TimingCommand.Count.ToString());
            Console.WriteLine("RepeatCommand Count:" + MessageClient.WorkFlow.RepeatCommand.Count.ToString());
        }
    }


}

