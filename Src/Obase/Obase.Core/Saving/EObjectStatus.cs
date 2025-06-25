/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举对象状态.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:48:08
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     枚举对象状态。
    /// </summary>
    [Flags]
    public enum EObjectStatus
    {
        /// <summary>
        ///     未发生更改。
        /// </summary>
        Unchanged,

        /// <summary>
        ///     新增的
        /// </summary>
        Added,

        /// <summary>
        ///     已删除。
        /// </summary>
        Deleted,

        /// <summary>
        ///     已修改
        /// </summary>
        Modified
    }
}