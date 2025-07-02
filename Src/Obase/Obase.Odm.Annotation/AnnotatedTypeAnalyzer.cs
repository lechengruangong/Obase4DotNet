/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：标注类型分析器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:10:23
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Reflection;
using Obase.Core.Odm.Builder;

namespace Obase.Odm.Annotation
{
    /// <summary>
    ///     标注类型分析器
    /// </summary>
    public class AnnotatedTypeAnalyzer : ITypeAnalyzer
    {
        /// <summary>
        ///     初始化标注类型分析器
        /// </summary>
        /// <param name="next">下一节</param>
        public AnnotatedTypeAnalyzer(ITypeAnalyzer next)
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
            else
                //复杂类型 配置构造函数
                ConfigConstructor(type, configurator);
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
            //类型的标记
            var attrs = type.GetCustomAttributes().ToArray();
            if (attrs.Length > 0)
            {
                var attribute = attrs.LastOrDefault(p =>
                    p is EntityAttribute);
                if (attribute == null)
                    return;
                //配置实体型
                if (attribute is EntityAttribute entityAttribute)
                {
                    //主键设置
                    if (entityAttribute.KeyAttributes != null && entityAttribute.KeyAttributes.Length > 0)
                        foreach (var keyAttribute in entityAttribute.KeyAttributes)
                            configurator.HasKeyAttribute(keyAttribute);
                    //是否自增
                    configurator.HasKeyIsSelfIncreased(entityAttribute.IsSelfIncrease);
                    //表名
                    configurator.ToTable(string.IsNullOrEmpty(entityAttribute.TableName)
                        ? type.Name
                        : entityAttribute.TableName);
                    //配置构造函数
                    ConfigConstructor(type, configurator);
                }
            }
        }

        /// <summary>
        ///     配置指定的关联型。
        /// </summary>
        /// <param name="type">要配置的关联型。</param>
        /// <param name="configurator">该关联型的配置器。</param>
        public void Configurate(Type type, IAssociationTypeConfigurator configurator)
        {
            //类型的标记
            var attrs = type.GetCustomAttributes().ToArray();
            if (attrs.Length > 0)
            {
                var attribute = attrs.LastOrDefault(p =>
                    p is AssociationAttribute);
                if (attribute == null)
                    return;
                //配置显式关联型
                if (attribute is AssociationAttribute associationAttribute)
                {
                    //表名
                    configurator.ToTable(string.IsNullOrEmpty(associationAttribute.TableName)
                        ? type.Name
                        : associationAttribute.TableName);
                    //配置构造函数
                    ConfigConstructor(type, configurator);
                }
            }

            //是否是隐式关联型
            if (typeof(ImplicitAssociation).IsAssignableFrom(type))
            {
                var prop = type.GetProperty("End1");
                ConfigTableName(configurator, prop);
                prop = type.GetProperty("End2");
                ConfigTableName(configurator, prop);
            }
        }

        /// <summary>
        ///     配置表名
        /// </summary>
        /// <param name="configurator">关联型配置器</param>
        /// <param name="prop">要配置的属性</param>
        private void ConfigTableName(IAssociationTypeConfigurator configurator, PropertyInfo prop)
        {
            if (prop != null)
            {
                var propType = prop.PropertyType;
                var end1Props = propType.GetProperties();
                {
                    foreach (var propertyInfo in end1Props)
                    {
                        //类型的标记
                        var end1Attributes = propertyInfo.GetCustomAttributes().ToArray();
                        if (end1Attributes.Length > 0)
                        {
                            var attribute = end1Attributes.LastOrDefault(p =>
                                p is ImplicitAssociationAttribute);
                            if (attribute == null)
                                return;
                            //配置隐式关联型
                            if (attribute is ImplicitAssociationAttribute implicitAssociation)
                                //表名
                                configurator.ToTable(string.IsNullOrEmpty(implicitAssociation.Target)
                                    ? propType.Name
                                    : implicitAssociation.Target);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     配置构造函数
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <param name="configurator">结构化类型配置器</param>
        private void ConfigConstructor(Type type, IStructuralTypeConfigurator configurator)
        {
            //尝试寻找所有构造函数
            var constructors =
                type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (constructors.Length > 0)
                foreach (var constructor in constructors)
                {
                    //如果被标记为构造函数
                    var constructorAttr = constructor.GetCustomAttribute<ConstructorAttribute>();
                    if (constructorAttr != null)
                    {
                        //配置构造函数
                        var parameterConfigurator = configurator.HasConstructor(constructor);

                        var parameters = constructorAttr.PropNames;

                        if (parameters.Length > 0)
                        {
                            if (parameters.Length != constructor.GetParameters().Length)
                                throw new ArgumentException("构造函数标记ConstructorAttribute的参数数量与所标记的构造函数不符");

                            foreach (var parameter in parameters)
                            {
                                if (type.GetProperty(parameter) == null ||
                                    type.GetProperty(parameter)?.GetMethod == null)
                                    throw new ArgumentException(
                                        $"构造函数标记ConstructorAttribute的参数{parameter}无法找到对应的可用的读属性访问器");
                                parameterConfigurator.Map(parameter);
                            }

                            parameterConfigurator.End();
                        }
                        else
                        {
                            if (constructor.GetParameters().Length != 0)
                                throw new ArgumentException("构造函数标记ConstructorAttribute的参数数量与所标记的构造函数不符");
                            parameterConfigurator.End();
                        }
                    }
                }
        }
    }
}