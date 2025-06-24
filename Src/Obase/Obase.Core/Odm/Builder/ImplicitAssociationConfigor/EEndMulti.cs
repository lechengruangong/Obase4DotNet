/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举端在另外一端引用的多重性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 15:07:55
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.Builder.ImplicitAssociationConfigor
{
    /// <summary>
    ///     表示端在另外一端引用的多重性
    /// </summary>
    public enum EEndMulti
    {
        /// <summary>
        ///     没有引用
        /// </summary>
        None,

        /// <summary>
        ///     一对一
        /// </summary>
        Single,

        /// <summary>
        ///     一对多
        /// </summary>
        Multi
    }
}
