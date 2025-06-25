/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：适用于隐式关联的关联端配置器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 16:06:21
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Obase.Core.Common;

namespace Obase.Core.Odm.Builder.ImplicitAssociationConfigor
{
    /// <summary>
    ///     适用于隐式关联的关联端配置器
    /// </summary>
    public abstract class AssociationEndConfiguration : TypeElementConfiguration, IAssociationEndConfigurator
    {
        /// <summary>
        ///     行为触发器
        /// </summary>
        protected readonly List<IBehaviorTrigger> _behaviorTriggers = new List<IBehaviorTrigger>();

        /// <summary>
        ///     反射建模加入的映射
        /// </summary>
        private readonly HashSet<string> _reflectAddedMapping = new HashSet<string>();

        /// <summary>
        ///     关联端映射集合
        /// </summary>
        protected readonly List<AssociationEndMapping> Mappings = new List<AssociationEndMapping>();

        /// <summary>
        ///     是否启用延迟加载
        /// </summary>
        protected bool _enableLazyLoading;

        /// <summary>
        ///     关联端在关联型上的索引号（从1开始计数）。
        /// </summary>
        protected byte _endIndex;

        /// <summary>
        ///     关联端的实体Clr类型
        /// </summary>
        protected Type _entityType;

        /// <summary>
        ///     指示当前关联端是否为聚合关联端。
        /// </summary>
        protected bool _isAggregated;

        /// <summary>
        ///     指示当前关联端是否作为伴随端。
        /// </summary>
        private bool _isCompanionEnd;

        /// <summary>
        ///     获取该关联端上基于当前关联定义的关联引用
        /// </summary>
        protected IAssociationReferenceConfigurator _referenceConfigurator;

        /// <summary>
        ///     关联配置器建造器，用于建造隐式关联配置器。
        /// </summary>
        protected AssociationConfiguratorBuilder AssociationConfiguratorBuilder;

        /// <summary>
        ///     指示是否把关联端对象默认视为新对象。当该属性为true时，如果关联端对象未被显式附加到上下文，该对象将被视为新对象实施持久化。
        /// </summary>
        protected bool DefaultAsNew;

        /// <summary>
        ///     指定关联或关联端的加载优先级，数值小者先加载。
        /// </summary>
        protected int LoadingPriority;

        /// <summary>
        ///     获取元素类型。
        /// </summary>
        public override EElementType ElementType => EElementType.AssociationEnd;

        /// <summary>
        ///     指示当前关联端是否作为伴随端。
        /// </summary>
        public bool IsCompanionEnd => _isCompanionEnd;

        /// <summary>
        ///     关联端在关联型上的索引号（从1开始计数）。
        /// </summary>
        public byte EndIndex => _endIndex;

        /// <summary>
        ///     关联端的实体Clr类型
        /// </summary>
        public Type EntityType => _entityType;

        /// <summary>
        ///     获取该关联端上基于当前关联定义的关联引用。
        /// </summary>
        public IAssociationReferenceConfigurator ReferenceConfigurator => _referenceConfigurator;

        /// <summary>
        ///     设置一个值，该值指示是否把关联端对象默认视为新对象。当该属性为true时，如果关联端对象未被显式附加到上下文，该对象将被视为新对象实施持久化。
        /// </summary>
        /// <param name="defaultAsNew">指示是否把关联端对象默认视为新对象。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAssociationEndConfigurator.HasDefaultAsNew(bool defaultAsNew, bool overrided)
        {
            //覆盖的 覆盖值
            if (overrided)
                DefaultAsNew = defaultAsNew;
        }

        /// <summary>
        ///     设置一个值，该值指示当前关联端是否为聚合关联端。
        /// </summary>
        /// <param name="isAggregated">指示当前关联端是否为聚合关联端。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void IsAggregated(bool isAggregated, bool overrided)
        {
            //覆盖的 覆盖值
            if (overrided)
                _isAggregated = isAggregated;
        }

        /// <summary>
        ///     配置关联端映射
        /// </summary>
        /// <param name="keyAttribute">关联端标识属性的名称。</param>
        /// <param name="targetField">上述标识属性的映射字段。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void IAssociationEndConfigurator.HasMapping(string keyAttribute, string targetField, bool overrided)
        {
            if (overrided) Mappings.Clear();
            var keys = $"{keyAttribute}/{targetField}";
            //没有任何映射 直接加入
            if (Mappings.Count == 0)
            {
                Mappings.Add(new AssociationEndMapping { KeyAttribute = keyAttribute, TargetField = targetField });
                //记录一下 是由反射加入的
                _reflectAddedMapping.Add(keys);
            }
            //已有映射
            else
            {
                //当前Mapping内的所有映射
                var exKeys = Mappings.Select(p => $"{p.KeyAttribute}/{p.TargetField}").OrderBy(p => p).ToArray();
                var flag = _reflectAddedMapping.OrderBy(p => p).SequenceEqual(exKeys);
                //如果由反射加入的集合与当前Mapping集合一一对应
                if (flag)
                {
                    //就可以加入
                    HasMapping(keyAttribute, targetField);
                    //记录一下 是由反射加入的
                    _reflectAddedMapping.Add(keys);
                }
                //否则 不加入 因为当前Mapping内是由其他方式加入的 不可以覆盖
            }
        }

        /// <summary>
        ///     指示是否将当前关联端作为伴随端。
        ///     说明
        ///     设置当前端为伴随端会将之前设置的伴随端改设不作为伴随端。
        ///     当override为false时，其它端只要任意一端已设置为伴随端，本方法就不再执行设置操作。
        /// </summary>
        /// <param name="value">指示是否作为伴随端。</param>
        /// <param name="overrided">指示是否覆盖既有设置。</param>
        public void AsCompanion(bool value, bool overrided)
        {
            //覆盖的 直接覆盖
            if (overrided)
            {
                AsCompanion(value);
            }
            else
            {
                //如果任意一个端都不是伴随
                var endConfigs = AssociationConfiguratorBuilder.EndConfigurations;
                if (!endConfigs.Any(p => p.IsCompanionEnd)) AsCompanion(value);
            }
        }

        /// <summary>
        ///     生成基于当前关联定义的关联引用的配置器，如果配置器已存在返回该配置器。
        /// </summary>
        /// <returns>关联引用配置器；如果当前关联端实体型上未定义相应的关联引用，返回null。</returns>
        /// <param name="propInfo">返回关联引用的访问器，如果关联引用没有访问器返回null。</param>
        public IAssociationReferenceConfigurator AssociationReference(out PropertyInfo propInfo)
        {
            propInfo = _entityType.GetProperty(_name);
            return _referenceConfigurator;
        }

        /// <summary>
        ///     为元素配置项设置一个扩展配置器
        /// </summary>
        /// <typeparam name="TExtensionConfiguration">扩展配置器的类型，须继承自ElementExtensionConfiguration。</typeparam>
        /// <returns></returns>
        public ElementExtensionConfiguration HasExtension<TExtensionConfiguration>()
            where TExtensionConfiguration : ElementExtensionConfiguration, new()
        {
            var configType = typeof(TExtensionConfiguration);
            try
            {
                //判断是否已经存在该扩展配置器
                var ext = ExtensionConfigs.FirstOrDefault(p => p.GetType() == configType);
                if (ext != null)
                    return ext;
                //不存在则创建一个新的扩展配置器
                var extensionConfiguration =
                    (ElementExtensionConfiguration)Activator.CreateInstance(configType);
                ExtensionConfigs.Add(extensionConfiguration);
                return extensionConfiguration;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"添加扩展配置器失败,{configType.Name}没有适合的无参构造函数", nameof(configType), e);
            }
        }

        /// <summary>
        ///     为类型元素设置取值器。
        /// </summary>
        /// <param name="valueGetter">取值器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasValueGetter(IValueGetter valueGetter, bool overrided)
        {
            //覆盖的 覆盖值
            if (overrided)
            {
                _valueGetter = valueGetter;
            }
            else
            {
                //不覆盖 没值的时候才设置
                if (_valueGetter == null)
                    _valueGetter = valueGetter;
            }
        }

        /// <summary>
        ///     使用一个能够获取类型元素值的方法为类型元素创建取值器。
        ///     如果该方法的返回值类型与元素的IsMultiple属性不匹配，则引发异常。
        ///     实施建议：
        ///     调用MethodInfo类的CreateDelegate方法创建代表该方法的委托，然后创建委托取值器。
        /// </summary>
        /// <param name="method">获取元素值的方法。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasValueGetter(MethodInfo method, bool overrided)
        {
            //覆盖的 覆盖值
            if (overrided)
            {
                HasValueGetter(method);
            }
            else
            {
                //不覆盖 没值的时候才设置
                if (_valueGetter == null)
                    HasValueGetter(method);
            }
        }

        /// <summary>
        ///     使用一个能够获取类型元素值的属性访问器为类型元素创建取值器。
        ///     如果该属性访问器的返回值类型与元素的IsMultiple属性不匹配，则引发异常。
        ///     实施建议：
        ///     首先取出该属性访问器的Get方法，然后调用MethodInfo类的CreateDelegate方法创建代表该方法的委托，最后创建委托取值器。
        /// </summary>
        /// <param name="property">获取元素值的属性访问器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasValueGetter(PropertyInfo property, bool overrided)
        {
            //覆盖的 覆盖值
            if (overrided)
            {
                HasValueGetter(property);
            }
            else
            {
                //不覆盖 没值的时候才设置
                if (_valueGetter == null)
                    HasValueGetter(property);
            }
        }

        /// <summary>
        ///     使用表示类型元素的字段为类型元素创建取值器。
        ///     如果字段的数据类型与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        /// <param name="field">表示类型元素的字段。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasValueGetter(FieldInfo field, bool overrided)
        {
            //覆盖的 覆盖值
            if (overrided)
            {
                HasValueGetter(field);
            }
            else
            {
                //不覆盖 没值的时候才设置
                if (_valueGetter == null)
                    HasValueGetter(field);
            }
        }

        /// <summary>
        ///     使用指定的类成员为类型元素创建取值器。
        ///     如果该成员与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="memberType">成员的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasValueGetter(string memberName, MemberTypes memberType, bool overrided)
        {
            // 覆盖的 覆盖值
            if (overrided)
            {
                HasValueGetter(memberName, memberType);
            }
            else
            {
                //不覆盖 没值的时候才设置
                if (_valueGetter == null)
                    HasValueGetter(memberName, memberType);
            }
        }

        /// <summary>
        ///     使用与类型元素同名的类成员为类型元素创建取值器。
        ///     如果该成员与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        /// <param name="memberType">同名成员的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasValueGetter(MemberTypes memberType, bool overrided)
        {
            // 覆盖的 覆盖值
            if (overrided)
            {
                HasValueGetter(_name, memberType);
            }
            else
            {
                //不覆盖 没值的时候才设置
                if (_valueGetter == null)
                    HasValueGetter(_name, memberType);
            }
        }

        /// <summary>
        ///     使用与类型元素同名的属性访问器为类型元素创建取值器。
        ///     如果该成员与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasValueGetter(bool overrided)
        {
            // 覆盖的 覆盖值
            if (overrided)
            {
                HasValueGetter(_name, MemberTypes.Property);
            }
            else
            {
                //不覆盖 没值的时候才设置
                if (_valueGetter == null)
                    HasValueGetter(_name, MemberTypes.Property);
            }
        }

        /// <summary>
        ///     为类型元素设置设值器。
        /// </summary>
        /// <param name="valueSetter">设值器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasValueSetter(IValueSetter valueSetter, bool overrided)
        {
            //覆盖的 覆盖值
            if (overrided)
            {
                HasValueSetter(valueSetter);
            }
            else
            {
                //不覆盖 没值的时候才设置
                if (_valueSetter == null)
                    HasValueSetter(valueSetter);
            }
        }

        /// <summary>
        ///     使用一个能够为类型元素设值的方法为类型元素创建设值器。
        ///     实施说明
        ///     检测方法的DeclaringType，如果为引用类型，使用MethodInfo.CreateDelegate方法创建Action{TStructural,
        ///     TElement}委托；如果是结构体，使用Emit创建SetValue{TStructural, TElement}委托。
        ///     使用上述委托，调用ValueSetter的Create方法创建设值器。
        /// </summary>
        /// <param name="method">为类型元素设值的方法。</param>
        /// <param name="mode">设值模式。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasValueSetter(MethodInfo method, EValueSettingMode mode, bool overrided)
        {
            // 覆盖的 覆盖值
            if (overrided)
            {
                HasValueSetter(method, mode);
            }
            else
            {
                //不覆盖 没值的时候才设置
                if (_valueSetter == null)
                    HasValueSetter(method, mode);
            }
        }

        /// <summary>
        ///     使用指定的类成员为类型元素创建设值器。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="memberType">成员的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasValueSetter(string memberName, MemberTypes memberType, bool overrided)
        {
            // 覆盖的 覆盖值
            if (overrided)
            {
                HasValueSetter(memberName, memberType);
            }
            else
            {
                //不覆盖 没值的时候才设置
                if (_valueSetter == null)
                    HasValueSetter(memberName, memberType);
            }
        }

        /// <summary>
        ///     使用一个能够为类型元素设值的Property为类型元素创建设值器。
        ///     实施说明
        ///     取出Property的Set方法，然后调用HasValueSetter(methodInfo, mode)方法。
        /// </summary>
        /// <param name="property">为类型元素设值的属性访问器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasValueSetter(PropertyInfo property, bool overrided)
        {
            // 覆盖的 覆盖值
            if (overrided)
            {
                HasValueSetter(property);
            }
            else
            {
                //不覆盖 没值的时候才设置
                if (_valueSetter == null)
                    HasValueSetter(property);
            }
        }

        /// <summary>
        ///     使用与类型元素同名的类成员为类型元素创建设值器。
        /// </summary>
        /// <param name="memberType">成员的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasValueSetter(MemberTypes memberType, bool overrided)
        {
            // 覆盖的 覆盖值
            if (overrided)
            {
                HasValueSetter(_name, memberType);
            }
            else
            {
                //不覆盖 没值的时候才设置
                if (_valueSetter == null)
                    HasValueSetter(_name, memberType);
            }
        }

        /// <summary>
        ///     为集合类型的元素创建设值器，该设值器可以向集合添加或移除元素。
        ///     实施说明
        ///     检测方法的DeclaringType，如果为引用类型，使用MethodInfo.CreateDelegate方法创建Action{TStructural,
        ///     TElement>委托；如果是结构体，使用Emit创建SetValue{TStructural, TElement}委托。
        ///     使用上述委托，实例化CollectionValueSetter。
        /// </summary>
        /// <param name="appendingMethod">添加集合项的方法。</param>
        /// <param name="removingMethod">移除集合项的方法。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasValueSetter(MethodInfo appendingMethod, MethodInfo removingMethod, bool overrided)
        {
            // 覆盖的 覆盖值
            if (overrided)
            {
                HasValueSetter(appendingMethod, EValueSettingMode.Assignment);
            }
            else
            {
                //不覆盖 没值的时候才设置
                if (_valueSetter == null)
                    HasValueSetter(appendingMethod, EValueSettingMode.Assignment);
            }
        }

        /// <summary>
        ///     使用与类型元素同名的属性访问器为类型元素创建设值器。
        /// </summary>
        public void HasValueSetter(bool overrided)
        {
            // 覆盖的 覆盖值
            if (overrided)
            {
                HasValueSetter(_name, MemberTypes.Property);
            }
            else
            {
                //不覆盖 没值的时候才设置
                if (_valueSetter == null)
                    HasValueSetter(_name, MemberTypes.Property);
            }
        }

        /// <summary>
        ///     使用表示类型元素的字段为类型元素创建设值器。
        ///     实施说明
        ///     使用ValueSetter类的Create方法创建设值器。
        /// </summary>
        /// <param name="field">表示类型元素的字段。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasValueSetter(FieldInfo field, bool overrided)
        {
            // 覆盖的 覆盖值
            if (overrided)
            {
                HasValueSetter(field);
            }
            else
            {
                //不覆盖 没值的时候才设置
                if (_valueSetter == null)
                    HasValueSetter(field);
            }
        }

        /// <summary>
        ///     进入当前元素所属类型的配置项。
        /// </summary>
        public IStructuralTypeConfigurator Upward()
        {
            return (IStructuralTypeConfigurator)_typeConfiguration;
        }

        /// <summary>
        ///     是否已启用延迟加载。
        /// </summary>
        public bool EnableLazyLoading => _enableLazyLoading;

        /// <summary>
        ///     设置是否支持延迟加载。
        /// </summary>
        /// <param name="enableLazyLoading"></param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasEnableLazyLoading(bool enableLazyLoading, bool overrided)
        {
            //覆盖的 覆盖值
            if (overrided)
                _enableLazyLoading = enableLazyLoading;
        }

        /// <summary>
        ///     指定关联或关联端的加载优先级，数值小者先加载。
        /// </summary>
        /// <param name="loadingPriority">加载优先级。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasLoadingPriority(int loadingPriority, bool overrided)
        {
            //覆盖的 覆盖值
            if (overrided)
                LoadingPriority = loadingPriority;
        }

        /// <summary>
        ///     设置加载触发器。
        ///     每次调用本方法将追加一个加载触发器。
        /// </summary>
        /// <param name="loadingTrigger">加载触发器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasLoadingTrigger(IBehaviorTrigger loadingTrigger, bool overrided)
        {
            //覆盖的 清空
            if (overrided)
                _behaviorTriggers.Clear();
            _behaviorTriggers.Add(loadingTrigger);
        }

        /// <summary>
        ///     使用一个能触发引用加载的方法为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="method">触发引用加载的方法。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasLoadingTrigger(MethodInfo method, bool overrided)
        {
            //覆盖的 清空
            if (overrided)
                _behaviorTriggers.Clear();
            _behaviorTriggers.Add(new MethodTrigger(method));
        }

        /// <summary>
        ///     使用一个能触发引用加载的属性访问器为引用元素创建Property-Get型加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="property">触发引用加载的属性访问器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasLoadingTrigger(PropertyInfo property, bool overrided)
        {
            //覆盖的 清空
            if (overrided)
                _behaviorTriggers.Clear();
            _behaviorTriggers.Add(new MethodTrigger(property.SetMethod));
        }

        /// <summary>
        ///     使用一个能触发引用加载的属性访问器为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="property">触发引用加载的属性访问器。</param>
        /// <param name="triggerType">要创建的加载触发器的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasLoadingTrigger(PropertyInfo property, EBehaviorTriggerType triggerType, bool overrided)
        {
            //覆盖的 清空
            if (overrided)
                _behaviorTriggers.Clear();

            HasLoadingTrigger(property, triggerType);
        }

        /// <summary>
        ///     使用一个能触发引用加载的成员为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="triggerType">要创建的加载触发器的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasLoadingTrigger(string memberName, EBehaviorTriggerType triggerType, bool overrided)
        {
            //覆盖的 清空
            if (overrided)
                _behaviorTriggers.Clear();

            HasLoadingTrigger(memberName, triggerType);
        }

        /// <summary>
        ///     使用与引用元素同名的成员为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="triggerType">要创建的加载触发器的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasLoadingTrigger(EBehaviorTriggerType triggerType, bool overrided)
        {
            //覆盖的 清空
            if (overrided)
                _behaviorTriggers.Clear();

            HasLoadingTrigger(_name, triggerType);
        }

        /// <summary>
        ///     使用与引用元素同名的属性访问器为引用元素创建Property-Get型加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="overrided">是否覆盖既有配置</param>
        public void HasLoadingTrigger(bool overrided)
        {
            //覆盖的 清空
            if (overrided)
                _behaviorTriggers.Clear();
            HasLoadingTrigger(_name, EBehaviorTriggerType.PropertyGet);
        }

        /// <summary>
        ///     设置一个值，该值指示是否把关联端对象默认视为新对象。当该属性为true时，如果关联端对象未被显式附加到上下文，该对象将被视为新对象实施持久化。
        /// </summary>
        /// <param name="defaultAsNew">指示是否把关联端对象默认视为新对象。</param>
        public AssociationEndConfiguration HasDefaultAsNew(bool defaultAsNew)
        {
            DefaultAsNew = defaultAsNew;
            return this;
        }

        /// <summary>
        ///     设置一个值，该值指示当前关联端是否为聚合关联端。
        /// </summary>
        /// <param name="isAggregated">指示当前关联端是否为聚合关联端。</param>
        public AssociationEndConfiguration IsAggregated(bool isAggregated)
        {
            _isAggregated = isAggregated;
            return this;
        }

        /// <summary>
        ///     配置关联端映射
        /// </summary>
        /// <param name="keyAttribute">关联端标识属性的名称。</param>
        /// <param name="targetField">上述标识属性的映射字段。</param>
        /// <returns></returns>
        public AssociationEndConfiguration HasMapping(string keyAttribute, string targetField)
        {
            //在没有映射的情况下添加映射
            if (Mappings.Count == 0 ||
                Mappings.All(p => p.TargetField != targetField && p.KeyAttribute != keyAttribute))
                Mappings.Add(new AssociationEndMapping { KeyAttribute = keyAttribute, TargetField = targetField });
            return this;
        }

        /// <summary>
        ///     指示是否将当前关联端作为伴随端。
        ///     说明
        ///     设置当前端为伴随端会将之前设置的伴随端改设不作为伴随端。
        ///     当override为false时，其它端只要任意一端已设置为伴随端，本方法就不再执行设置操作。
        /// </summary>
        /// <param name="value">指示是否作为伴随端。</param>
        public AssociationEndConfiguration AsCompanion(bool value)
        {
            //如果设置为伴随端 则先将其他端都设置为不作为伴随端
            if (value)
            {
                var endConfigs = AssociationConfiguratorBuilder.EndConfigurations;
                foreach (var endConfig in endConfigs) endConfig.AsCompanion(false);
            }

            _isCompanionEnd = value;

            return this;
        }

        /// <summary>
        ///     为类型元素设置取值器。
        /// </summary>
        /// <param name="valueGetter">取值器。</param>
        public AssociationEndConfiguration HasValueGetter(IValueGetter valueGetter)
        {
            _valueGetter = valueGetter;
            return this;
        }

        /// <summary>
        ///     使用一个能够获取类型元素值的方法为类型元素创建取值器。
        ///     如果该方法的返回值类型与元素的IsMultiple属性不匹配，则引发异常。
        ///     实施建议：
        ///     调用MethodInfo类的CreateDelegate方法创建代表该方法的委托，然后创建委托取值器。
        /// </summary>
        /// <param name="method">获取元素值的方法。</param>
        public AssociationEndConfiguration HasValueGetter(MethodInfo method)
        {
            if (IsMultiple)
            {
                //包装要取的值
                var ienumableType = typeof(IEnumerable<>).MakeGenericType(method.ReturnType.GetGenericArguments()[0]);
                var delegateType =
                    typeof(Func<,>).MakeGenericType(AssociationConfiguratorBuilder.AssociationType, ienumableType);
                //创建委托
                var delegateFunc = method.CreateDelegate(delegateType);
                if (delegateFunc == null) throw new ArgumentException($"{method.Name}方法与目标多重性不一致.");
                //创建取值器
                var valueGetter =
                    typeof(DelegateValueGetter<,>).MakeGenericType(AssociationConfiguratorBuilder.AssociationType,
                        ienumableType);
                var valueGetterInstance = Activator.CreateInstance(valueGetter, delegateFunc) as IValueGetter;
                return HasValueGetter(valueGetterInstance);
            }
            else
            {
                var delegateType = typeof(Func<,>).MakeGenericType(AssociationConfiguratorBuilder.AssociationType,
                    method.ReturnType);
                //创建委托
                var delegateFunc = method.CreateDelegate(delegateType);
                if (delegateFunc == null) throw new ArgumentException($"{method.Name}方法与目标多重性不一致.");
                //创建取值器
                var valueGetter =
                    typeof(DelegateValueGetter<,>).MakeGenericType(AssociationConfiguratorBuilder.AssociationType,
                        method.ReturnType);
                var valueGetterInstance = Activator.CreateInstance(valueGetter, delegateFunc) as IValueGetter;
                return HasValueGetter(valueGetterInstance);
            }
        }

        /// <summary>
        ///     使用一个能够获取类型元素值的属性访问器为类型元素创建取值器。
        ///     如果该属性访问器的返回值类型与元素的IsMultiple属性不匹配，则引发异常。
        ///     实施建议：
        ///     首先取出该属性访问器的Get方法，然后调用MethodInfo类的CreateDelegate方法创建代表该方法的委托，最后创建委托取值器。
        /// </summary>
        /// <param name="property">获取元素值的属性访问器。</param>
        public AssociationEndConfiguration HasValueGetter(PropertyInfo property)
        {
            //区分是否为结构
            if (property.ReflectedType?.IsValueType == true)
            {
                //用表达式编译
                var pe = Expression.Parameter(property.ReflectedType);
                var funcType =
                    typeof(Func<,>).MakeGenericType(property.ReflectedType, property.PropertyType);
                var member = Expression.Property(pe, property);
                //构造取值表达式
                var exp = Expression.Lambda(funcType, member, pe);
                //用表达式编译结果构造委托设值器
                var getter =
                    typeof(DelegateValueGetter<,>).MakeGenericType(AssociationConfiguratorBuilder.AssociationType,
                        property.PropertyType);
                var getterObj = Activator.CreateInstance(getter, exp.Compile()) as IValueGetter;
                return HasValueGetter(getterObj);
            }

            return HasValueGetter(property.GetMethod);
        }

        /// <summary>
        ///     使用表示类型元素的字段为类型元素创建取值器。
        ///     如果字段的数据类型与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        /// <param name="field">表示类型元素的字段。</param>
        public AssociationEndConfiguration HasValueGetter(FieldInfo field)
        {
            //是IEnumable并且不是string 则认为是多重的
            var filedIsMulty = field.FieldType.GetInterface("IEnumerable") != null && field.FieldType != typeof(string);

            if (filedIsMulty != IsMultiple) throw new ArgumentException($"{field.Name}与目标的多重性不一致.");

            var filedSetter = new FieldValueGetter(field);

            return HasValueGetter(filedSetter);
        }

        /// <summary>
        ///     使用指定的类成员为类型元素创建取值器。
        ///     如果该成员与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="memberType">成员的类型。</param>
        public AssociationEndConfiguration HasValueGetter(string memberName, MemberTypes memberType)
        {
            var memberInfo = AssociationConfiguratorBuilder.AssociationType.GetMember(memberName, memberType,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (memberInfo.Length <= 0) throw new ArgumentException($"无法获取到成员{memberName}.");
            //根据不同的成员类型创建取值器
            switch (memberType)
            {
                case MemberTypes.Field:
                {
                    HasValueGetter((FieldInfo)memberInfo[0]);
                    break;
                }
                case MemberTypes.Property:
                {
                    HasValueGetter((PropertyInfo)memberInfo[0]);
                    break;
                }
                case MemberTypes.Method:
                {
                    HasValueGetter((MethodInfo)memberInfo[0]);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(memberType), memberType, $"取值器不支持此成员类型{memberType}");
            }

            return this;
        }

        /// <summary>
        ///     使用与类型元素同名的类成员为类型元素创建取值器。
        ///     如果该成员与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        /// <param name="memberType">同名成员的类型。</param>
        public AssociationEndConfiguration HasValueGetter(MemberTypes memberType)
        {
            return HasValueGetter(_name, memberType);
        }

        /// <summary>
        ///     使用与类型元素同名的属性访问器为类型元素创建取值器。
        ///     如果该成员与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        public AssociationEndConfiguration HasValueGetter()
        {
            return HasValueGetter(_name, MemberTypes.Property);
        }

        /// <summary>
        ///     为类型元素设置设值器。
        /// </summary>
        /// <param name="valueSetter">设值器。</param>
        public AssociationEndConfiguration HasValueSetter(IValueSetter valueSetter)
        {
            _valueSetter = valueSetter;
            return this;
        }

        /// <summary>
        ///     使用一个能够为类型元素设值的方法为类型元素创建设值器。
        ///     实施说明
        ///     检测方法的DeclaringType，如果为引用类型，使用MethodInfo.CreateDelegate方法创建Action{TStructural,
        ///     TElement}委托；如果是结构体，使用Emit创建SetValue{TStructural, TElement}委托。
        ///     使用上述委托，调用ValueSetter的Create方法创建设值器。
        /// </summary>
        /// <param name="method">为类型元素设值的方法。</param>
        /// <param name="mode">设值模式。</param>
        public AssociationEndConfiguration HasValueSetter(MethodInfo method, EValueSettingMode mode)
        {
            if (method.GetParameters().Length != 1) throw new ArgumentException("设值器方法只能有一个参数.");
            Delegate del;
            Type[] types = { method.DeclaringType, method.GetParameters()[0].ParameterType };
            //值类型（即非引用类型）
            if (method.DeclaringType != null && method.DeclaringType.IsValueType)
            {
                //调用IL
                //定义方法 TStruct 引用传递 TValue 值传递 设定Owner为结构类型 跳过JIT检查
                var dynamicMethod = new DynamicMethod(method.Name, null, new[] { types[0].MakeByRefType(), types[1] },
                    types[0], true);
                //IL 压入参数
                var il = dynamicMethod.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Callvirt,
                    types[0].GetMethod(method.Name, new[] { types[1] }) ??
                    throw new InvalidOperationException($"无法从{method.DeclaringType}的方法{method.Name}中构造设值器"));
                il.Emit(OpCodes.Ret);

                //根据IL生成的SetValue委托
                var setValueFuncType = typeof(SetValue<,>).MakeGenericType(types);
                del = method.CreateDelegate(setValueFuncType);
            }
            //引用类型
            else
            {
                var typeAction = typeof(Action<,>).MakeGenericType(types);
                del = method.CreateDelegate(typeAction);
            }

            return HasValueSetter(ValueSetter.Create(del, mode));
        }

        /// <summary>
        ///     使用指定的类成员为类型元素创建设值器。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="memberType">成员的类型。</param>
        public AssociationEndConfiguration HasValueSetter(string memberName, MemberTypes memberType)
        {
            var memberInfo = AssociationConfiguratorBuilder.AssociationType.GetMember(memberName, memberType,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (memberInfo.Length <= 0) throw new ArgumentException($"无法获取到成员{memberName}.");
            //根据不同的成员类型创建设值器
            switch (memberType)
            {
                case MemberTypes.Field:
                {
                    HasValueSetter((FieldInfo)memberInfo[0]);
                    break;
                }
                case MemberTypes.Property:
                {
                    HasValueSetter((PropertyInfo)memberInfo[0]);
                    break;
                }
                case MemberTypes.Method:
                {
                    //此处无法确定eValueSettingMode
                    throw new InvalidOperationException("不支持只用Method和MethodName构造设值器");
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(memberType), memberType, $"设值器不支持此成员类型{memberType}");
            }

            return this;
        }

        /// <summary>
        ///     使用一个能够为类型元素设值的Property为类型元素创建设值器。
        ///     实施说明
        ///     取出Property的Set方法，然后调用HasValueSetter(methodInfo, mode)方法。
        /// </summary>
        /// <param name="property">为类型元素设值的属性访问器。</param>
        public AssociationEndConfiguration HasValueSetter(PropertyInfo property)
        {
            //使用PropertyInfo的GetSetMethod方法获取Set方法
            var setMethod = property.GetSetMethod(true);
            if (setMethod == null) throw new Exception($"Property({property.Name})没有Set方法");
            return HasValueSetter(setMethod, EValueSettingMode.Assignment);
        }

        /// <summary>
        ///     使用与类型元素同名的类成员为类型元素创建设值器。
        /// </summary>
        /// <param name="memberType">成员的类型。</param>
        public AssociationEndConfiguration HasValueSetter(MemberTypes memberType)
        {
            return HasValueSetter(_name, memberType);
        }

        /// <summary>
        ///     为集合类型的元素创建设值器，该设值器可以向集合添加或移除元素。
        ///     实施说明
        ///     检测方法的DeclaringType，如果为引用类型，使用MethodInfo.CreateDelegate方法创建Action{TStructural,
        ///     TElement>委托；如果是结构体，使用Emit创建SetValue{TStructural, TElement}委托。
        ///     使用上述委托，实例化CollectionValueSetter。
        /// </summary>
        /// <param name="appendingMethod">添加集合项的方法。</param>
        /// <param name="removingMethod">移除集合项的方法。</param>
        public AssociationEndConfiguration HasValueSetter(MethodInfo appendingMethod, MethodInfo removingMethod)
        {
            return HasValueSetter(appendingMethod, EValueSettingMode.Assignment);
        }

        /// <summary>
        ///     使用与类型元素同名的属性访问器为类型元素创建设值器。
        /// </summary>
        public AssociationEndConfiguration HasValueSetter()
        {
            return HasValueSetter(_name, MemberTypes.Property);
        }

        /// <summary>
        ///     使用表示类型元素的字段为类型元素创建设值器。
        ///     实施说明
        ///     使用ValueSetter类的Create方法创建设值器。
        /// </summary>
        /// <param name="field">表示类型元素的字段。</param>
        public AssociationEndConfiguration HasValueSetter(FieldInfo field)
        {
            return HasValueSetter(ValueSetter.Create(field));
        }

        /// <summary>
        ///     使用一个能够获取类型元素的值且返回值为单个对象的委托为不具备多重性的类型元素创建取值器。
        ///     注：当元素具备多重性时，该方法将引发异常。
        /// </summary>
        /// <typeparam name="TProperty">表示元素的类型。对于属性，它表示属性值类型；对于关联引用，它表示关联类型；对于关联端，它表示关联端的类型。</typeparam>
        /// <typeparam name="TStructural">关联端的类型</typeparam>
        /// <param name="getValue">获取元素值的委托。</param>
        public AssociationEndConfiguration HasValueGetter<TStructural, TProperty>(Func<TStructural, TProperty> getValue)
        {
            if (IsMultiple) throw new ArgumentException($"{Name}类型的设值器为多重性,不能设置单一设值器.");
            //使用委托取值器
            var valueGetter = new DelegateValueGetter<TStructural, TProperty>(getValue);
            return HasValueGetter(valueGetter);
        }

        /// <summary>
        ///     使用一个能够获取类型元素的值且返回值为对象序列的委托为具备多重性的类型元素创建取值器。
        ///     注：当元素不具备多重性时，该方法将引发异常。
        /// </summary>
        /// <typeparam name="TProperty">表示元素的类型。对于属性，它表示属性值类型；对于关联引用，它表示关联类型；对于关联端，它表示关联端的类型。</typeparam>
        /// <typeparam name="TStructural">关联端的类型</typeparam>
        /// <param name="getValue">获取元素值的委托。</param>
        public AssociationEndConfiguration HasValueGetter<TStructural, TProperty>(
            Func<TStructural, IEnumerable<TProperty>> getValue)
        {
            if (!IsMultiple) throw new ArgumentException($"{Name}类型的设值器为单一性,不能设置多重设值器.");
            //使用委托取值器
            var valueGetter = new DelegateValueGetter<TStructural, IEnumerable<TProperty>>(getValue);
            return HasValueGetter(valueGetter);
        }

        /// <summary>
        ///     为lambda表达式指示的元素创建设值器，该lambda表达式的主体须为MemberExpression，其访问的成员代表要设值的元素。
        /// </summary>
        /// <typeparam name="TProperty">作为lambda表达式主体的MemberExpression的类型，亦即元素值的类型。</typeparam>
        /// <typeparam name="TStructural">关联端的类型</typeparam>
        /// <param name="expression">表达式</param>
        /// 实施说明
        /// 从Lambda表达式取出Body，将其转换为MemberExpression，然后通过其Member属性获取PropertyInfo。
        public AssociationEndConfiguration HasValueSetter<TStructural, TProperty>(
            Expression<Func<TStructural, TProperty>> expression)
        {
            if (expression.Body is MemberExpression member)
            {
                //获取反射属性
                //获取实体模型对应类型的属性
                var property = typeof(TStructural).GetProperty(member.Member.Name);
                if (property == null)
                    throw new ArgumentException($"{typeof(TStructural).FullName}无法找到{member.Member.Name},不能配置设值器.");
                return HasValueSetter(property);
            }

            throw new ArgumentException("不能使用非属性访问表达式配置关联端设值器");
        }

        /// <summary>
        ///     为lambda表达式指示的元素创建设值器，该lambda表达式的主体须为MemberExpression，其访问的成员代表要设值的元素。
        /// </summary>
        /// <param name="propertyExp">表示属性访问器的Lambda表达式。</param>
        /// <param name="valueCreator"></param>
        /// <typeparam name="TProperty">作为lambda表达式主体的MemberExpression的类型，亦即元素值的类型。</typeparam>
        /// <typeparam name="TElement">值序列项的类型。</typeparam>
        /// <typeparam name="TStructural">关联端的类型</typeparam>
        /// 实施说明:
        /// 从Lambda表达式取出Body，将其置换为MemberExpression，然后通过其Member属性获取PropertyInfo。
        /// 从上述PropertyInfo获取SetMethod，然后调用CreateDelegate方法创建Action
        /// {TStructural, TProperty}
        /// 委托。
        /// 使用上述委托实例化DelegateEnumerableValueSetter{TStructural, TProperty, TElement}。
        public AssociationEndConfiguration HasValueSetter<TStructural, TProperty, TElement>(
            Expression<Func<TStructural, TProperty>> propertyExp,
            Func<IEnumerable<TElement>, TProperty> valueCreator)
        {
            if (propertyExp.Body is MemberExpression member)
            {
                var property = typeof(TStructural).GetProperty(member.Member.Name);
                if (property != null)
                {
                    //使用委托取值器
                    var setMethod = property.SetMethod;
                    var setDelegate = setMethod.CreateDelegate(typeof(Action<TStructural, TProperty>));
                    var setterType =
                        typeof(DelegateEnumerableValueSetter<,,>).MakeGenericType(typeof(TStructural),
                            typeof(TProperty),
                            typeof(TElement));
                    return HasValueSetter((ValueSetter)Activator.CreateInstance(setterType, setDelegate, valueCreator));
                }
            }

            throw new ArgumentException("不能使用非属性访问表达式配置关联端设值器");
        }

        /// <summary>
        ///     使用能够修改元素值的委托为类型元素创建设值器,
        ///     当设值模式为Assignment时该委托为元素赋值，
        ///     当设值模式为Appending时该委托在元素值序列尾部追加项。
        /// </summary>
        /// <typeparam name="TValue">
        ///     Assignment模式下为元素值的类型，Appending模式下为元素值序列项的类型。
        /// </typeparam>
        /// <typeparam name="TStructural">关联端的类型</typeparam>
        /// <param name="setValue">表示属性访问器的Lambda表达式。</param>
        /// <param name="mode">设值模式。</param>
        /// 实施说明
        /// 使用ValueSetter的Create方法创建设值器。
        public AssociationEndConfiguration HasValueSetter<TStructural, TValue>(Action<TStructural, TValue> setValue,
            EValueSettingMode mode)
        {
            return HasValueSetter(ValueSetter.Create(setValue, mode));
        }

        /// <summary>
        ///     使用能够为元素赋值的委托为类型元素创建设值器。
        /// </summary>
        /// <param name="setValue">表示属性访问器的Lambda表达式。</param>
        /// <param name="valueCreator"></param>
        /// <typeparam name="TValue">元素值的类型。</typeparam>
        /// <typeparam name="TElement">值序列项的类型。</typeparam>
        /// <typeparam name="TStructural">关联端的类型</typeparam>
        /// 实施说明:
        /// 实例化DelegateEnumerableValueSetter
        /// {TStructural, TValue, TElement}
        public AssociationEndConfiguration HasValueSetter<TStructural, TValue, TElement>(
            Action<TStructural, TValue> setValue,
            Func<IEnumerable<TElement>, TValue> valueCreator)
            where TValue : IEnumerable<TElement>
        {
            var type = typeof(DelegateEnumerableValueSetter<,,>).MakeGenericType(typeof(TStructural), typeof(TValue),
                typeof(TElement));
            return HasValueSetter((ValueSetter)Activator.CreateInstance(type, setValue, valueCreator));
        }

        /// <summary>
        ///     设置是否支持延迟加载。
        /// </summary>
        /// <param name="enableLazyLoading"></param>
        public AssociationEndConfiguration HasEnableLazyLoading(bool enableLazyLoading)
        {
            _enableLazyLoading = enableLazyLoading;
            return this;
        }

        /// <summary>
        ///     指定关联或关联端的加载优先级，数值小者先加载。
        /// </summary>
        /// <param name="loadingPriority">加载优先级。</param>
        public AssociationEndConfiguration HasLoadingPriority(int loadingPriority)
        {
            LoadingPriority = loadingPriority;
            return this;
        }

        /// <summary>
        ///     设置加载触发器。
        ///     每次调用本方法将追加一个加载触发器。
        /// </summary>
        /// <param name="loadingTrigger">加载触发器。</param>
        public AssociationEndConfiguration HasLoadingTrigger(
            IBehaviorTrigger loadingTrigger)
        {
            _behaviorTriggers.Add(loadingTrigger);
            return this;
        }

        /// <summary>
        ///     使用一个能触发引用加载的方法为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="method">触发引用加载的方法。</param>
        public AssociationEndConfiguration HasLoadingTrigger(MethodInfo method)
        {
            _behaviorTriggers.Add(new MethodTrigger(method));
            return this;
        }

        /// <summary>
        ///     使用一个能触发引用加载的属性访问器为引用元素创建Property-Get型加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="property">触发引用加载的属性访问器。</param>
        public AssociationEndConfiguration HasLoadingTrigger(PropertyInfo property)
        {
            _behaviorTriggers.Add(new MethodTrigger(property.SetMethod));
            return this;
        }

        /// <summary>
        ///     使用一个能触发引用加载的属性访问器为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="property">触发引用加载的属性访问器。</param>
        /// <param name="triggerType">要创建的加载触发器的类型。</param>
        public AssociationEndConfiguration HasLoadingTrigger(PropertyInfo property, EBehaviorTriggerType triggerType)
        {
            //根据不同的触发器类型创建触发器
            MethodInfo method;
            switch (triggerType)
            {
                case EBehaviorTriggerType.Method:
                    throw new ArgumentException("方法型触发器不能用PropertyInfo构造.");
                case EBehaviorTriggerType.PropertyGet:
                    method = property.GetMethod;
                    break;
                case EBehaviorTriggerType.PropertySet:
                    method = property.SetMethod;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(triggerType), triggerType,
                        $"未知的行为触发器的类型{triggerType}");
            }

            _behaviorTriggers.Add(new MethodTrigger(method));
            return this;
        }

        /// <summary>
        ///     使用一个能触发引用加载的成员为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="triggerType">要创建的加载触发器的类型。</param>
        public AssociationEndConfiguration HasLoadingTrigger(string memberName, EBehaviorTriggerType triggerType)
        {
            var property = AssociationConfiguratorBuilder.AssociationType.GetProperty(memberName);
            return HasLoadingTrigger(property, triggerType);
        }

        /// <summary>
        ///     使用与引用元素同名的成员为引用元素创建加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        /// <param name="triggerType">要创建的加载触发器的类型。</param>
        public AssociationEndConfiguration HasLoadingTrigger(EBehaviorTriggerType triggerType)
        {
            return HasLoadingTrigger(_name, triggerType);
        }

        /// <summary>
        ///     使用与引用元素同名的属性访问器为引用元素创建Property-Get型加载触发器。
        ///     每次调用本方法将追加一个触发器。
        /// </summary>
        public AssociationEndConfiguration HasLoadingTrigger()
        {
            return HasLoadingTrigger(_name, EBehaviorTriggerType.PropertyGet);
        }


        /// <summary>
        ///     设置关联型
        /// </summary>
        /// <param name="structuralTypeConfiguration"></param>
        internal void SetAssociationType(StructuralTypeConfiguration structuralTypeConfiguration)
        {
            _typeConfiguration = structuralTypeConfiguration;
        }
    }

    /// <summary>
    ///     适用于隐式关联的关联端配置器。
    ///     类型参数
    ///     TEntity
    ///     作为关联端的实体类型。
    /// </summary>
    public class AssociationEndConfiguration<TEntity> : AssociationEndConfiguration
        where TEntity : class
    {
        /// <summary>
        ///     基于当前关联定义的关联引用的配置器。
        /// </summary>
        private AssociationReferenceConfiguration<TEntity> _associationReferenceConfiguration;


        /// <summary>
        ///     初始化AssociationEndConfiguration类的新实例。
        ///     实施说明
        ///     串联字符串“End”和关联端索引生成关联端名称。
        ///     关联端的多重性恒为false。
        /// </summary>
        /// <param name="endIndex">关联端在关联型上的索引号（从1开始计数）。</param>
        /// <param name="assoConfigBuilder">关联配置器建造器。</param>
        public AssociationEndConfiguration(byte endIndex, AssociationConfiguratorBuilder assoConfigBuilder)
        {
            _entityType = typeof(TEntity);
            _endIndex = endIndex;
            IsMultiple = false;
            _name = $"End{endIndex}";
            AssociationConfiguratorBuilder = assoConfigBuilder;
        }

        /// <summary>
        ///     获取行为触发器，对于属性是指修改触发器，对于关联引用和关联端是加载触发器。
        /// </summary>
        public override List<IBehaviorTrigger> BehaviorTriggers => _behaviorTriggers;


        /// <summary>
        ///     启动对关联端上基于当前关联定义的关联引用的配置；如果相应的配置项未创建则新建一个。
        ///     类型参数
        ///     TReferred
        ///     被引对象组成的元组的类型。被引对象是指关联引用指向的对象，如果关联引用是多重性的，它是指其中的一个。
        ///     实施说明
        ///     如果_associationReferenceConfiguration不为null，直接返回。应寄存新建的配置器，避免重复创建。
        ///     参见顺序图“配置隐式关联”。
        /// </summary>
        /// <param name="name">关联引用名称，它将作为关联引用的键</param>
        /// <param name="isMultiple">关联引用是否具有多重性。</param>
        public AssociationReferenceConfiguration<TEntity> AssociationReference<TReferred>(string name, bool isMultiple)
        {
            if (_associationReferenceConfiguration == null)
            {
                //首先 查找端的配置
                var entityTypeConfiguration =
                    (EntityTypeConfiguration<TEntity>)AssociationConfiguratorBuilder.ModelBuilder.FindConfiguration(
                        typeof(TEntity));

                if (entityTypeConfiguration == null)
                    throw new ArgumentException($"类型为{typeof(TEntity)}的实体型未注册");

                //创建关联应用配置类型
                var assRefCfgType =
                    typeof(AssociationReferenceConfiguration<,>).MakeGenericType(typeof(TEntity), typeof(TReferred));
                //创建关联应用配置类型 实例
                var configuration =
                    Activator.CreateInstance(assRefCfgType, name, isMultiple, _endIndex,
                            entityTypeConfiguration) as AssociationReferenceConfiguration
                        <TEntity, EntityTypeConfiguration<TEntity>>;

                //保存
                _associationReferenceConfiguration = configuration ??
                                                     throw new ArgumentException(
                                                         $"创建类型为{typeof(TEntity)}.{name}关联引用配置类型失败");
                _referenceConfigurator = configuration;
                //加入实体型的配置
                entityTypeConfiguration.AddAssociationReference(_associationReferenceConfiguration);
                //配置左端
                _referenceConfigurator.HasLeftEnd(_name);
            }

            return _associationReferenceConfiguration;
        }

        /// <summary>
        ///     启动对关联端上基于当前关联定义的关联引用的配置；如果相应的配置项未创建则新建一个。
        ///     类型参数
        ///     TResult
        ///     关联引用值的类型。由于本方法的当前重载版本用于配置不具多重性的关联引用，该类型也就是被引对象组成的元组的类型。被引对象是指关联引用指向的对象。
        ///     实施说明
        ///     调用AssociationReference(PropertyInfo)方法。
        /// </summary>
        /// <param name="expression">lamda表达式</param>
        public AssociationReferenceConfiguration<TEntity> AssociationReference<TAssociation>(
            Expression<Func<TEntity, TAssociation>> expression) where TAssociation : class
        {
            if (expression.Body is MemberExpression member)
            {
                //获取实体模型对应类型的属性
                var property = typeof(TEntity).GetProperty(member.Member.Name);

                if (property == null)
                    throw new ArgumentNullException(nameof(member.Member.Name),
                        $"无法在实体型{typeof(TEntity).FullName}内找到到关联引用{member.Member.Name}");
                return AssociationReference(property);
            }

            throw new ArgumentException("不能使用非属性访问表达式配置关联引用");
        }

        /// <summary>
        ///     启动对关联端上基于当前关联定义的关联引用的配置，如果相应的配置项未创建则新建一个。
        ///     实施说明
        ///     如果_associationReferenceConfiguration不为null，直接返回。应寄存新建的配置器，避免重复创建。
        ///     以访问器的名称创建关联引用配置项实例，然后进行如下配置：
        ///     （1）检测是否存在get方法（含protected internal），如果存在配置取值器；
        ///     （2）检测是否存在set方法（含protected internal），如果存在配置设值器；
        ///     （3）如果存在get方法且为virtual则配置加载触发器；
        ///     （4）设置左端名。
        ///     参见顺序图“配置隐式关联”。
        /// </summary>
        /// <param name="propInfo">关联引用的访问器。</param>
        private AssociationReferenceConfiguration<TEntity> AssociationReference(PropertyInfo propInfo)
        {
            if (_associationReferenceConfiguration == null)
            {
                //名称
                var name = propInfo.Name;

                //是否集合属性
                var isMultiple = Utils.GetIsMultipe(propInfo, out var type);

                //首先 查找端的配置
                var entityTypeConfiguration =
                    (EntityTypeConfiguration<TEntity>)AssociationConfiguratorBuilder.ModelBuilder.FindConfiguration(
                        typeof(TEntity));

                //创建关联应用配置类型
                var assRefCfgType =
                    typeof(AssociationReferenceConfiguration<,>).MakeGenericType(typeof(TEntity), type);

                //创建关联应用配置类型 实例
                var configuration =
                    Activator.CreateInstance(assRefCfgType, name, isMultiple, _endIndex,
                        AssociationConfiguratorBuilder) as IAssociationReferenceConfigurator;

                if (configuration == null)
                    throw new ArgumentException($"创建类型为{propInfo.ReflectedType}.{propInfo.Name}关联引用配置类型失败.");

                //取值器
                configuration.HasValueGetter(propInfo);
                //设值器
                if (propInfo.SetMethod != null)
                {
                    var parType = propInfo.SetMethod.GetParameters()[0].ParameterType;
                    var actionType = typeof(Action<,>).MakeGenericType(propInfo.DeclaringType, parType);
                    var del = propInfo.SetMethod.CreateDelegate(actionType);

                    var model = EValueSettingMode.Assignment;
                    if (parType != typeof(string) && parType.GetInterface("IEnumerable") != null)
                        model = EValueSettingMode.Appending;

                    configuration.HasValueSetter(ValueSetter.Create(del, model));
                }

                //保存
                _associationReferenceConfiguration = (AssociationReferenceConfiguration<TEntity>)configuration;
                _referenceConfigurator = configuration;
                //配置左端
                _referenceConfigurator.HasLeftEnd(_name);
                //加入实体型的配置
                entityTypeConfiguration.AddAssociationReference(_associationReferenceConfiguration);
            }

            return _associationReferenceConfiguration;
        }

        /// <summary>
        ///     根据元素配置项包含的元数据信息创建元素实例
        ///     本方法由派生类实现
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected override TypeElement CreateReally(ObjectDataModel model)
        {
            var endEntityType = model.GetEntityType(_entityType);

            if (endEntityType == null)
                throw new ArgumentException($"{_entityType.Name}未在模型中注册.");

            //根据配置项数据创建模型对象并设值
            var end = new AssociationEnd(Name, endEntityType)
            {
                Mappings = Mappings,
                LoadingTriggers = BehaviorTriggers,
                EnableLazyLoading = _enableLazyLoading,
                IsMultiple = IsMultiple,
                ValueGetter = _valueGetter,
                ValueSetter = _valueSetter,
                DefaultAsNew = DefaultAsNew,
                LoadingPriority = LoadingPriority,
                IsAggregated = _isAggregated
            };
            return end;
        }
    }
}