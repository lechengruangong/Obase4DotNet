/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举算术聚合运算符.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:20:29
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Query
{
    /// <summary>
    ///     枚举算术聚合运算符。
    /// </summary>
    public enum EAggregationOperator : byte
    {
        /// <summary>
        ///     求和。
        /// </summary>
        Sum,

        /// <summary>
        ///     求平均数。
        /// </summary>
        Average,

        /// <summary>
        ///     取最大值。
        /// </summary>
        Max,

        /// <summary>
        ///     取最小值。
        /// </summary>
        Min
    }
}