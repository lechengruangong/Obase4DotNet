/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：属性标注属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:32:46
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
    ///     属性标注属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TypeAttributeAttribute : TypeElementAttribute
    {
        /// <summary>
        ///     初始化属性标注属性
        /// </summary>
        /// <param name="field">映射字段名,为空字符串则表示与属性名相同</param>
        /// <param name="maxnumber">最大字符数 只有1到255是有效的 如果设置为0 会被设置为255 如果超过255 会被设置为Text字段</param>
        /// <param name="precision">以小数位数表示的精度，0表示小数点后没有位数。精度最大值28</param>
        /// <param name="nullable">指示是否可空。对于主键设置为可空是无效的</param>
        public TypeAttributeAttribute(string field = null, ushort maxnumber = 0, byte precision = 0,
            bool nullable = true)
        {
            Field = field;
            Maxnumber = maxnumber;
            Precision = precision;
            Nullable = nullable;
        }

        /// <summary>
        ///     映射字段名,为空字符串则表示与属性名相同
        /// </summary>
        public string Field { get; }

        /// <summary>
        ///     最大字符数 只有1到255是有效的 如果设置为0 会被设置为255 如果超过255 会被设置为Text字段
        /// </summary>
        public ushort Maxnumber { get; }

        /// <summary>
        ///     以小数位数表示的精度，0表示小数点后没有位数。精度最大值28
        /// </summary>
        public byte Precision { get; }

        /// <summary>
        ///     指示是否可空。对于主键设置为可空是无效的
        /// </summary>
        public bool Nullable { get; }

        /// <summary>
        ///     基于指定的类型成员，配置指定的属性
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">属性配置器</param>
        protected internal override void ConfigurateAttribute(MemberInfo memberInfo,
            IAttributeConfigurator configurator)
        {
            var config = configurator;
            if (memberInfo is PropertyInfo properties)
            {
                //没有配置取值器并且可读还是公开的
                if (properties.GetMethod != null &&
                    (properties.GetMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public)
                {
                    if (properties.DeclaringType != null && properties.DeclaringType.IsValueType)
                        config.HasValueGetter(properties, false);
                    else
                        config.HasValueGetter(properties.GetMethod);
                }

                //没有配置设值器并且可写还是公开的 internal的 protect internal的
                if (properties.SetMethod != null &&
                    ((properties.SetMethod.Attributes & MethodAttributes.Public) == MethodAttributes.Public ||
                     properties.SetMethod.IsAssembly
                     || properties.SetMethod.IsFamilyAndAssembly || properties.SetMethod.IsFamilyOrAssembly))
                {
                    if (properties.ReflectedType?.IsValueType == true)
                    {
                        config.HasValueSetter(properties);
                    }
                    else
                    {
                        var settingMode =
                            properties.PropertyType.GetInterfaces().Any(p => p == typeof(IEnumerable))
                                ? EValueSettingMode.Appending
                                : EValueSettingMode.Assignment;
                        config.HasValueSetter(properties.SetMethod, settingMode);
                    }
                }
            }
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的引用元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">引用元素配置器</param>
        protected internal override void ConfigurateReferenceElement(MemberInfo memberInfo,
            IReferenceElementConfigurator configurator)
        {
            throw new InvalidOperationException("属性标注不应调用此方法");
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的关联引用
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">关联引用配置器</param>
        protected internal override void ConfigurateAssociationReference(MemberInfo memberInfo,
            IAssociationReferenceConfigurator configurator)
        {
            throw new InvalidOperationException("属性标注不应调用此方法");
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的关联端
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">关联端配置器</param>
        protected internal override void ConfigurateAssociationEnd(MemberInfo memberInfo,
            IAssociationEndConfigurator configurator)
        {
            throw new InvalidOperationException("属性标注不应调用此方法");
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的类型或其所属的元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="typeConfigurator">结构化类型配置器</param>
        protected internal override void ConfigurateType(MemberInfo memberInfo,
            IStructuralTypeConfigurator typeConfigurator)
        {
            throw new InvalidOperationException("属性标注不应调用此方法");
        }
    }
}