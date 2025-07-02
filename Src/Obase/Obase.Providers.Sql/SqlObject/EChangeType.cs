/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举修改类型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:52:57
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     枚举修改类型。
    /// </summary>
    public enum EChangeType
    {
        /// <summary>
        ///     插入
        /// </summary>
        Insert,

        /// <summary>
        ///     更新
        /// </summary>
        Update,

        /// <summary>
        ///     删除
        /// </summary>
        Delete
    }
}