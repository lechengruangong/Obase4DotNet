/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举Like类型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 12:04:03
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     枚举Like类型
    /// </summary>
    public enum ELikeType
    {
        /// <summary>
        ///     包含
        /// </summary>
        Contains,

        /// <summary>
        ///     开头
        /// </summary>
        StartWith,

        /// <summary>
        ///     结尾
        /// </summary>
        EndWith
    }
}