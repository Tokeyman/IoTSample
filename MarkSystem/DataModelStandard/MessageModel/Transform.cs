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
            if (string.IsNullOrWhiteSpace(message)) return new byte[0];
            try
            {
                string[] strArray = message.Split(' ');
                byte[] buffer = new byte[strArray.Length];
                for (int i = 0; i < strArray.Length; i++)
                {
                    buffer[i] = Convert.ToByte(strArray[i], 16);
                }
                return buffer;
            }
            catch (Exception )
            {
                return new byte[0];
            }

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

        #region Int与Byte数组互转

        /// <summary>
        /// Int 转4字节 Byte数组 非BCD变换，高位在前，低位在后
        /// </summary>
        /// <param name="value">Int值</param>
        /// <returns>Byte数组</returns>
        public static byte[] IntToBytes(int value)
        {
            byte[] src = new byte[4];
            src[0] = (byte)((value >> 24) & 0xFF);
            src[1] = (byte)((value >> 16) & 0xFF);
            src[2] = (byte)((value >> 8) & 0xFF);
            src[3] = (byte)(value & 0xFF);
            return src;
        }

        /// <summary>
        /// 4字节Byte数组 转 Int， 高位在前，低位在后
        /// </summary>
        /// <param name="src">Byte数组</param>
        /// <returns>Int值</returns>
        public static int BytesToInt(byte[] src)
        {
            int value;
            value = (int)(((src[0] & 0xFF) << 24)
            | ((src[1] & 0xFF) << 16)
            | ((src[2] & 0xFF) << 8)
            | (src[3] & 0xFF));
            return value;
        }
        #endregion
    }
}
