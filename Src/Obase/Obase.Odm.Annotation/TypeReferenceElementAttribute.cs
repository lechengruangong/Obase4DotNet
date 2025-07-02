/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：类型引用元素标注属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:37:16
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Reflection;
using Obase.Core.Odm.Builder;

namespace Obase.Odm.Annotation
{
    /// <summary>
    ///     类型引用元素标注属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class TypeReferenceElementAttribute : MemberAttribute
    {
        /// <summary>
        ///     判定指定的类型成员是否将作为类型元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="elementName">元素名称</param>
        protected internal override bool AsElement(MemberInfo memberInfo, out string elementName)
        {
            elementName = null;
            return this is AssociationReferenceAttribute || this is AssociationEndAttribute;
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
        ///     基于指定的类型成员，配置指定的引用元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">引用元素配置器</param>
        protected internal override void ConfigurateReferenceElement(MemberInfo memberInfo,
            IReferenceElementConfigurator configurator)
        {
            if (configurator is IAssociationReferenceConfigurator referenceConfigurator)
                ConfigurateAssociationReference(memberInfo, referenceConfigurator);

            if (configurator is IAssociationEndConfigurator endConfigurator)
                ConfigurateAssociationEnd(memberInfo, endConfigurator);
        }
    }
}