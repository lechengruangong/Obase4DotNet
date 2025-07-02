/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举聚合函数.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:52:20
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     枚举聚合函数。
    /// </summary>
    public enum EAggregationFunction
    {
        /// <summary>
        ///     不执行任何聚合操作。
        /// </summary>
        None,

        /// <summary>
        ///     求平均值。
        /// </summary>
        Average,

        /// <summary>
        ///     统计个数。
        /// </summary>
        Count,

        /// <summary>
        ///     取最大值。
        /// </summary>
        Max,

        /// <summary>
        ///     取最小值。
        /// </summary>
        Min,

        /// <summary>
        ///     算术累加。
        /// </summary>
        Sum
    }
}