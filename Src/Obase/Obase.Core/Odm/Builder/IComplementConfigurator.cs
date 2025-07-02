/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：补充配置规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 16:55:35
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     为在生成模型的过程中执行补充配置定义规范。
    ///     说明
    ///     补充配置是指根据类型配置项中的元数据配置模型中对应的类型，该类型此前已根据当前类型配置项生成并已注册到模型中。
    ///     生成模型总体上分为两步：第一步生成所有类型，在此期间执行类型层面的反射建模；第二步针对每个类型生成其元素，在此期间执行元素层级的反射建模，并且在需要时生成代理类
    ///     型。第一步生成类型时，由于元素还未创建，因此某些配置可能无法当场执行，须等到元素创建完成时执行补充配置。
    /// </summary>
    public interface IComplementConfigurator
    {
        /// <summary>
        ///     补充配置管道中的下一个配置器。
        /// </summary>
        IComplementConfigurator Next { get; }

        /// <summary>
        ///     根据类型配置项中的元数据配置指定的类型。
        /// </summary>
        /// <param name="targetType">要配置的类型。</param>
        /// <param name="configuration">包含配置元数据的类型配置项。</param>
        void Configurate(StructuralType targetType, StructuralTypeConfiguration configuration);
    }
}