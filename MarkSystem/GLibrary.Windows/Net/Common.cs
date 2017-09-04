using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GLibrary.Windows.Net
{
    /// <summary>
    /// 网络服务通用类
    /// </summary>
    public class Common
    {
        #region 验证IP地址
        /// <summary>
        /// 判定输入IP是否合法
        /// </summary>
        /// <param name="address">IP地址</param>
        /// <returns></returns>
        static public IPAddress VerifyInputIP(string address)
        {
            IPAddress ip = null;
            try
            {
                if (!System.Net.IPAddress.TryParse(address, out ip))
                {
                    throw new ArgumentException("输入IP地址错误，请重新输入");
                }
            }
            catch (Exception)
            {
                return null;
            }
            return ip;
        }

        #endregion

        #region 验证端口
        /// <summary>
        /// 判定输入端口是否合法
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns></returns>
        static public int VerifyInputPort(string port)
        {
            int PortNum;
            try
            {
                if (port == "")
                {
                    throw new ArgumentException("输入端口错误，请重新输入");
                }
                PortNum = Convert.ToInt32(port);
            }
            catch (Exception)
            {
                return -1;
            }
            return PortNum;
        }

        #endregion

        #region 获得本地IP地址列表
        /// <summary>
        /// 获得本地IP地址列表
        /// </summary>
        /// <returns>本地IP地址列表</returns>
        public IPAddress[] GetLocalIpList()
        {
            IPAddress[] localList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            if (localList.Length < 1)
                return null;
            else
                return localList;
        }
        #endregion

    }
}
