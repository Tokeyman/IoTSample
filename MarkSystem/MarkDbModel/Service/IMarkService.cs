using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkDbModel.Entity;

namespace MarkDbModel.Service
{
    public interface IMarkService
    {

        #region 给Server端的操作接口
        /// <summary>
        /// 注册客户端
        /// </summary>
        /// <param name="ClientGuid"></param>
        void Register(string ClientGuid,string Ip,int Port);

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="ClientGuid"></param>
        void Close(string Ip,int Port);


        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="Data">Push数据</param>
        /// <param name="Status">客户端状态</param>
        void Push(string ClientGuid,string Data, string Status);

        /// <summary>
        /// 获取工作数据
        /// </summary>
        /// <param name="ClientGuid"></param>
        /// <returns></returns>
        CommandGroup Pull(string ClientGuid);

        #endregion 给Server端的操作接口

        #region 给Web端的操作接口

        //暂时不提供

        #endregion 给Web端的操作接口
    }
}
