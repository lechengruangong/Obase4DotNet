/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示“更新幻影”冲突的异常.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:59:00
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     表示“更新幻影”冲突的异常。
    /// </summary>
    public class UpdatingPhantomException : ConcurrentConflictException
    {
        /// <summary>
        ///     创建UpdatingPhantomException实例。
        /// </summary>
        /// <param name="obj">发生冲突的对象。</param>
        /// <param name="objType">发生冲突的对象的类型。</param>
        /// <param name="exception">内部异常</param>
        public UpdatingPhantomException(object obj, ObjectType objType, Exception exception) : base(obj, objType, exception)
        {
        }

        /// <summary>
        ///     返回异常详细信息。
        ///     信息格式：“发生了并发冲突，更新对象时发现目标对象已被删除，对象标识为[ObjectKey]。”
        /// </summary>
        public override string Message => $"发生了并发冲突，更新对象时发现目标对象已被删除，对象标识为[{ObjectKey}]";
    }
}