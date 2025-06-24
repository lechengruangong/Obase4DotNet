/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举引用元素在对象导航中承担的功能.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 17:15:09
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm
{
    /// <summary>
    ///     枚举引用元素在对象导航中承担的功能。
    /// </summary>
    public enum ENavigationUse
    {
        /// <summary>
        ///     直接引用。
        /// </summary>
        DirectlyReference,

        /// <summary>
        ///     发出引用。
        /// </summary>
        EmittingReference,

        /// <summary>
        ///     到达引用。
        /// </summary>
        ArrivingReference
    }
}
