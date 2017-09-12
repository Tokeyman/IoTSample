using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace XField
{
    class Program
    {
        static void Main(string[] args)
        {
            List<IPAddress> IpList = new List<IPAddress>();

            IPAddress add, comp;
            IPAddress.TryParse("127.0.0.1", out add);

            IpList.Add(add);
            IPAddress.TryParse("127.0.0.2", out add);
            IpList.Add(add);
            comp = IPAddress.Parse("127.0.0.2");


            bool b = add.Equals(comp);
            var c = IpList.FirstOrDefault(f => f.Equals(comp));

            Console.ReadLine();

        }
    }
}
