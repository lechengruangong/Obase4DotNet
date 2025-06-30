/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示“重复创建”冲突的异常.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:59:43
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     表示“重复创建”冲突的异常。
    /// </summary>
    public class RepeatCreationException : ConcurrentConflictException
    {
        /// <summary>
        ///     创建RepeatCreationException实例。
        /// </summary>
        /// <param name="obj">发生冲突的对象。</param>
        /// <param name="objType">发生冲突的对象的类型。</param>
        public RepeatCreationException(object obj, ObjectType objType) : base(obj, objType)
        {
        }

        /// <summary>
        ///     返回异常详细信息。
        ///     信息格式：“发生了并发冲突，创建对象时发现已存在相同标识的对象，对象标识为[ObjectKey]。”
        /// </summary>
        public override string Message => $"发生了并发冲突，创建对象时发现已存在相同标识的对象，对象标识为[{ObjectKey}]";
    }
}