/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：多租户类型解析器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:02:59
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Reflection;
using Obase.Core.Odm.Builder;

namespace Obase.MultiTenant
{
    /// <summary>
    ///     多租户类型解析器
    /// </summary>
    public class TypeAnalyzer : ITypeAnalyzer
    {
        /// <summary>
        ///     构造类型解析器
        /// </summary>
        /// <param name="next">下一节</param>
        public TypeAnalyzer(ITypeAnalyzer next)
        {
            Next = next;
        }

        /// <summary>
        ///     获取类型解析管道中的下一个解析器。
        /// </summary>
        public ITypeAnalyzer Next { get; }

        /// <summary>
        ///     配置指定的类型。
        /// </summary>
        /// <param name="type">要配置的类型。</param>
        /// <param name="configurator">该类型的配置器。</param>
        public void Configurate(Type type, IStructuralTypeConfigurator configurator)
        {
            if (configurator is IObjectTypeConfigurator objectTypeConfigurator)
                Configurate(type, objectTypeConfigurator);

            //复杂类型无配置
        }

        /// <summary>
        ///     配置指定的对象类型。
        /// </summary>
        /// <param name="type">要配置的对象类型。</param>
        /// <param name="configurator">该对象类型的配置器。</param>
        public void Configurate(Type type, IObjectTypeConfigurator configurator)
        {
            if (configurator is IEntityTypeConfigurator entityTypeConfigurator)
                Configurate(type, entityTypeConfigurator);

            if (configurator is IAssociationTypeConfigurator associationTypeConfigurator)
                Configurate(type, associationTypeConfigurator);
        }

        /// <summary>
        ///     配置指定的实体型。
        /// </summary>
        /// <param name="type">要配置的实体类。</param>
        /// <param name="configurator">该实体型的配置器。</param>
        public void Configurate(Type type, IEntityTypeConfigurator configurator)
        {
            ConfigExt(type, configurator);
        }

        /// <summary>
        ///     配置指定的关联型。
        /// </summary>
        /// <param name="type">要配置的关联型。</param>
        /// <param name="configurator">该关联型的配置器。</param>
        public void Configurate(Type type, IAssociationTypeConfigurator configurator)
        {
            ConfigExt(type, configurator);
        }

        /// <summary>
        ///     配置具体的拓展
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="configurator">对象类型配置器</param>
        private static void ConfigExt(Type type, IObjectTypeConfigurator configurator)
        {
            //类型的标记
            var attrs = type.GetCustomAttributes().ToArray();
            if (attrs.Length > 0)
            {
                var attribute = attrs.LastOrDefault(p =>
                    p is MultiTenantAttribute);

                if (attribute is MultiTenantAttribute multiTenantAttribute)
                {
                    var multiTenantType = typeof(MultiTenantExtensionConfiguration<>);
                    multiTenantType = multiTenantType.MakeGenericType(type);
                    var ext = configurator.HasExtension(multiTenantType);
                    multiTenantType.GetField("_tenantIdField", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?.SetValue(ext, multiTenantAttribute.MultiTenantField);
                    multiTenantType.GetField("_tenantIdType", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?.SetValue(ext, multiTenantAttribute.TenantIdType);
                }
            }

            foreach (var propertyInfo in type.GetProperties())
            {
                //类型的标记
                var propAttributes = propertyInfo.GetCustomAttributes().ToArray();

                if (propAttributes.Length > 0)
                {
                    var attribute = propAttributes.LastOrDefault(p =>
                        p is MultiTenantMarkAttribute);

                    if (attribute is MultiTenantMarkAttribute)
                    {
                        if (propertyInfo.PropertyType != typeof(string) && propertyInfo.PropertyType != typeof(int)
                                                                        && propertyInfo.PropertyType != typeof(long) &&
                                                                        propertyInfo.PropertyType != typeof(Guid))
                            throw new ArgumentException("多租户主键属性必须为string,int,long,Guid类型中的一种");

                        var multiTenant = typeof(MultiTenantExtensionConfiguration<>);
                        multiTenant = multiTenant.MakeGenericType(type);
                        var ext = configurator.HasExtension(multiTenant);

                        multiTenant.GetField("_tenantIdMark", BindingFlags.Instance | BindingFlags.NonPublic)
                            ?.SetValue(ext, propertyInfo.Name);
                        multiTenant.GetField("_tenantIdType", BindingFlags.Instance | BindingFlags.NonPublic)
                            ?.SetValue(ext, propertyInfo.PropertyType);
                    }
                }
            }
        }
    }
}