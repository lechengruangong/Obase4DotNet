/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示序列相等运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 14:45:26
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示序列相等运算
    /// </summary>
    public class SequenceEqualOp : QueryOp
    {
        /// <summary>
        ///     相等比较器，用于测试来自两个序列的元素是否相等。
        /// </summary>
        private readonly IEqualityComparer _comparer;

        /// <summary>
        ///     参与比较的另一序列。
        /// </summary>
        private readonly IEnumerable _other;

        /// <summary>
        ///     创建SequenceEqualOp实例。
        /// </summary>
        /// <param name="other">参与比较的另一序列。</param>
        /// <param name="sourceType">源类型</param>
        /// <param name="comparer">相等比较器，用于测试来自两个序列的元素是否相等。</param>
        internal SequenceEqualOp(IEnumerable other, Type sourceType, IEqualityComparer comparer = null)
            : base(EQueryOpName.SequenceEqual, sourceType)
        {
            _other = other;
            _comparer = comparer;
        }

        /// <summary>
        ///     获取相等比较器，该比较器用于测试来自两个序列的元素是否相等。
        /// </summary>
        public IEqualityComparer Comparer => _comparer;

        /// <summary>
        ///     获取参与比较的另一序列。
        /// </summary>
        public IEnumerable Other => _other;

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => SourceType;
    }
}