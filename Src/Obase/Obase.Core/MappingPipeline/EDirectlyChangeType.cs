/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举就地修改类型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:46:22
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     枚举就地修改类型。
    /// </summary>
    [Flags]
    [Serializable]
    public enum EDirectlyChangeType : byte
    {
        /// <summary>
        ///     删除对象。
        /// </summary>
        Delete,

        /// <summary>
        ///     更新属性值。
        /// </summary>
        Update,

        /// <summary>
        ///     属性值自增。
        /// </summary>
        Increment
    }
}