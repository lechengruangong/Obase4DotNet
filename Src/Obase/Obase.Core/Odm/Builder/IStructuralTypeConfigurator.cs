/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：定义配置类型的规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 14:35:16
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Reflection;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     定义配置类型的规范
    /// </summary>
    public interface IStructuralTypeConfigurator
    {
        /// <summary>
        ///     继承自谁
        /// </summary>
        Type DerivedFrom { get; }

        /// <summary>
        ///     启动一个属性配置项，如果要启动的实体型配置项未创建则新建一个。
        /// </summary>
        /// <param name="name">属性名称，它将作为配置项的键</param>
        /// <param name="dataType">属性的数据类型。</param>
        IAttributeConfigurator Attribute(string name, Type dataType);

        /// <summary>
        ///     启动一个属性配置项，如果要启动的实体型配置项未创建则新建一个。
        ///     类型参数：
        ///     TAttribute    属性的数据类型。
        /// </summary>
        /// <param name="name">属性名称，它将作为配置项的键</param>
        IAttributeConfigurator Attribute<TAttribute>(string name) where TAttribute : struct;

        /// <summary>
        ///     指定当前类型的基类型。
        /// </summary>
        /// <param name="type">基类型。</param>
        void DeriveFrom(Type type);

        /// <summary>
        ///     指定当前类型的基类型。
        ///     类型参数
        ///     TDerived
        ///     基类型。
        /// </summary>
        void DeriveFrom<TDerived>();

        /// <summary>
        ///     根据名称获取元素配置器。
        /// </summary>
        /// <param name="name">元素名称。</param>
        ITypeElementConfigurator GetElement(string name);

        /// <summary>
        ///     使用一个构造函数为类型创建实例构造器。
        /// </summary>
        /// <param name="constructorInfo">构造函数。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        IParameterConfigurator HasConstructor(ConstructorInfo constructorInfo, bool overrided = true);

        /// <summary>
        ///     设置类型的实例构造器。
        /// </summary>
        /// <param name="constructor">实例构造器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasConstructor(IInstanceConstructor constructor, bool overrided = true);

        /// <summary>
        ///     为类型配置项设置一个扩展配置器。
        /// </summary>
        /// <param name="configType">扩展配置器的类型，须继承自TypeExtensionConfiguration。</param>
        TypeExtensionConfiguration HasExtension(Type configType);

        /// <summary>
        ///     为类型配置项设置一个扩展配置器
        /// </summary>
        /// <typeparam name="TExtensionConfiguration">扩展配置器的类型，须继承自TypeExtensionConfiguration。</typeparam>
        /// <returns></returns>
        TypeExtensionConfiguration HasExtension<TExtensionConfiguration>()
            where TExtensionConfiguration : TypeExtensionConfiguration, new();

        /// <summary>
        ///     设置类型的命名空间。
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void HasNamespace(string nameSpace, bool overrided = true);
    }
}