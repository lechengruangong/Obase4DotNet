/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：显式关联型标注属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:13:57
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Obase.Core.Odm;
using Obase.Core.Odm.Builder;

namespace Obase.Odm.Annotation
{
    /// <summary>
    ///     显式关联端标注属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AssociationEndAttribute : TypeReferenceElementAttribute
    {
        /// <summary>
        ///     基于指定的类型成员，配置指定的属性
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">属性配置器</param>
        protected internal override void ConfigurateAttribute(MemberInfo memberInfo,
            IAttributeConfigurator configurator)
        {
            throw new InvalidOperationException("关联端标注不应调用此方法");
        }


        /// <summary>
        ///     基于指定的类型成员，配置指定的关联引用
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">关联引用配置器</param>
        protected internal override void ConfigurateAssociationReference(MemberInfo memberInfo,
            IAssociationReferenceConfigurator configurator)
        {
            throw new InvalidOperationException("关联端标注不应调用此方法");
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的关联端
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">关联端配置器</param>
        protected internal override void ConfigurateAssociationEnd(MemberInfo memberInfo,
            IAssociationEndConfigurator configurator)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                //默认配置关联端取值器
                if (propertyInfo.GetMethod != null)
                {
                    if (propertyInfo.ReflectedType?.IsValueType == true)
                        configurator.HasValueGetter(propertyInfo);
                    else
                        configurator.HasValueGetter(propertyInfo.GetMethod);
                }

                //默认配置关联端设值器
                if (propertyInfo.SetMethod != null)
                {
                    if (propertyInfo.ReflectedType?.IsValueType == true)
                    {
                        configurator.HasValueSetter(propertyInfo);
                    }
                    else
                    {
                        var settingMode = propertyInfo.PropertyType.GetInterfaces().Any(p => p == typeof(IEnumerable))
                            ? EValueSettingMode.Appending
                            : EValueSettingMode.Assignment;
                        configurator.HasValueSetter(propertyInfo.SetMethod, settingMode);
                    }
                }

                if (configurator is TypeElementConfiguration typeElement)
                    //配置关联端触发器
                    if ((typeElement.BehaviorTriggers.Any(p => p.UniqueId != propertyInfo.Name) ||
                         typeElement.BehaviorTriggers.Count == 0) &&
                        propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsVirtual &&
                        !propertyInfo.GetMethod.IsFinal)
                        //启用了延迟加载才配置触发器
                        if (configurator.EnableLazyLoading)
                        {
                            //默认属性触发器（用以延时加载，访问属性的get的访问器时触发）
                            var trigger = (IBehaviorTrigger)Activator.CreateInstance(
                                typeof(PropertyGetTrigger<,>).MakeGenericType(propertyInfo.DeclaringType,
                                    propertyInfo.PropertyType), propertyInfo);
                            configurator.HasLoadingTrigger(trigger);
                        }
            }
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的类型或其所属的元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="typeConfigurator">结构化类型配置器</param>
        protected internal override void ConfigurateType(MemberInfo memberInfo,
            IStructuralTypeConfigurator typeConfigurator)
        {
            throw new InvalidOperationException("关联端标注不应调用此方法");
        }
    }
}