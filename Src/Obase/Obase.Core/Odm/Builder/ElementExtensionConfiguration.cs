/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：元素扩展的配置器,提供类型元素配置的扩展配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 16:15:49
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     元素扩展的配置器。
    /// </summary>
    public abstract class ElementExtensionConfiguration
    {
        /// <summary>
        ///     获取元素扩展的类型。
        /// </summary>
        public abstract ElementExtension ExtensionType { get; }

        /// <summary>
        ///     根据配置元数据生成元素扩展实例。
        ///     实施说明
        ///     寄存生成结果，避免重复生成。
        /// </summary>
        /// <returns>生成的元素扩展实例。</returns>
        internal abstract ElementExtension MakeExtension();
    }
}
