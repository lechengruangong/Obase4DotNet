/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举元素类型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 16:20:39
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     枚举元素类型。
    /// </summary>
    [Flags]
    public enum EElementType : byte
    {
        /// <summary>
        ///     属性。
        /// </summary>
        Attribute,

        /// <summary>
        ///     关联引用。
        /// </summary>
        AssociationReference,

        /// <summary>
        ///     关联端。
        /// </summary>
        AssociationEnd
    }
}
