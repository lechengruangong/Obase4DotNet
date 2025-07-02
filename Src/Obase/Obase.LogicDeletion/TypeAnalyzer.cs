/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：逻辑删除类型解析器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:32:42
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Reflection;
using Obase.Core.Odm.Builder;

namespace Obase.LogicDeletion
{
    /// <summary>
    ///     逻辑删除类型解析器
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
        /// <param name="type">类型</param>
        /// <param name="configurator">对象类型配置器</param>
        private static void ConfigExt(Type type, IObjectTypeConfigurator configurator)
        {
            //类型的标记
            var attrs = type.GetCustomAttributes().ToArray();
            if (attrs.Length > 0)
            {
                var attribute = attrs.LastOrDefault(p =>
                    p is LogicDeletionAttribute);

                if (attribute is LogicDeletionAttribute logicDeletionAttribute)
                {
                    var logicType = typeof(LogicDeletionExtensionConfiguration<>);
                    logicType = logicType.MakeGenericType(type);
                    var ext = configurator.HasExtension(logicType);
                    logicType.GetField("_deletionField", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?.SetValue(ext, logicDeletionAttribute.DeletionField);
                }
            }

            foreach (var propertyInfo in type.GetProperties())
            {
                //类型的标记
                var propAttributes = propertyInfo.GetCustomAttributes().ToArray();

                if (propAttributes.Length > 0)
                {
                    var attribute = propAttributes.LastOrDefault(p =>
                        p is LogicDeletionMarkAttribute);

                    if (attribute is LogicDeletionMarkAttribute)
                    {
                        if (propertyInfo.PropertyType != typeof(bool))
                            throw new ArgumentException("逻辑删除属性必须为bool类型");

                        var logicType = typeof(LogicDeletionExtensionConfiguration<>);
                        logicType = logicType.MakeGenericType(type);
                        var ext = configurator.HasExtension(logicType);
                        logicType.GetField("_deletionMark", BindingFlags.Instance | BindingFlags.NonPublic)
                            ?.SetValue(ext, propertyInfo.Name);
                    }
                }
            }
        }
    }
}