/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举Sql语句的类型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 11:54:19
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Providers.Sql.SqlObject
{
    /// <summary>
    ///     枚举Sql语句的类型。
    /// </summary>
    [Flags]
    public enum ESqlType : byte
    {
        /// <summary>
        ///     插入。
        /// </summary>
        Insert,

        /// <summary>
        ///     删除。
        /// </summary>
        Delete,

        /// <summary>
        ///     更新。
        /// </summary>
        Update,

        /// <summary>
        ///     查询。
        /// </summary>
        Query
    }
}