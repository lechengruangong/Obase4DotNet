/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举关系运算符.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:11:45
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     枚举关系运算符。
    /// </summary>
    public enum ERelationOperator
    {
        /// <summary>
        ///     等于
        /// </summary>
        Equal,

        /// <summary>
        ///     不等于
        /// </summary>
        Unequal,

        /// <summary>
        ///     小于等于
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        ///     小于
        /// </summary>
        LessThan,

        /// <summary>
        ///     大于
        /// </summary>
        GreaterThan,

        /// <summary>
        ///     大于等于
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        ///     LIKE
        /// </summary>
        Like,

        /// <summary>
        ///     IN
        /// </summary>
        In,

        /// <summary>
        ///     NOT IN
        /// </summary>
        NotIn
    }
}