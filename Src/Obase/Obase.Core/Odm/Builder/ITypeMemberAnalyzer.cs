/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：反射建模过程中解析类型成员接口,提供解析类型成员的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 12:18:33
└──────────────────────────────────────────────────────────────┘
*/

using System.Reflection;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     定义在反射建模过程中解析类型成员的规范。
    /// </summary>
    public interface ITypeMemberAnalyzer
    {
        /// <summary>
        ///     获取类型成员解析管道中的下一个解析器。
        /// </summary>
        ITypeMemberAnalyzer Next { get; }

        /// <summary>
        ///     判定指定的类型成员是否将作为类型元素。
        /// </summary>
        /// <param name="memberInfo">类型成员。</param>
        /// <param name="name">如果作为元素，返回元素名称。</param>
        bool AsElement(MemberInfo memberInfo, out string name);

        /// <summary>
        ///     基于指定的类型成员，配置指定的元素。
        /// </summary>
        /// <param name="memberInfo">要解析的成员。</param>
        /// <param name="configurator">用于配置类型元素的配置器。</param>
        void Configurate(MemberInfo memberInfo, ITypeElementConfigurator configurator);

        /// <summary>
        ///     基于指定的类型成员，配置指定的属性。
        /// </summary>
        /// <param name="memberInfo">要解析的成员。</param>
        /// <param name="configurator">用于配置属性的配置器。</param>
        void Configurate(MemberInfo memberInfo, IAttributeConfigurator configurator);

        /// <summary>
        ///     基于指定的类型成员，配置指定的引用元素。
        /// </summary>
        /// <param name="memberInfo">要解析的成员。</param>
        /// <param name="configurator">用于配置引用元素的配置器。</param>
        void Configurate(MemberInfo memberInfo, IReferenceElementConfigurator configurator);

        /// <summary>
        ///     基于指定的类型成员，配置指定的关联引用。
        /// </summary>
        /// <param name="memberInfo">要解析的成员。</param>
        /// <param name="configurator">用于配置关联引用的配置器。</param>
        void Configurate(MemberInfo memberInfo, IAssociationReferenceConfigurator configurator);

        /// <summary>
        ///     基于指定的类型成员，配置指定的关联端。
        /// </summary>
        /// <param name="memberInfo">要解析的成员。</param>
        /// <param name="configurator">用于配置关联端的配置器。</param>
        void Configurate(MemberInfo memberInfo, IAssociationEndConfigurator configurator);

        /// <summary>
        ///     基于指定的类型成员，配置指定的类型或其所属的元素。
        /// </summary>
        /// <param name="memberInfo">要解析的成员。</param>
        /// <param name="typeConfigurator">当前类型的配置器。</param>
        void Configurate(MemberInfo memberInfo, IStructuralTypeConfigurator typeConfigurator);
    }
}