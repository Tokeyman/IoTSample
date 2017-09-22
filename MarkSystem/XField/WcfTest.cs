using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XField.MarkService;
using DataModelStandard;

namespace XField
{
    class Program
    {
        static void Main(string[] args)
        {
            MarkServiceClient client = new MarkServiceClient();
            var message=client.Pull("0001");
            Console.ReadLine();
        }
    }
}
