/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：数据源不支持处理的并发冲突异常.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:47:00
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     表示“数据源不支持”的异常
    /// </summary>
    public class UnSupportedDataSourceException : ConcurrentConflictException
    {
        /// <summary>
        ///     内部异常
        /// </summary>
        private readonly RepeatInsertionException _repeatInsertionException;

        /// <summary>
        ///     创建ConcurrentConflictException实例。
        /// </summary>
        /// <param name="obj">发生并发冲突的对象。</param>
        /// <param name="objType">发生并发冲突的对象的类型。</param>
        /// <param name="repeatInsertionException">内部异常</param>
        public UnSupportedDataSourceException(object obj, ObjectType objType,
            RepeatInsertionException repeatInsertionException) : base(obj, objType)
        {
            _repeatInsertionException = repeatInsertionException;
        }

        /// <summary>
        ///     发生异常的消息
        /// </summary>
        public override string Message =>
            $"不支持当前{ObjectType.Name}的并发冲突策略-{ObjectType.ConcurrentConflictHandlingStrategy}:{_repeatInsertionException.UnSupportMessage}";

        /// <summary>
        ///     内部异常
        /// </summary>
        public RepeatInsertionException InsertionException => _repeatInsertionException;
    }
}