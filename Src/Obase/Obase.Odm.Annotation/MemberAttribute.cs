/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：成员标注属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:31:08
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Reflection;
using Obase.Core.Odm.Builder;

namespace Obase.Odm.Annotation
{
    /// <summary>
    ///     成员标注属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public abstract class MemberAttribute : Attribute
    {
        /// <summary>
        ///     判定指定的类型成员是否将作为类型元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="elementName">元素名称</param>
        protected internal abstract bool AsElement(MemberInfo memberInfo, out string elementName);

        /// <summary>
        ///     基于指定的类型成员，配置指定的元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">类型元素配置器</param>
        protected internal abstract void ConfigurateElement(MemberInfo memberInfo,
            ITypeElementConfigurator configurator);

        /// <summary>
        ///     基于指定的类型成员，配置指定的属性
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">属性配置器</param>
        protected internal abstract void ConfigurateAttribute(MemberInfo memberInfo,
            IAttributeConfigurator configurator);

        /// <summary>
        ///     基于指定的类型成员，配置指定的引用元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">引用元素配置器</param>
        protected internal abstract void ConfigurateReferenceElement(MemberInfo memberInfo,
            IReferenceElementConfigurator configurator);

        /// <summary>
        ///     基于指定的类型成员，配置指定的关联引用
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">关联引用配置器</param>
        protected internal abstract void ConfigurateAssociationReference(MemberInfo memberInfo,
            IAssociationReferenceConfigurator configurator);

        /// <summary>
        ///     基于指定的类型成员，配置指定的关联端
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">关联端配置器</param>
        protected internal abstract void ConfigurateAssociationEnd(MemberInfo memberInfo,
            IAssociationEndConfigurator configurator);

        /// <summary>
        ///     基于指定的类型成员，配置指定的类型或其所属的元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="typeConfigurator">结构化类型配置器</param>
        protected internal abstract void ConfigurateType(MemberInfo memberInfo,
            IStructuralTypeConfigurator typeConfigurator);
    }
}