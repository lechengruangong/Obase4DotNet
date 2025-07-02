/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举聚合级别.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 17:08:43
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm
{
    /// <summary>
    ///     枚举聚合级别。
    /// </summary>
    public enum EAggregationLevel
    {
        /// <summary>
        ///     不聚合
        /// </summary>
        None,

        /// <summary>
        ///     共享
        /// </summary>
        Shared,

        /// <summary>
        ///     组合
        /// </summary>
        Composite
    }
}