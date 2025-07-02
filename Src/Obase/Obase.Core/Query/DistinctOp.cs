/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Distinct运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:43:34
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Distinct运算。
    /// </summary>
    public class DistinctOp : QueryOp
    {
        /// <summary>
        ///     相等比较器，用于测试两个元素是否相等。
        /// </summary>
        private readonly IEqualityComparer _comparer;

        /// <summary>
        ///     创建DistinctOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型。</param>
        /// <param name="comparer">相等比较器，用于测试两个元素是否相等。</param>
        internal DistinctOp(Type sourceType, IEqualityComparer comparer = null)
            : base(EQueryOpName.Distinct, sourceType)
        {
            _comparer = comparer;
        }

        /// <summary>
        ///     获取相等比较器，该比较器用于测试两个元素是否相等。
        /// </summary>
        public IEqualityComparer Comparer => _comparer;

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => SourceType;
    }
}