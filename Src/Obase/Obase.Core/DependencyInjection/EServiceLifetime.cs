/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：服务的生命周期枚举.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 10:56:24
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.DependencyInjection
{
    /// <summary>
    ///     服务的生命周期
    /// </summary>
    public enum EServiceLifetime
    {
        /// <summary>
        ///     单例的
        ///     从始至终使用的都是同一个对象
        /// </summary>
        Singleton,

        /// <summary>
        ///     多例的
        ///     每次使用时创建新的对象
        /// </summary>
        Transient
    }
}
