using System;
using System.Collections.Generic;
using System.Text;

namespace DataModelStandard.MessageModel
{
    public static class Transform
    {

        public static byte[] StringToBytes(string message)
        {
            message = message.Trim();
            string[] strArray = message.Split(' ');
            byte[] buffer = new byte[strArray.Length];
            for (int i = 0; i < strArray.Length; i++)
            {
                buffer[i] = Convert.ToByte(strArray[i], 16);
            }
            return buffer;
        }

        public static string BytesToString(byte[] buffer)
        {
            string message = "";
            for (int i = 0; i < buffer.Length; i++)
            {
                message += buffer[i].ToString("X2") + " ";
            }
            return message;
        }
    }
}
