/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举连接方式.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:19:27
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     枚举连接方式。
    /// </summary>
    public enum ESourceJoinType
    {
        /// <summary>
        ///     左连接
        /// </summary>
        Left,

        /// <summary>
        ///     右连接
        /// </summary>
        Right,

        /// <summary>
        ///     内连接
        /// </summary>
        Inner
    }
}
