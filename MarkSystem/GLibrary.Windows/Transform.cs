using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLibrary.Windows
{
    /// <summary>
    /// 通用变换类
    /// </summary>
    public class Transform
    {
        #region String与Byte数组相互转换

        /// <summary>
        /// 按照指定格式将字符串转换为字节数组
        /// </summary>
        /// <param name="Message">要转换的字符串</param>
        /// <param name="Type">字符串类型</param>
        /// <param name="Split">当字符串为Hex型时两个字节间的分隔符</param>
        /// <returns>字符数组</returns>
        static public byte[] StringToByteArray(string Message, Encoding Type, char Split)
        {
            try
            {
                Message = Message.Trim();
                if (Type == Encoding.String)
                    return System.Text.Encoding.ASCII.GetBytes(Message);
                else
                {
                    string[] stringArray = Message.Split(Split);
                    byte[] buffer = new byte[stringArray.Length];
                    for (int i = 0; i < stringArray.Length; i++)
                    {
                        buffer[i] = Convert.ToByte(stringArray[i], 16);
                    }
                    return buffer;
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// 按照指定格式将字节数组转换为字符串
        /// </summary>
        /// <param name="Buffer">要转换的数组</param>
        /// <param name="Type">数组类型</param>
        /// <param name="Split">当字符串为Hex型时两个字节间的分隔符</param>
        /// <returns>字符串</returns>
        static public string ByteArrayToString(byte[] Buffer, Encoding Type, char Split)
        {
            try
            {
                string StringMessage = "";
                if (Type == Encoding.Hex)
                {
                    for (int i = 0; i < Buffer.Length; i++)
                    {
                        StringMessage += Buffer[i].ToString("X2") + Split.ToString();
                    }
                }
                else
                {
                    StringMessage = System.Text.Encoding.Default.GetString(Buffer);
                }
                return StringMessage;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        #endregion

        #region String与decimal数组相互转换
        /// <summary>
        /// Decimal类型转String类型
        /// </summary>
        /// <param name="values">Decimal数组</param>
        /// <param name="Split">分隔符</param>
        /// <returns>结果字符串</returns>
        static public string DecimalArrayToString(decimal[] values, char Split)
        {
            return string.Join(Split.ToString(), values);
        }

        /// <summary>
        /// 字符串转Decimal数组
        /// </summary>
        /// <param name="Message">要转换的字符串</param>
        /// <param name="Split">分隔符</param>
        /// <returns>结果数组</returns>
        static public decimal[] StringToDecimalArray(string Message, char Split)
        {
            try
            {
                string[] strs = Message.Split(Split);
                decimal[] values = new decimal[strs.Length];
                for (int i = 0; i < strs.Length; i++)
                {
                    values[i] = decimal.Parse(strs[i]);
                }
                return values;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region decimal 转byte数组

        /// <summary>
        /// 生成BCD码
        /// </summary>
        /// <param name="numberic">Decimal数值</param>
        /// <param name="dotpos">小数点位数</param>
        /// <param name="HighByte">高字节</param>
        /// <param name="LowByte">低字节</param>
        public static void GenerateBCDByte(decimal numberic, int dotpos, ref byte HighByte, ref byte LowByte)
        {
            int ca = 0, cb = 0;
            for (int i = 0; i < dotpos; i++)
            {
                numberic *= 10;
            }
            if (numberic > 9999m) throw new Exception("参数错误");

            ca = Convert.ToInt32(numberic) / 1000;
            cb = Convert.ToInt32(numberic) % 1000 / 100;
            HighByte = Convert.ToByte(ca * 16 + cb);

            ca = Convert.ToInt32(numberic) % 100 / 10;
            cb = Convert.ToInt32(numberic) % 10;
            LowByte = Convert.ToByte(ca * 16 + cb);
        }


        #endregion

        #region Int 转byte数组
        /// <summary>
        /// 生成BCD码
        /// </summary>
        /// <param name="numberic">Int数值</param>
        /// <param name="HighByte">高字节</param>
        /// <param name="LowByte">低字节</param>
        public static void GenerateBCDByte(int numberic, ref byte HighByte, ref byte LowByte)
        {
            int ca = 0, cb = 0;
            if (numberic > 9999) throw new Exception("参数非法");
            else
            {
                ca = numberic / 1000;
                cb = numberic % 1000 / 100;
                HighByte = Convert.ToByte(ca * 16 + cb);
                ca = numberic % 100 / 10;
                cb = numberic % 10;
                LowByte = Convert.ToByte(ca * 16 + cb);
            }
        }

        /// <summary>
        /// 生成十六进制
        /// </summary>
        /// <param name="numberic">Int数值</param>
        /// <param name="HighByte">高字节</param>
        /// <param name="LowByte">低字节</param>
        public static void GenerateByte(int numberic, ref byte HighByte, ref byte LowByte)
        {
            int ca = 0, cb = 0;
            if (numberic > 65535) throw new Exception("参数非法");
            else
            {
                ca = numberic / 256;
                cb = numberic % 256;
                HighByte = Convert.ToByte(ca);
                LowByte = Convert.ToByte(cb);
            }
        }



        /// <summary>
        /// BCD Int转单字节Byte
        /// </summary>
        /// <param name="numberic">Int数值</param>
        /// <param name="Byte">Byte字节</param>
        public static void GenerateBCDByte(int numberic, ref byte Byte)
        {
            int ca = 0, cb = 0;
            if (numberic > 99) throw new Exception("参数非法");
            else
            {
                ca = numberic / 10;
                cb = numberic % 10;
                Byte = Convert.ToByte(ca * 16 + cb);
            }
        }
        #endregion

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

        #region 获得实际Decimal值
        /// <summary>
        /// 生成实际Decimal值
        /// </summary>
        /// <param name="HighByte">高字节</param>
        /// <param name="LowByte">低字节</param>
        /// <param name="dotpos">小数点位数</param>
        /// <returns></returns>
        public static decimal GetRealDecimal(byte HighByte, byte LowByte, int dotpos)
        {
            decimal dec = 0;
            int d;
            int ca = 0, cb = 0, cc = 0, cd = 0;
            ca = HighByte / 16;
            cb = HighByte % 16;

            d = ca * 1000 + cb * 100;

            cc = LowByte / 16;
            cd = LowByte % 16;

            d += cc * 10 + cd;
            switch (dotpos)
            {
                case 0: dec = Convert.ToDecimal(d); break;
                case 1: dec = Convert.ToDecimal(d) / 10; break;
                case 2: dec = Convert.ToDecimal(d) / 100; break;
                case 3: dec = Convert.ToDecimal(d) / 1000; break;
                case 4: dec = Convert.ToDecimal(d) / 10000; break;
                default:
                    dec = 0m;
                    break;
            }
            return dec;
        }

        /// <summary>
        /// 获得实际参数Decimal值
        /// </summary>
        /// <param name="HighByte">高字节</param>
        /// <param name="MidByte">中字节</param>
        /// <param name="LowByte">低字节</param>
        /// <param name="dotpos">小数位</param>
        /// <returns></returns>
        public static decimal GetRealDecimal(byte HighByte, byte MidByte, byte LowByte, int dotpos)
        {
            decimal dec = 0m;
            int d;
            int ca = 0, cb = 0, cc = 0, cd = 0, ce = 0, cf = 0;

            ca = HighByte / 16;
            cb = HighByte % 16;
            cc = MidByte / 16;
            cd = MidByte % 16;
            ce = LowByte / 16;
            cf = LowByte % 16;

            d = ca * 100000 + cb * 10000 + cc * 1000 + cd * 100 + ce * 10 + cf;
            switch (dotpos)
            {
                case 0: dec = Convert.ToDecimal(d); break;
                case 1: dec = Convert.ToDecimal(d) / 100; break;
                case 2: dec = Convert.ToDecimal(d) / 100; break;
                case 3: dec = Convert.ToDecimal(d) / 1000; break;
                case 4: dec = Convert.ToDecimal(d) / 10000; break;
                case 5: dec = Convert.ToDecimal(d) / 100000; break;
                case 6: dec = Convert.ToDecimal(d) / 1000000; break;
                default:
                    dec = 0m;
                    break;
            }
            return dec;
        }

        /// <summary>
        /// 获得Int值
        /// </summary>
        /// <param name="HighByte">高字节</param>
        /// <param name="LowByte">低字节</param>
        /// <returns>返回值</returns>
        public static int GetRealInt(byte HighByte, byte LowByte)
        {
            int a = 0;
            return a = HighByte * 256 + LowByte;
        }

        /// <summary>
        /// 获得BCD编码的Int值
        /// </summary>
        /// <param name="Byte">字节</param>
        /// <returns>返回值</returns>
        public static int GetRealInt(byte Byte)
        {
            return Byte / 16 * 10 + Byte % 16;
        }
        #endregion
    }
}
