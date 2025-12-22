/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：默认的类型成员解析管道,解析属性访问器为属性,关联引用,关联端等.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 15:16:46
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Obase.Core.Common;
using Obase.Core.Odm.Builder.ImplicitAssociationConfigor;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     默认的类型成员解析管道。
    ///     实现反射建模逻辑
    /// </summary>
    public class DefaultTypeMemberAnalyzer : ITypeMemberAnalyzer
    {
        /// <summary>
        ///     建模器
        /// </summary>
        private readonly ModelBuilder _modelBuilder;

        /// <summary>
        ///     构造默认的类型成员解析管道
        /// </summary>
        /// <param name="builder">建模器</param>
        /// <param name="next">下一节</param>
        public DefaultTypeMemberAnalyzer(ModelBuilder builder, ITypeMemberAnalyzer next)
        {
            _modelBuilder = builder;
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
            if (memberInfo is PropertyInfo propertyInfo)
            {
                //默认的判断条件
                var result = PrimitiveType.IsObasePrimitiveType(propertyInfo.PropertyType);
                name = null;
                return result;
            }

            name = null;
            return false;
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的元素。
        /// </summary>
        /// <param name="memberInfo">要解析的成员。</param>
        /// <param name="configurator">用于配置类型元素的配置器。</param>
        public void Configurate(MemberInfo memberInfo, ITypeElementConfigurator configurator)
        {
            //调用具体的配置方法
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
            var config = configurator;
            if (memberInfo is PropertyInfo properties)
            {
                //映射字段
                configurator.ToField(memberInfo.Name, false);

                //没有配置取值器并且可读还是公开的
                if (properties.GetMethod != null &&
                    (properties.GetMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public)
                {
                    if (properties.ReflectedType != null && properties.ReflectedType.IsValueType)
                        config.HasValueGetter(properties, false);
                    else
                        config.HasValueGetter(properties.GetMethod, false);
                }

                //没有配置设值器并且可写还是公开的 internal的 protect internal的
                if (properties.SetMethod != null &&
                    ((properties.SetMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public ||
                     properties.SetMethod.IsAssembly
                     || properties.SetMethod.IsFamilyAndAssembly || properties.SetMethod.IsFamilyOrAssembly))
                {
                    if (properties.ReflectedType?.IsValueType == true)
                    {
                        config.HasValueSetter(properties, false);
                    }
                    else
                    {
                        var settingMode =
                            properties.PropertyType.GetInterfaces().Any(p => p == typeof(IEnumerable))
                                ? EValueSettingMode.Appending
                                : EValueSettingMode.Assignment;
                        config.HasValueSetter(properties.SetMethod, settingMode, false);
                    }
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
            if (memberInfo is PropertyInfo propertyInfo)
            {
                //属性为集合类型
                Utils.GetIsMultipe(propertyInfo, out var type);
                //是否是元组
                var isTuple = Utils.IsTuple(type);

                //尝试按照显式进行查询
                var obvious = _modelBuilder.FindConfiguration(type);
                //不为空 则查询是否为关联型配置
                if (obvious != null)
                {
                    var obviousAssociationConfig = typeof(AssociationTypeConfiguration<>);
                    obviousAssociationConfig = obviousAssociationConfig.MakeGenericType(type);
                    //目标类型被配置为显式关联型
                    if (obvious.GetType() == obviousAssociationConfig)
                    {
                        var firstType = ((IAssociationTypeConfigurator)obvious).AssociationEnds.FirstOrDefault()
                            ?.EntityType;
                        if (firstType != null &&
                            ((IAssociationTypeConfigurator)obvious).AssociationEnds.All(p => p.EntityType == firstType))
                        {
                            //如果是显式自关联 不配置 需要用户配置
                        }
                        else
                        {
                            //查找显式关联型的各个属性
                            var obviousProps = obvious.ClrType.GetProperties();
                            //只保留与关联端类型相同的属性
                            var endType = ((IAssociationTypeConfigurator)obvious).AssociationEnds
                                .Select(p => p.EntityType).ToList();
                            obviousProps = obviousProps.Where(p => endType.Contains(p.PropertyType)).ToArray();
                            //配置左端右端
                            ConfigLeftAndRight(configurator, obviousProps, propertyInfo);
                        }
                    }
                }

                //没找到显示关联型
                //按照隐式关联型查询 引用的类型是否被配置为实体型
                //不是元组 按照普通的两方关联处理
                var endTypes = Array.Empty<Type>();
                if (!isTuple)
                {
                    //查询属性类型模型配置项
                    var implicitEntityConfig = _modelBuilder.FindConfiguration(type);
                    if (implicitEntityConfig is IEntityTypeConfigurator)
                        //提取关联端
                        endTypes = new[] { propertyInfo.ReflectedType, type };
                }
                //是元组 要分拆为多方关联
                else
                {
                    //如果是元组 取出所有类型参数判断
                    var configs = type.GetGenericArguments().Select(_modelBuilder.FindConfiguration).ToArray();
                    //都是实体型 才进入推断
                    if (configs.All(p => p is IEntityTypeConfigurator))
                    {
                        //加入自己这一端的类型
                        var endTypesList = new List<Type> { propertyInfo.ReflectedType };
                        //取出另外的端类型
                        endTypesList.AddRange(type.GetGenericArguments());
                        //提取关联端
                        endTypes = endTypesList.ToArray();
                    }
                }

                var endTags = AssociationConfiguratorBuilder.GenerateEndsTag(endTypes, _modelBuilder);
                //Tag不是空
                if (!string.IsNullOrEmpty(endTags))
                {
                    //配置隐式关联关联引用
                    //查找隐式关联型建造器
                    var implicitAssociationConfig = _modelBuilder.FindImplicitAssociationConfigurationBuilder(endTags);
                    if (implicitAssociationConfig != null)
                    {
                        var firstType = implicitAssociationConfig.EndConfigurations.FirstOrDefault()?.EntityType;
                        if (firstType != null &&
                            implicitAssociationConfig.EndConfigurations.All(p => p.EntityType == firstType))
                        {
                            //如果是自关联 不配置 需要用户配置
                        }
                        else
                        {
                            //隐式关联的各个属性
                            var implicitsProps = implicitAssociationConfig.AssociationType.GetProperties();
                            //配置左端右端
                            ConfigLeftAndRight(configurator, implicitsProps, propertyInfo);
                        }
                    }
                }

                //取值器
                //没有配置取值器并且可读还是公开的
                if (propertyInfo.GetMethod != null &&
                    (propertyInfo.GetMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public)
                    configurator.HasValueGetter(propertyInfo, false);

                //设值器
                //没有配置设值器并且可写还是公开的 internal的 protect internal的
                if (propertyInfo.SetMethod != null &&
                    ((propertyInfo.SetMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public ||
                     propertyInfo.SetMethod.IsAssembly
                     || propertyInfo.SetMethod.IsFamilyAndAssembly || propertyInfo.SetMethod.IsFamilyOrAssembly))
                    configurator.HasValueSetter(propertyInfo, false);

                if (configurator is TypeElementConfiguration typeElement)
                    //追加触发器
                    if ((typeElement.BehaviorTriggers.Any(p => p.UniqueId != propertyInfo.Name) ||
                         typeElement.BehaviorTriggers.Count == 0) &&
                        propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsVirtual && !propertyInfo.GetMethod.IsFinal)
                        //启用了延迟加载才配置触发器
                        if (configurator.EnableLazyLoading)
                            configurator.HasLoadingTrigger(propertyInfo, false);
            }
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的关联端。
        /// </summary>
        /// <param name="memberInfo">要解析的成员。</param>
        /// <param name="configurator">用于配置关联端的配置器。</param>
        public void Configurate(MemberInfo memberInfo, IAssociationEndConfigurator configurator)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                //取值器
                if (propertyInfo.GetMethod != null)
                    configurator.HasValueGetter(propertyInfo, false);
                //设值器
                if (propertyInfo.SetMethod != null)
                    configurator.HasValueSetter(propertyInfo, false);

                if (configurator is TypeElementConfiguration typeElement)
                    //配置关联端触发器
                    if ((typeElement.BehaviorTriggers.Any(p => p.UniqueId != propertyInfo.Name) ||
                         typeElement.BehaviorTriggers.Count == 0) && 
                        propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsVirtual && !propertyInfo.GetMethod.IsFinal)
                        //启用了延迟加载才配置触发器
                        if (configurator.EnableLazyLoading)
                        {
                            //默认属性触发器（用以延时加载，访问属性的get的访问器时触发）
                            var trigger = (IBehaviorTrigger)Activator.CreateInstance(
                                typeof(PropertyGetTrigger<,>).MakeGenericType(propertyInfo.ReflectedType,
                                    propertyInfo.PropertyType), propertyInfo);
                            configurator.HasLoadingTrigger(trigger, false);
                        }

                if (!(_modelBuilder.FindConfiguration(configurator.EntityType) is IEntityTypeConfigurator
                        entityTypeConfigurator)) throw new ArgumentException($"{configurator.EntityType}未配置为实体型");
                //进入宿主 关联型
                if (configurator.Upward() is IAssociationTypeConfigurator associationTypeConfigurator)
                {
                    //处理每一个键
                    var keyAttrs = entityTypeConfigurator.GetKeyAttributesFiled();

                    foreach (var attr in keyAttrs)
                    {
                        string targetField;
                        //不在同一映射表
                        if (entityTypeConfigurator.TargetTable != associationTypeConfigurator.TargetTable)
                        {
                            //主键参考列表
                            var list = new List<string> { "code", "id" };
                            //自身或者是类名加自身
                            targetField = list.Contains(attr.ToLower())
                                ? $"{configurator.EntityType.Name}{attr}"
                                : attr;
                        }
                        //在同一映射表
                        else
                        {
                            targetField = attr;
                        }

                        configurator.HasMapping(attr, targetField, false);
                    }
                }
            }
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的类型或其所属的元素。
        /// </summary>
        /// <param name="memberInfo">要解析的成员。</param>
        /// <param name="typeConfigurator">当前类型的配置器。</param>
        public void Configurate(MemberInfo memberInfo, IStructuralTypeConfigurator typeConfigurator)
        {
            //默认配置器无作为其他元素进行配置
        }

        /// <summary>
        ///     配置左端和右端
        /// </summary>
        /// <param name="configurator">关联引用配置器</param>
        /// <param name="props">关联型属性集合</param>
        /// <param name="propertyInfo">当前要配置的属性</param>
        private void ConfigLeftAndRight(IAssociationReferenceConfigurator configurator, PropertyInfo[] props,
            PropertyInfo propertyInfo)
        {
            //与当前属性所在类的类型相同 推断为左端
            var leftEnd = props.FirstOrDefault(p => p.PropertyType == propertyInfo.ReflectedType)?.Name;
            if (!string.IsNullOrEmpty(leftEnd)) configurator.HasLeftEnd(leftEnd, false);
            //另外一端 推断为右端
            var rightEnd = props.FirstOrDefault(p => p.Name != leftEnd)?.Name;
            if (!string.IsNullOrEmpty(rightEnd)) configurator.HasRightEnd(rightEnd, false);
        }
    }
}