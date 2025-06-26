/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：结束删除组事件数据类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 09:54:29
└──────────────────────────────────────────────────────────────┘
*/


using System;
using Obase.Core.Odm;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     结束删除组事件数据类
    /// </summary>
    public class EndDeletingGroupEventArgs : DeletingGroupEventArgs
    {
        /// <summary>
        ///     删除操作发生的异常，如果删除成功则值为NULL。
        /// </summary>
        private readonly Exception _exception;


        /// <summary>
        ///     创建EndDeletingGroupEventArgs实例，并指定尝试删除的对象的类型、对象集合及执行过程中发生的异常。
        /// </summary>
        /// <param name="objType">尝试删除对象的类型。</param>
        /// <param name="objects">尝试删除对象的集合。</param>
        /// <param name="exception">删除过程中发生的异常。</param>
        public EndDeletingGroupEventArgs(ObjectType objType, object[] objects, Exception exception = null) : base(
            objType, objects)
        {
            _exception = exception;
        }

        /// <summary>
        ///     获取删除操作发生的异常，如果删除成功则值为NULL。
        /// </summary>
        public Exception Exception => _exception;

        /// <summary>
        ///     获取一个值，该值指示删除操作是否发生了异常。
        /// </summary>
        public bool Failed => _exception != null;
    }
}