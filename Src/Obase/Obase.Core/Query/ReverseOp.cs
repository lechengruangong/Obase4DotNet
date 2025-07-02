/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示Reverse运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 14:44:39
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示Reverse运算。
    /// </summary>
    public class ReverseOp : QueryOp
    {
        /// <summary>
        ///     创建ReverseOp实例。
        /// </summary>
        /// <param name="sourceType">查询源类型。</param>
        internal ReverseOp(Type sourceType)
            : base(EQueryOpName.Reverse, sourceType)
        {
        }

        /// <summary>
        ///     结果类型
        /// </summary>
        public override Type ResultType => SourceType;
    }
}