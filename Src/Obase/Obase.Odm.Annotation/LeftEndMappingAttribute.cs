/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：隐式关联左端标注属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:29:42
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Reflection;
using Obase.Core.Odm.Builder;
using Obase.Core.Odm.Builder.ImplicitAssociationConfigor;

namespace Obase.Odm.Annotation
{
    /// <summary>
    ///     隐式关联左端标注属性
    ///     仅用于隐式关联的关联端标注
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LeftEndMappingAttribute : MemberAttribute
    {
        /// <summary>
        ///     此端的键属性
        /// </summary>
        private readonly string _keyAttribute;

        /// <summary>
        ///     此端的映射属性
        /// </summary>
        private readonly string _target;

        /// <summary>
        ///     初始化隐式关联左端端映射标注属性
        /// </summary>
        /// <param name="keyAttribute">键属性</param>
        /// <param name="target">映射属性</param>
        public LeftEndMappingAttribute(string keyAttribute, string target)
        {
            _keyAttribute = keyAttribute;
            _target = target;
        }

        /// <summary>
        ///     判定指定的类型成员是否将作为类型元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="elementName">元素名称</param>
        protected internal override bool AsElement(MemberInfo memberInfo, out string elementName)
        {
            elementName = null;
            return true;
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">类型元素配置器</param>
        protected internal override void ConfigurateElement(MemberInfo memberInfo,
            ITypeElementConfigurator configurator)
        {
            if (configurator is IReferenceElementConfigurator referenceElementConfigurator)
                ConfigurateReferenceElement(memberInfo, referenceElementConfigurator);

            if (configurator is IAttributeConfigurator attributeConfigurator)
                ConfigurateAttribute(memberInfo, attributeConfigurator);
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的属性
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">属性配置器</param>
        protected internal override void ConfigurateAttribute(MemberInfo memberInfo,
            IAttributeConfigurator configurator)
        {
            throw new InvalidOperationException("隐式关联左端标注不应调用此方法");
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的引用元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">引用元素配置器</param>
        protected internal override void ConfigurateReferenceElement(MemberInfo memberInfo,
            IReferenceElementConfigurator configurator)
        {
            if (configurator is IAssociationEndConfigurator associationEndConfigurator)
                ConfigurateAssociationEnd(memberInfo, associationEndConfigurator);

            if (configurator is IAssociationReferenceConfigurator associationReferenceConfigurator)
                ConfigurateAssociationReference(memberInfo, associationReferenceConfigurator);
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的关联引用
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">关联引用配置器</param>
        protected internal override void ConfigurateAssociationReference(MemberInfo memberInfo,
            IAssociationReferenceConfigurator configurator)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                //属性为集合类型
                var type = propertyInfo.PropertyType.GetInterface("IEnumerable");
                if (type != null && propertyInfo.PropertyType != typeof(string))
                    //集合元素类型
                    type = propertyInfo.PropertyType.GenericTypeArguments[0];
                else
                    type = propertyInfo.PropertyType;

                //进入所属类型
                if (configurator.Upward() is StructuralTypeConfiguration structuralTypeConfiguration)
                {
                    var modelBuilder = structuralTypeConfiguration.ModelBuilder;
                    //查找隐式关联
                    var endType = new[] { propertyInfo.DeclaringType, type };
                    var endTags = AssociationConfiguratorBuilder.GenerateEndsTag(endType, modelBuilder);
                    var implicitAssociationConfig = modelBuilder.FindImplicitAssociationConfigurationBuilder(endTags);

                    //有隐式关联型配置
                    if (implicitAssociationConfig != null)
                    {
                        var ends = implicitAssociationConfig.EndConfigurations;

                        var leftEnd = ends.FirstOrDefault(p => p.EntityType == propertyInfo.ReflectedType);
                        leftEnd?.HasMapping(_keyAttribute, _target);
                    }
                }
            }
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的关联端
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">关联端配置器</param>
        protected internal override void ConfigurateAssociationEnd(MemberInfo memberInfo,
            IAssociationEndConfigurator configurator)
        {
            throw new InvalidOperationException("隐式关联左端标注不应调用此方法");
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的类型或其所属的元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="typeConfigurator">结构化类型配置器</param>
        protected internal override void ConfigurateType(MemberInfo memberInfo,
            IStructuralTypeConfigurator typeConfigurator)
        {
            //无需配置
        }
    }
}