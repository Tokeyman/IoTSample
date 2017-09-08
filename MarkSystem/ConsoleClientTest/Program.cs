using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLibrary.Windows.Net;
using System.Net;
using DataModelStandard.MessageModel;
using Newtonsoft.Json;

namespace ConsoleClientTest
{
    class Program
    {
        private static ClientModel Client;
        private static TcpClient Socket;
        static void Main(string[] args)
        {
            IPAddress.TryParse("127.0.0.1", out IPAddress remoteHost);

            Socket = new TcpClient(remoteHost, 21890);

            Socket.DataReceived += Socket_DataReceived;

            Socket.Connect();

            string cmd = Console.ReadLine();

            Client = new ClientModel("0001");
            Client.SocketSend += Client_SocketSend;
            Client.UartSend += Client_UartSend;



            while (cmd != "quit")
            {
                if (cmd == "Go")
                {
                    Client.Go();
                }
                else
                {
                    Console.WriteLine("Input Again");
                }
                cmd = Console.ReadLine();
            }


            Socket.Dispose();
        }

        private static void Client_UartSend(object sender, UartSendArgs e)
        {
            // throw new NotImplementedException();
            var str = "";
            for (int i = 0; i < e.Buffer.Length; i++)
            {
                str += e.Buffer[i].ToString("X2") + " ";
            }

            Console.WriteLine("SEND TO UART:" + str);
        }

        private static void Client_SocketSend(object sender, ClientSocketSendArgs e)
        {
            var message = JsonConvert.SerializeObject(e.MessageModel);
            var buffer = System.Text.Encoding.UTF8.GetBytes(message);
            Socket.Send(buffer);
        }

        private static void Socket_DataReceived(object sender, TcpClientDataReceivedArgs e)
        {
            var message = System.Text.Encoding.UTF8.GetString(e.ReceivedBuffer);
            //Console.WriteLine("Data Received:" + message);

            var messageFlow = JsonConvert.DeserializeObject<MessageModel>(message);
            Client.Process(messageFlow);
            if (messageFlow.Command == "Update")
            {
                //Client.Update(messageFlow);
                Console.WriteLine("TimingCommand Count:" + Client.WorkFlow.TimingCommand.Count.ToString());
                Console.WriteLine("RepeatCommand Count:" + Client.WorkFlow.RepeatCommand.Count.ToString());
            }
        }
    }


}

