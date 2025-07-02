/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举集运算的运算符.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:46:29
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Query
{
    /// <summary>
    ///     枚举集运算的运算符。
    /// </summary>
    public enum ESetOperator
    {
        /// <summary>
        ///     求并集（不去重）。
        /// </summary>
        Concat,

        /// <summary>
        ///     求交集。
        /// </summary>
        Interact,

        /// <summary>
        ///     求差集。
        /// </summary>
        Except,

        /// <summary>
        ///     求并集（去重）。
        /// </summary>
        Union
    }
}