/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示DefaultIfEmpty运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:42:42
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示DefaultIfEmpty运算。
    /// </summary>
    public class DefaultIfEmptyOp : QueryOp
    {
        /// <summary>
        ///     序列为空时要返回的值。
        /// </summary>
        private readonly object _defaultValue;

        /// <summary>
        ///     创建DefaultIfEmptyOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="defaultValue">序列为空时要返回的值。</param>
        internal DefaultIfEmptyOp(Type sourceType, object defaultValue = null)
            : base(EQueryOpName.DefaultIfEmpty, sourceType)
        {
            _defaultValue = defaultValue;
        }

        /// <summary>
        ///     获取序列为空时要返回的值。
        /// </summary>
        public object DefaultValue => _defaultValue;

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => _defaultValue?.GetType();
    }
}