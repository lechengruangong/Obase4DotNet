/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：显式关联引用标注属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:15:35
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Reflection;
using Obase.Core.Odm;
using Obase.Core.Odm.Builder;

namespace Obase.Odm.Annotation
{
    /// <summary>
    ///     显式关联引用标注属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AssociationReferenceAttribute : TypeReferenceElementAttribute
    {
        /// <summary>
        ///     是否启用延迟加载
        /// </summary>
        private readonly bool _enableLazyLoading;

        /// <summary>
        ///     初始化显式关联引用标注属性
        /// </summary>
        /// <param name="enableLazyLoading">是否启用延迟加载</param>
        public AssociationReferenceAttribute(bool enableLazyLoading = false)
        {
            _enableLazyLoading = enableLazyLoading;
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的属性
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">属性配置器</param>
        protected internal override void ConfigurateAttribute(MemberInfo memberInfo,
            IAttributeConfigurator configurator)
        {
            throw new InvalidOperationException("关联引用标注不应调用此方法");
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的关联引用
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">关联引用配置器</param>
        protected internal override void ConfigurateAssociationReference(MemberInfo memberInfo,
            IAssociationReferenceConfigurator configurator)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                //属性为集合类型
                var type = propertyInfo.PropertyType.GetInterface("IEnumerable");
                if (type != null && propertyInfo.PropertyType != typeof(string))
                    //集合元素类型
                    type = propertyInfo.PropertyType.GenericTypeArguments[0];
                else
                    type = propertyInfo.PropertyType;

                //查找显式关联型的各个属性
                var obviousProps = type.GetProperties();

                var leftEnd = obviousProps.FirstOrDefault(p => p.PropertyType == propertyInfo.ReflectedType)?.Name;
                if (!string.IsNullOrEmpty(leftEnd)) configurator.HasLeftEnd(leftEnd);

                var rightEnd = obviousProps.FirstOrDefault(p => p.PropertyType == type)?.Name;
                if (!string.IsNullOrEmpty(rightEnd)) configurator.HasRightEnd(rightEnd);

                //取值器
                if (propertyInfo.GetMethod != null)
                    configurator.HasValueGetter(propertyInfo);

                //设值器
                if (propertyInfo.SetMethod != null)
                {
                    var actionType = typeof(Action<,>).MakeGenericType(propertyInfo.DeclaringType,
                        propertyInfo.SetMethod.GetParameters()[0].ParameterType);
                    var @delegate = propertyInfo.SetMethod.CreateDelegate(actionType);
                    configurator.HasValueSetter(ValueSetter.Create(@delegate,
                        EValueSettingMode.Assignment));
                }

                //设置是否延迟加载
                configurator.HasEnableLazyLoading(_enableLazyLoading);

                if (configurator is TypeElementConfiguration typeElement)
                    //追加触发器
                    if (typeElement.BehaviorTriggers.Count == 0 && propertyInfo.GetMethod != null &&
                        propertyInfo.GetMethod.IsVirtual && !propertyInfo.GetMethod.IsFinal)
                        //启用了延迟加载才配置触发器
                        if (configurator.EnableLazyLoading)
                            configurator.HasLoadingTrigger(propertyInfo);
            }
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的关联端
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">关联端配置器</param>
        protected internal override void ConfigurateAssociationEnd(MemberInfo memberInfo,
            IAssociationEndConfigurator configurator)
        {
            throw new InvalidOperationException("关联引用标注不应调用此方法");
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的类型或其所属的元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="typeConfigurator">结构化类型配置器</param>
        protected internal override void ConfigurateType(MemberInfo memberInfo,
            IStructuralTypeConfigurator typeConfigurator)
        {
            throw new InvalidOperationException("关联引用标注不应调用此方法");
        }
    }
}