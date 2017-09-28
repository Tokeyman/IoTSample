using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModelStandard.MessageModel;

namespace MarkHost
{
    public interface IServerService
    {
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="ClientGuid"></param>
        /// <param name="Ip"></param>
        /// <param name="Port"></param>
        void Register(string ClientGuid, string Ip, int Port);

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="Ip"></param>
        /// <param name="Port"></param>
        void Close(string Ip, int Port);

        /// <summary>
        /// 向数据库推送数据
        /// </summary>
        /// <param name="Buffer"></param>
        void Push(string ClientGuid, byte[] Buffer, string Status);

        /// <summary>
        /// 从数据库获取工作编程信息
        /// </summary>
        /// <param name="Guid"></param>
        /// <returns></returns>
        WorkFlow Pull(string Guid);

        OperationAction DequeueOperation();

    }

    public class OperationAction
    {
        public string TargetGuid { get; set; }
        public string Action { get; set; }
    }
}
