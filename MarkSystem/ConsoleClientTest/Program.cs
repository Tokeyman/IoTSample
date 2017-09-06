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
        private static ClientModel Client;
        static void Main(string[] args)
        {
            IPAddress.TryParse("127.0.0.1", out IPAddress remoteHost);

            TcpClient Socket = new TcpClient(remoteHost, 21890);

            Socket.DataReceived += Client_DataReceived;

            Socket.Connect();

            string cmd = Console.ReadLine();

            Client = new ClientModel("0001");

            while (cmd != "quit")
            {
                if (cmd == "Register")
                {
                    var json = JsonConvert.SerializeObject(Client.Register());
                    var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                    Socket.Send(buffer);
                }
                else if (cmd == "Pull")
                {

                    var json = JsonConvert.SerializeObject(Client.Pull());
                    var buffer = System.Text.Encoding.UTF8.GetBytes(json);

                    Socket.Send(buffer);
                }
                else if(cmd=="Push")
                {
                    Client.Push("Go");
                }
                else
                {
                    Console.WriteLine("Input Again");
                }
                cmd = Console.ReadLine();
            }


            Socket.Dispose();
        }

        private static void Client_DataReceived(object sender, TcpClientDataReceivedArgs e)
        {
            var message = System.Text.Encoding.UTF8.GetString(e.ReceivedBuffer);
            //Console.WriteLine("Data Received:" + message);

            var messageFlow = JsonConvert.DeserializeObject<MessageModel>(message);
            if(messageFlow.Command=="Update")
            {
                Client.Update(messageFlow);
            }
           
            Console.WriteLine("TimingCommand Count:" + Client.WorkFlow.TimingCommand.Count.ToString());
            Console.WriteLine("RepeatCommand Count:" + Client.WorkFlow.RepeatCommand.Count.ToString());
        }
    }


}

