using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using DataModelStandard.MessageModel;
namespace MarkWcf
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IService1”。
    [ServiceContract]
    public interface IMarkService
    {
        /*
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        [OperationContract]
        string Test(string Name);
        */
        // TODO: 在此添加您的服务操作

        [OperationContract]
        string GetDb();

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="ClientGuid"></param>
        /// <param name="Ip"></param>
        /// <param name="Port"></param>
        [OperationContract]
        void Register(string ClientGuid, string Ip, int Port);

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="Ip"></param>
        /// <param name="Port"></param>
        [OperationContract]
        void Close(string Ip, int Port);

        /// <summary>
        /// 向数据库推送数据
        /// </summary>
        /// <param name="Buffer"></param>
        [OperationContract]
        void Push(string ClientGuid, byte[] Buffer, string Status);

        /// <summary>
        /// 从数据库获取工作编程信息
        /// </summary>
        /// <param name="Guid"></param>
        /// <returns></returns>
        [OperationContract]
        WorkFlow Pull(string Guid);

        [OperationContract]
        OperationAction DequeueOperation();

    }

    // 使用下面示例中说明的数据约定将复合类型添加到服务操作。
    // 可以将 XSD 文件添加到项目中。在生成项目后，可以通过命名空间“MarkWcf.ContractType”直接使用其中定义的数据类型。
    /*
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }*/

    [DataContract]
    public class OperationAction
    {
        [DataMember]
        public string TargetGuid { get; set; }
        [DataMember]
        public string Action { get; set; }
    }

    [DataContract]
    public class ConnectedClient
    {
        [DataMember]
        public string Ip { get; set; }
        [DataMember]
        public int Port { get; set; }
    }

    [DataContract]
    public class RegisteredClient //TODO
    {
        [DataMember]
        public string Guid { get; set; }

        [DataMember]
        public string Ip { get; set; }

        [DataMember]
        public int Port { get; set; }
    }
}
