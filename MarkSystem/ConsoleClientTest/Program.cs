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
        private static MarkClinet Client;
        private static TcpClient Socket;
        private static JsonSerializerSettings jSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        static void Main(string[] args)
        {
            IPAddress.TryParse("127.0.0.1", out IPAddress remoteHost);

            Socket = new TcpClient(remoteHost, 21890);

            Socket.DataReceived += Socket_DataReceived;
            Socket.OnException += Socket_OnException;
            Socket.Connected += Socket_Connected;
            Socket.Disconected += Socket_Disconected;
            Socket.Connect();

            string cmd = Console.ReadLine();

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

        private static void Socket_Disconected(object sender, EventArgs e)
        {
            Client.Dispose();
            Console.WriteLine("Socket Disonnected");
            Console.WriteLine("Reconnecting in 5 seconds");
            ReconnectTimer = new System.Timers.Timer(5000);
            ReconnectTimer.AutoReset = false;
            ReconnectTimer.Elapsed += ReconnectTimer_Elapsed;
            ReconnectTimer.Start();
        }

        private static void Socket_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("Socket Connected");
            Client = new MarkClinet("0012");
            Client.SendToServer += Client_SendToServer;
            Client.SendToUart += Client_UartSend;
            Client.Go();
        }

        private static System.Timers.Timer ReconnectTimer;

        private static void Socket_OnException(object sender, GLibrary.Windows.ExceptionArgs e)
        {
            Console.WriteLine("Socket error.");
            Console.WriteLine("Reconnecting in 5 seconds");
            ReconnectTimer = new System.Timers.Timer(5000);
            ReconnectTimer.AutoReset = false;
            ReconnectTimer.Elapsed += ReconnectTimer_Elapsed;
            ReconnectTimer.Start();
        }

        private static void ReconnectTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Socket.Connect();
        }

        private static void Client_UartSend(object sender, ClientSendToUartArgs e)
        {
            // throw new NotImplementedException();
            var str = "";
            for (int i = 0; i < e.Buffer.Length; i++)
            {
                str += e.Buffer[i].ToString("X2") + " ";
            }

            Console.WriteLine("SEND TO UART:" + str);
        }

        private static void Client_SendToServer(object sender, ClientSendToServerArgs e)
        {
            var message = JsonConvert.SerializeObject(e.Message, jSettings);
            var buffer = System.Text.Encoding.UTF8.GetBytes(message);
            if (Socket.IsRunning) Socket.Send(DataConverter.Pack(buffer));
        }


        private static void Socket_DataReceived(object sender, TcpClientDataReceivedArgs e)
        {
            var data = DataConverter.Unpack(e.ReceivedBuffer);
            foreach (var item in data)
            {
                var json = System.Text.Encoding.UTF8.GetString(item);
                //Console.WriteLine("Data Received:" + message);

                var message = JsonConvert.DeserializeObject<MarkMessage>(json, jSettings);
                Client.Process(message);
                if ((string)message[PropertyString.Action] == ActionType.Update)
                {
                    //Client.Update(messageFlow);
                    Console.WriteLine("TimingCommand Count:" + Client.WorkFlow.TimingCommand.Count.ToString());
                    Console.WriteLine("RepeatCommand Count:" + Client.WorkFlow.RepeatCommand.Count.ToString());
                }
            }
        }
    }


}

