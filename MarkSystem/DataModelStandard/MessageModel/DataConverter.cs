using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataModelStandard.MessageModel
{
    public static class DataConverter
    {
        //Packer 
        //Add Start Bytes 0x7f 0xef
        //Add Length 4 Bytes Int32  indicates data length
        //Orignal Data
        //Add End Bytes 0xfe

        public static byte[] Pack(byte[] buffer)
        {
            List<byte> bufferList = buffer.ToList();
            int length = bufferList.Count;
            var lengthBuffer = Transform.IntToBytes(length);
            bufferList.InsertRange(0, lengthBuffer);
            bufferList.InsertRange(0, new byte[] { 0x7f, 0xef });
            bufferList.Add(0xfe);
            return bufferList.ToArray();
        }

        //Unpack buffer and returns as list
        private static byte[] StartBytes = new byte[] { 0x7f, 0xef };
        private static List<byte> Buffer = new List<byte>();
        public static List<byte[]> Unpack(byte[] Stream)
        {
            //TODO do sth.
            List<byte[]> Data = new List<byte[]>();

            Buffer.AddRange(Stream);
            if (Buffer.Count > 20480) Buffer.Clear();
            while (Buffer.Count > 6) //start bytes and length bytes  2+4
            {
                bool StartIsRight = true;
                for (int i = 0; i < 2; i++)
                {
                    if (Buffer[i] != StartBytes[i]) { StartIsRight = false; break; }
                }
                if (StartIsRight) //起始符正确
                {
                    //提取长度
                    byte[] lengthArrays = new byte[] { Buffer[2], Buffer[3], Buffer[4], Buffer[5] };
                    int cmdLenth = Transform.BytesToInt(lengthArrays);

                    if (Buffer.Count < (cmdLenth + 7)) break;//未接收完全 跳出，等待下次处理

                    bool EndIsRight = true;
                    if (Buffer[cmdLenth - 1 + 7] != 0xfe) { EndIsRight = false; break; } //结束符不对
                    if(EndIsRight)
                    {
                        //提取数据，保存起来
                        byte[] cmd = Buffer.Skip(6).Take(cmdLenth).ToArray();
                        Data.Add(cmd);
                        Buffer.RemoveRange(0, cmdLenth + 7);
                        continue; //继续处理
                    }
                    else //结束符错误 删除掉这条数据
                    {
                        Buffer.RemoveRange(0, cmdLenth + 7);
                    }

                }
                else //起始符错误
                {
                    Buffer.RemoveAt(0); //删除掉第一个字节循环判定
                }
            }

            return Data;
        }


    }
}
