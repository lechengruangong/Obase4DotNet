/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：指示是否使用延迟加载的接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 16:56:32
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     指示是否使用延迟加载的接口
    /// </summary>
    public interface ILazyLoadingConfiguration
    {
        /// <summary>
        ///     是否启用延迟加载
        /// </summary>
        bool EnableLazyLoading { get; }

        /// <summary>
        ///     指定关联或关联端的加载优先级，数值小者先加载。
        /// </summary>
        int LoadingPriority { get; }
    }
}