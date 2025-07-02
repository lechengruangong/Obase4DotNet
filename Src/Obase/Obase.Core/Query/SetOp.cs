/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Set运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 14:46:38
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Set运算。
    /// </summary>
    public class SetOp : QueryOp
    {
        /// <summary>
        ///     相等比较器，用于测试来自于两个集合的元素是否相等。
        /// </summary>
        private readonly IEqualityComparer _comparer;

        /// <summary>
        ///     集运算符。
        /// </summary>
        private readonly ESetOperator _operator;

        /// <summary>
        ///     参与运算的另一集合。
        /// </summary>
        private readonly IEnumerable _other;

        /// <summary>
        ///     创建SetOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="operator">集运算符。</param>
        /// <param name="other">参与运算的另一集合。</param>
        /// <param name="comparer">相等比较器，用于测试来自于两个集合的元素是否相等。</param>
        internal SetOp(Type sourceType, ESetOperator @operator, IEnumerable other, IEqualityComparer comparer)
            : base(EQueryOpName.Set, sourceType)
        {
            _operator = @operator;
            _other = other;
            _comparer = comparer;
        }

        /// <summary>
        ///     获取相等比较器，该比较器用于测试来自于两个集合的元素是否相等。
        /// </summary>
        public IEqualityComparer Comparer => _comparer;

        /// <summary>
        ///     获取集运算符。
        /// </summary>
        public ESetOperator Operator => _operator;

        /// <summary>
        ///     获取参与运算的另一集合。
        /// </summary>
        public IEnumerable Other => _other;

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => SourceType;
    }
}