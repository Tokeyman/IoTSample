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
        static void Main(string[] args)
        {
            IPAddress.TryParse("127.0.0.1", out IPAddress remoteHost);

            TcpClient Client = new TcpClient(remoteHost, 21890);

            Client.DataReceived += Client_DataReceived;

            Client.Connect();

            string cmd = Console.ReadLine();

            while (cmd != "c")
            {
                MessageModel model = new MessageModel()
                {
                    Guid = "0001",
                    ClientName = "Client0001",
                    Command = "Register",
                    Data = ""
                };

                var json = JsonConvert.SerializeObject(model);

                var buffer = System.Text.Encoding.UTF8.GetBytes(json);

                Client.Send(buffer);
                cmd = Console.ReadLine();
            }
            Client.Dispose();
            
        }

        private static void Client_DataReceived(object sender, TcpClientDataReceivedArgs e)
        {
            //throw new NotImplementedException();
            var message = System.Text.Encoding.UTF8.GetString(e.ReceivedBuffer);
            Console.WriteLine("Data Received:" + message);
        }
    }


}

