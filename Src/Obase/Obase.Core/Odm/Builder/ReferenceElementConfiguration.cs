/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联元素配置项,为关联引用配置项和关联端配置项提供基础实现.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 16:59:44
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     为关联引用配置项和关联端配置项提供基础实现。
    /// </summary>
    /// <typeparam name="TObject">关联型或关联端的类型</typeparam>
    /// <typeparam name="TConfiguration">具体的配置项类型</typeparam>
    public abstract class
        ReferenceElementConfiguration<TObject, TConfiguration> :
        TypeElementConfiguration<TObject, TConfiguration>,
        IReferenceElementConfigurator,
        ILazyLoadingConfiguration
        where TObject : class
        where TConfiguration : ReferenceElementConfiguration<TObject, TConfiguration>
    {

    }
}
