/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：标注成员解析器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:08:57
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Reflection;
using Obase.Core.Odm.Builder;

namespace Obase.Odm.Annotation
{
    /// <summary>
    ///     标注成员解析器
    /// </summary>
    public class AnnotatedMemberAnalyzer : ITypeMemberAnalyzer
    {
        /// <summary>
        ///     构造标注成员解析器
        /// </summary>
        /// <param name="next">下一节</param>
        public AnnotatedMemberAnalyzer(ITypeMemberAnalyzer next)
        {
            Next = next;
        }


        /// <summary>
        ///     获取类型成员解析管道中的下一个解析器。
        /// </summary>
        public ITypeMemberAnalyzer Next { get; }

        /// <summary>
        ///     判定指定的类型成员是否将作为类型元素。
        /// </summary>
        /// <param name="memberInfo">类型成员。</param>
        /// <param name="name">如果作为元素，返回元素名称。</param>
        public bool AsElement(MemberInfo memberInfo, out string name)
        {
            name = null;
            var attrs = memberInfo.GetCustomAttributes(typeof(MemberAttribute)).ToArray();
            if (attrs.Length > 0)
            {
                var result = false;

                var typeAttr = attrs.Cast<MemberAttribute>().ToArray();
                foreach (var attribute in typeAttr) result |= attribute.AsElement(memberInfo, out name);
                return result;
            }

            return false;
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的元素。
        /// </summary>
        /// <param name="memberInfo">要解析的成员。</param>
        /// <param name="configurator">用于配置类型元素的配置器。</param>
        public void Configurate(MemberInfo memberInfo, ITypeElementConfigurator configurator)
        {
            if (configurator is IReferenceElementConfigurator referenceElementConfigurator)
                Configurate(memberInfo, referenceElementConfigurator);

            if (configurator is IAttributeConfigurator attributeConfigurator)
                Configurate(memberInfo, attributeConfigurator);
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的属性。
        /// </summary>
        /// <param name="memberInfo">要解析的成员。</param>
        /// <param name="configurator">用于配置属性的配置器。</param>
        public void Configurate(MemberInfo memberInfo, IAttributeConfigurator configurator)
        {
            //调用TypeAttributeAttribute配置
            var attrs = memberInfo.GetCustomAttributes(typeof(MemberAttribute)).ToArray();
            if (attrs.Length > 0)
            {
                if (attrs.Length > 1)
                    throw new InvalidOperationException(
                        $"{memberInfo.DeclaringType?.FullName}.{memberInfo.Name}标注错误:不能将类型属性TypeAttribute与其他标注一同使用");

                var typeAttr = attrs.LastOrDefault(p => p is TypeAttributeAttribute);
                if (typeAttr is TypeAttributeAttribute typeAttribute)
                {
                    typeAttribute.ConfigurateAttribute(memberInfo, configurator);
                    if (!string.IsNullOrEmpty(typeAttribute.Field))
                        configurator.ToField(typeAttribute.Field);
                    if (typeAttribute.Maxnumber > 0)
                        configurator.HasMaxcharNumber(typeAttribute.Maxnumber);
                    if (typeAttribute.Precision > 0)
                        configurator.HasPrecision(typeAttribute.Precision);
                    configurator.HasNullable(typeAttribute.Nullable);
                }
            }
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的引用元素。
        /// </summary>
        /// <param name="memberInfo">要解析的成员。</param>
        /// <param name="configurator">用于配置引用元素的配置器。</param>
        public void Configurate(MemberInfo memberInfo, IReferenceElementConfigurator configurator)
        {
            if (configurator is IAssociationEndConfigurator associationEndConfigurator)
                Configurate(memberInfo, associationEndConfigurator);

            if (configurator is IAssociationReferenceConfigurator associationReferenceConfigurator)
                Configurate(memberInfo, associationReferenceConfigurator);
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的关联引用。
        /// </summary>
        /// <param name="memberInfo">要解析的成员。</param>
        /// <param name="configurator">用于配置关联引用的配置器。</param>
        public void Configurate(MemberInfo memberInfo, IAssociationReferenceConfigurator configurator)
        {
            //调用AssociationReferenceAttribute配置
            var attrs = memberInfo.GetCustomAttributes(typeof(MemberAttribute)).ToArray();
            if (attrs.Length > 0)
            {
                if (attrs.Count(p => p is AssociationReferenceAttribute || p is ImplicitAssociationAttribute) > 1)
                    throw new InvalidOperationException(
                        $"{memberInfo.DeclaringType?.FullName}.{memberInfo.Name}标注错误:不能同时标注为显式关联引用AssociationReference和隐式关联ImplicitAssociation");

                if (attrs.Count(p => p is AssociationReferenceAttribute) == 1 && attrs.Length > 1)
                    throw new InvalidOperationException(
                        $"{memberInfo.DeclaringType?.FullName}.{memberInfo.Name}标注错误:不能将显式关联引用AssociationReference与其他标注一同使用");

                if (attrs.Count(p => p is ImplicitAssociationAttribute) == 1 &&
                    attrs.Count(p => p is LeftEndMappingAttribute || p is RightEndMappingAttribute) != 2)
                    throw new InvalidOperationException(
                        $"{memberInfo.DeclaringType?.FullName}.{memberInfo.Name}标注错误:隐式关联ImplicitAssociation标注需要和隐式关联左端LeftEndMapping,隐式关联右端标注RightEndMapping一起标注");

                if (attrs.Count(p => p is ImplicitAssociationAttribute) == 1 && attrs.Length > 3)
                    throw new InvalidOperationException(
                        $"{memberInfo.DeclaringType?.FullName}.{memberInfo.Name}标注错误:不能将隐式关联引用AssociationReference与除隐式关联左端LeftEndMapping,隐式关联右端标注RightEndMapping的其他标注一同使用");

                //配置引用
                var typeAttr = attrs.LastOrDefault(p => p is AssociationReferenceAttribute);
                ((AssociationReferenceAttribute)typeAttr)?.ConfigurateAssociationReference(memberInfo, configurator);
                typeAttr = attrs.LastOrDefault(p => p is ImplicitAssociationAttribute);
                ((ImplicitAssociationAttribute)typeAttr)?.ConfigurateAssociationReference(memberInfo, configurator);
                //配置映射
                typeAttr = attrs.LastOrDefault(p => p is LeftEndMappingAttribute);
                ((LeftEndMappingAttribute)typeAttr)?.ConfigurateAssociationReference(memberInfo, configurator);
                typeAttr = attrs.LastOrDefault(p => p is RightEndMappingAttribute);
                ((RightEndMappingAttribute)typeAttr)?.ConfigurateAssociationReference(memberInfo, configurator);
            }
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的关联端。
        /// </summary>
        /// <param name="memberInfo">要解析的成员。</param>
        /// <param name="configurator">用于配置关联端的配置器。</param>
        public void Configurate(MemberInfo memberInfo, IAssociationEndConfigurator configurator)
        {
            //调用AssociationEndAttribute配置
            var attrs = memberInfo.GetCustomAttributes(typeof(MemberAttribute)).ToArray();
            if (attrs.Length > 0)
            {
                if (attrs.Count(p => p is AssociationEndAttribute || p is EndMappingAttribute) != 2)
                    throw new InvalidOperationException(
                        $"{memberInfo.DeclaringType?.FullName}.{memberInfo.Name}标注错误:显式关联端AssociationEnd标注需要和显式关联映射EndMapping标注一起标注");

                if (attrs.Count(p => p is AssociationEndAttribute || p is EndMappingAttribute) == 2 && attrs.Length > 2)
                    throw new InvalidOperationException(
                        $"{memberInfo.DeclaringType?.FullName}.{memberInfo.Name}标注错误:不能将显式关联端AssociationEnd标注和显式关联映射EndMapping与其他标注一同使用");

                if (attrs.Count(p => p is ImplicitAssociationAttribute) == 1
                    && attrs.Count(p => p is LeftEndMappingAttribute || p is RightEndMappingAttribute) != 2)
                    throw new InvalidOperationException(
                        $"{memberInfo.DeclaringType?.FullName}.{memberInfo.Name}标注错误:隐式关联左端LeftEndMapping需要和隐式关联右端标注RightEndMapping一起标注");

                if (attrs.Count(p =>
                        p is ImplicitAssociationAttribute || p is LeftEndMappingAttribute ||
                        p is RightEndMappingAttribute) == 3 && attrs.Length > 3)
                    throw new InvalidOperationException(
                        $"{memberInfo.DeclaringType?.FullName}.{memberInfo.Name}标注错误:不能将隐式关联左端LeftEndMapping和隐式关联右端标注RightEndMapping与其他标注一同使用");

                var typeAttr = attrs.LastOrDefault(p => p is AssociationEndAttribute);
                ((AssociationEndAttribute)typeAttr)?.ConfigurateAssociationEnd(memberInfo, configurator);
                typeAttr = attrs.LastOrDefault(p => p is EndMappingAttribute);
                ((EndMappingAttribute)typeAttr)?.ConfigurateAssociationEnd(memberInfo, configurator);
            }
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的类型或其所属的元素。
        /// </summary>
        /// <param name="memberInfo">要解析的成员。</param>
        /// <param name="typeConfigurator">当前类型的配置器。</param>
        public void Configurate(MemberInfo memberInfo, IStructuralTypeConfigurator typeConfigurator)
        {
            //无此类调用
        }
    }
}