/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：隐式关联型的配置器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 16:30:22
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.Core.Odm.Builder.ImplicitAssociationConfigor
{
    /// <summary>
    ///     隐式关联型的配置器
    /// </summary>
    /// <typeparam name="TAssociation">关联型类型</typeparam>
    public class AssociationTypeConfiguration<TAssociation> : Builder.AssociationTypeConfiguration<TAssociation>
        where TAssociation : class
    {
        /// <summary>
        ///     关联端标签。
        /// </summary>
        private readonly string _endsTag;

        /// <summary>
        ///     初始化AssociationTypeConfiguration类的新实例。
        /// </summary>
        /// <param name="endsConfigs">关联型所含关联端的配置项。</param>
        /// <param name="extensionConfigs">关联的扩展配置器。</param>
        /// <param name="endsTag">关联端标签。</param>
        /// <param name="modelBuilder">配置项所属的建模器。</param>
        internal AssociationTypeConfiguration(AssociationEndConfiguration[] endsConfigs,
            TypeExtensionConfiguration[] extensionConfigs, string endsTag,
            ModelBuilder modelBuilder) : base(modelBuilder)
        {
            //初始化容器
            TypeElementConfigurations = new Dictionary<string, TypeElementConfiguration>();
            //赋值
            _endsTag = endsTag;
            foreach (var endConfiguration in endsConfigs)
                TypeElementConfigurations.Add(endConfiguration.Name, endConfiguration);
            ExtensionConfigs.AddRange(extensionConfigs);
        }

        /// <summary>
        ///     获取关联端标签
        ///     关联端标签是以关联端类型（实体型）的完全限定名（即以命名空间限定的名称）串联而成的字符串。同一组类型（顺序无关）建立的多个隐式关联具有相同的关联端标签。
        /// </summary>
        internal string EndsTag => _endsTag;
    }
}
