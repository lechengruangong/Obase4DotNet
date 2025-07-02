/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：隐式关联标注属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:23:41
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using System.Reflection;
using Obase.Core.Odm;
using Obase.Core.Odm.Builder;
using Obase.Core.Odm.Builder.ImplicitAssociationConfigor;

namespace Obase.Odm.Annotation
{
    /// <summary>
    ///     隐式关联标注属性
    ///     仅用于隐式关联的关联标注
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ImplicitAssociationAttribute : MemberAttribute
    {
        /// <summary>
        ///     是否启用延迟加载
        /// </summary>
        private readonly bool _enableLazyLoading;

        /// <summary>
        ///     映射表名
        /// </summary>
        private readonly string _target;

        /// <summary>
        ///     初始化隐式关联端标注属性
        /// </summary>
        /// <param name="target">映射表名</param>
        /// <param name="enableLazyLoading">是否启用延迟加载</param>
        public ImplicitAssociationAttribute(string target, bool enableLazyLoading = false)
        {
            if (string.IsNullOrEmpty(target))
                throw new ArgumentException("隐式关联标注必须指定映射表名", nameof(target));
            _target = target;
            _enableLazyLoading = enableLazyLoading;
        }

        /// <summary>
        ///     映射表名
        /// </summary>
        public string Target => _target.Replace(" ", "");

        /// <summary>
        ///     判定指定的类型成员是否将作为类型元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="elementName">元素名称</param>
        protected internal override bool AsElement(MemberInfo memberInfo, out string elementName)
        {
            elementName = null;
            return true;
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">类型元素配置器</param>
        protected internal override void ConfigurateElement(MemberInfo memberInfo,
            ITypeElementConfigurator configurator)
        {
            if (configurator is IReferenceElementConfigurator referenceElementConfigurator)
                ConfigurateReferenceElement(memberInfo, referenceElementConfigurator);

            if (configurator is IAttributeConfigurator attributeConfigurator)
                ConfigurateAttribute(memberInfo, attributeConfigurator);
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的属性
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">属性配置器</param>
        protected internal override void ConfigurateAttribute(MemberInfo memberInfo,
            IAttributeConfigurator configurator)
        {
            throw new InvalidOperationException("隐式关联标注不应调用此方法");
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的引用元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="configurator">引用元素配置器</param>
        protected internal override void ConfigurateReferenceElement(MemberInfo memberInfo,
            IReferenceElementConfigurator configurator)
        {
            if (configurator is IAssociationEndConfigurator associationEndConfigurator)
                ConfigurateAssociationEnd(memberInfo, associationEndConfigurator);

            if (configurator is IAssociationReferenceConfigurator associationReferenceConfigurator)
                ConfigurateAssociationReference(memberInfo, associationReferenceConfigurator);
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

                //进入所属类型
                if (configurator.Upward() is StructuralTypeConfiguration structuralTypeConfiguration)
                {
                    var modelBuilder = structuralTypeConfiguration.ModelBuilder;
                    //查找隐式关联
                    var endType = new[] { propertyInfo.DeclaringType, type };
                    var endTags = AssociationConfiguratorBuilder.GenerateEndsTag(endType,
                        modelBuilder);
                    var implicitAssociationConfig = modelBuilder.FindImplicitAssociationConfigurationBuilder(endTags);

                    //有隐式关联型配置
                    if (implicitAssociationConfig != null)
                    {
                        var ends = implicitAssociationConfig.EndConfigurations;

                        var leftEnd = ends.FirstOrDefault(p => p.EntityType == propertyInfo.ReflectedType)
                            ?.Name;
                        if (!string.IsNullOrEmpty(leftEnd)) configurator.HasLeftEnd(leftEnd);

                        var rightEnd = ends.FirstOrDefault(p => p.EntityType == type)?.Name;
                        if (!string.IsNullOrEmpty(rightEnd)) configurator.HasRightEnd(rightEnd);
                    }
                }

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
            throw new InvalidOperationException("隐式关联标注不应调用此方法");
        }

        /// <summary>
        ///     基于指定的类型成员，配置指定的类型或其所属的元素
        /// </summary>
        /// <param name="memberInfo">成员</param>
        /// <param name="typeConfigurator">结构化类型配置器</param>
        protected internal override void ConfigurateType(MemberInfo memberInfo,
            IStructuralTypeConfigurator typeConfigurator)
        {
            //无需配置
        }
    }
}