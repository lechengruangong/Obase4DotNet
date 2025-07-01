/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举广义IN运算.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:03:29
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     枚举广义IN运算。
    /// </summary>
    public enum EInOperator
    {
        /// <summary>
        ///     狭义IN运算。
        /// </summary>
        In,

        /// <summary>
        ///     NOT IN运算。
        /// </summary>
        Notin
    }
}