using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModelStandard.MessageModel;

namespace XField
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] buffer1 = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            byte[] buffer2 = new byte[] { 0x05, 0x06, 0x07, 0x08 };

            var pa1 = DataConverter.Pack(buffer1);
            var pa2 = DataConverter.Pack(buffer2);

            var pa = pa1.Concat(pa2).ToArray();



            Console.WriteLine(Transform.BytesToString(pa));


            var result = DataConverter.Unpack(pa);


            Console.ReadLine();
        }
    }
}
