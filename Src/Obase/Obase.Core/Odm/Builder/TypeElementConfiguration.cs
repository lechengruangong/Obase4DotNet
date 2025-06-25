/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：类型元素配置,提供类型元素配置项提供基础实现.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:55:37
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     为属性配置项、关联引用配置项、关联端配置项提供基础实现。
    /// </summary>
    public abstract class TypeElementConfiguration
    {
        /// <summary>
        ///     元素扩展配置器。
        /// </summary>
        protected readonly List<ElementExtensionConfiguration> ExtensionConfigs =
            new List<ElementExtensionConfiguration>();

        /// <summary>
        ///     名称
        /// </summary>
        protected string _name;

        /// <summary>
        ///     创建当前元素配置项的类型配置项。
        /// </summary>
        protected StructuralTypeConfiguration _typeConfiguration;

        /// <summary>
        ///     取值器
        /// </summary>
        protected IValueGetter _valueGetter;

        /// <summary>
        ///     设值器
        /// </summary>
        protected IValueSetter _valueSetter;

        /// <summary>
        ///     指示元素是否具有多重性，即其值是否为集合。
        /// </summary>
        protected bool IsMultiple;

        /// <summary>
        ///     名称访问器
        /// </summary>
        public string Name => _name;

        /// <summary>
        ///     获取元素类型。
        /// </summary>
        public abstract EElementType ElementType { get; }

        /// <summary>
        ///     获取行为触发器，对于属性是指修改触发器，对于关联引用和关联端是加载触发器。
        /// </summary>
        public abstract List<IBehaviorTrigger> BehaviorTriggers { get; }

        /// <summary>
        ///     根据元素配置项包含的元数据信息创建元素实例。
        /// </summary>
        /// <param name="model">对象数据模型</param>
        internal TypeElement Create(ObjectDataModel model)
        {
            //调用实现类的CreateReally方法创建元素实例
            var typeElement = CreateReally(model);
            //获取当前元素配置项的类型扩展配置项
            var elementExtensions = ExtensionConfigs;
            //加入元素类型配置的扩展
            foreach (var elementExtensionConfiguration in elementExtensions)
                typeElement.AddExtension(elementExtensionConfiguration.MakeExtension());

            return typeElement;
        }

        /// <summary>
        ///     为元素配置项设置一个指定类型的扩展配置器，如果指定类型的配置器已存在，返回该配置器。
        /// </summary>
        /// <param name="configType">扩展配置器的类型，须继承自ElementExtensionConfiguration。</param>
        public ElementExtensionConfiguration HasExtension(Type configType)
        {
            try
            {
                //是否存在指定类型的扩展配置器 如有 直接返回
                var ext = ExtensionConfigs.FirstOrDefault(p => p.GetType() == configType);
                if (ext != null)
                    return ext;
                //构造指定类型的扩展配置器
                var extensionConfiguration =
                    (ElementExtensionConfiguration)Activator.CreateInstance(configType);
                //加入到扩展配置器列表中
                ExtensionConfigs.Add(extensionConfiguration);
                return extensionConfiguration;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"添加扩展配置器失败,{configType.Name}没有适合的无参构造函数", nameof(configType), e);
            }
        }

        /// <summary>
        ///     根据元素配置项包含的元数据信息创建元素实例
        ///     本方法由派生类实现
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        protected abstract TypeElement CreateReally(ObjectDataModel model);
    }

    /// <summary>
    ///     为属性配置项、关联引用配置项、关联端配置项提供基础实现。
    /// </summary>
    /// <typeparam name="TStructural">表示元素所属对象的类型</typeparam>
    /// <typeparam name="TConfiguration">配置项的具体类型</typeparam>
    public abstract class
        TypeElementConfiguration<TStructural, TConfiguration> :
        TypeElementConfiguration, ITypeElementConfigurator
        where TConfiguration : TypeElementConfiguration<TStructural, TConfiguration>
    {
        /// <summary>
        ///     创建类型元素配置项实例
        /// </summary>
        /// <param name="name">元素（属性、关联引用、关联端）名称</param>
        /// <param name="isMultiple">指示元素是否具有多重性，即其值是否为集合。</param>
        /// <param name="typeConfiguration">创建当前元素配置项的类型配置项。</param>
        protected TypeElementConfiguration(string name, bool isMultiple, StructuralTypeConfiguration typeConfiguration)
        {
            _name = name;
            IsMultiple = isMultiple;
            _typeConfiguration = typeConfiguration;
        }


        /// <summary>
        ///     设值器
        /// </summary>
        internal IValueGetter ValueGetter => _valueGetter;


        /// <summary>
        ///     取值器
        /// </summary>
        internal IValueSetter ValueSetter => _valueSetter;

        /// <summary>
        ///     类型配置项。
        /// </summary>
        internal StructuralTypeConfiguration TypeConfiguration => _typeConfiguration;

        /// <summary>
        ///     为元素配置项设置一个扩展配置器。
        /// </summary>
        /// <param name="configType">扩展配置器的类型，须继承自ElementExtensionConfiguration。</param>
        ElementExtensionConfiguration ITypeElementConfigurator.HasExtension(Type configType)
        {
            return base.HasExtension(configType);
        }


        /// <summary>
        ///     为元素配置项设置一个扩展配置器
        /// </summary>
        /// <typeparam name="TExtensionConfiguration">扩展配置器的类型，须继承自ElementExtensionConfiguration。</typeparam>
        /// <returns></returns>
        ElementExtensionConfiguration ITypeElementConfigurator.HasExtension<TExtensionConfiguration>()
        {
            var extensionConfigurationType = typeof(TExtensionConfiguration);
            try
            {
                //查找是否存在指定类型的扩展配置器 如有 直接返回
                var ext = ExtensionConfigs.FirstOrDefault(p => p.GetType() == extensionConfigurationType);
                if (ext != null)
                    return ext;
                //构造指定类型的扩展配置器
                var extensionConfiguration =
                    (ElementExtensionConfiguration)Activator.CreateInstance(extensionConfigurationType);
                //加入到扩展配置器列表中
                ExtensionConfigs.Add(extensionConfiguration);
                return extensionConfiguration;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"添加扩展配置器失败,{extensionConfigurationType.Name}没有适合的无参构造函数",
                    nameof(extensionConfigurationType), e);
            }
        }

        /// <summary>
        ///     为类型元素设置取值器。
        /// </summary>
        /// <param name="valueGetter">取值器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void ITypeElementConfigurator.HasValueGetter(IValueGetter valueGetter, bool overrided)
        {
            //如果覆盖了既有配置，则直接设置取值器
            if (overrided)
            {
                HasValueGetter(valueGetter);
            }
            else
            {
                //不覆盖的情形 如果没有设置过取值器，则设置取值器
                if (ValueGetter == null)
                    HasValueGetter(valueGetter);
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
        void ITypeElementConfigurator.HasValueGetter(MethodInfo method, bool overrided)
        {
            //如果覆盖了既有配置，则直接设置取值器
            if (overrided)
            {
                HasValueGetter(method);
            }
            else
            {
                //不覆盖的情形 如果没有设置过取值器，则设置取值器
                if (ValueGetter == null)
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
        void ITypeElementConfigurator.HasValueGetter(PropertyInfo property, bool overrided)
        {
            //如果覆盖了既有配置，则直接设置取值器
            if (overrided)
            {
                HasValueGetter(property);
            }
            else
            {
                //不覆盖的情形 如果没有设置过取值器，则设置取值器
                if (ValueGetter == null)
                    HasValueGetter(property);
            }
        }

        /// <summary>
        ///     使用表示类型元素的字段为类型元素创建取值器。
        ///     如果字段的数据类型与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        /// <param name="field">表示类型元素的字段。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void ITypeElementConfigurator.HasValueGetter(FieldInfo field, bool overrided)
        {
            //如果覆盖了既有配置，则直接设置取值器
            if (overrided)
            {
                HasValueGetter(field);
            }
            else
            {
                //不覆盖的情形 如果没有设置过取值器，则设置取值器
                if (ValueGetter == null)
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
        void ITypeElementConfigurator.HasValueGetter(string memberName, MemberTypes memberType, bool overrided)
        {
            //如果覆盖了既有配置，则直接设置取值器
            if (overrided)
            {
                HasValueGetter(memberName, memberType);
            }
            else
            {
                //不覆盖的情形 如果没有设置过取值器，则设置取值器
                if (ValueGetter == null)
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
            //如果覆盖了既有配置，则直接设置取值器
            if (overrided)
            {
                HasValueGetter(_name, memberType);
            }
            else
            {
                //不覆盖的情形 如果没有设置过取值器，则设置取值器
                if (ValueGetter == null)
                    HasValueGetter(_name, memberType);
            }
        }

        /// <summary>
        ///     使用与类型元素同名的属性访问器为类型元素创建取值器。
        ///     如果该成员与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        public void HasValueGetter(bool overrided)
        {
            //如果覆盖了既有配置，则直接设置取值器
            if (overrided)
            {
                HasValueGetter(_name, MemberTypes.Property);
            }
            else
            {
                //不覆盖的情形 如果没有设置过取值器，则设置取值器
                if (ValueGetter == null)
                    HasValueGetter(_name, MemberTypes.Property);
            }
        }

        /// <summary>
        ///     为类型元素设置设值器。
        /// </summary>
        /// <param name="valueSetter">设值器。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void ITypeElementConfigurator.HasValueSetter(IValueSetter valueSetter, bool overrided)
        {
            //如果覆盖了既有配置，则直接设置设值器
            if (overrided)
            {
                HasValueSetter(valueSetter);
            }
            else
            {
                //不覆盖的情形 如果没有设置过设值器，则设置设值器
                if (ValueSetter == null)
                    HasValueSetter(valueSetter);
            }
        }

        /// <summary>
        ///     使用指定的类成员为类型元素创建设值器。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="memberType">成员的类型。</param>
        /// <param name="overrided">是否覆盖既有配置</param>
        void ITypeElementConfigurator.HasValueSetter(string memberName, MemberTypes memberType, bool overrided)
        {
            //如果覆盖了既有配置，则直接设置设值器
            if (overrided)
            {
                HasValueSetter(memberName, memberType);
            }
            else
            {
                //不覆盖的情形 如果没有设置过设值器，则设置设值器
                if (ValueSetter == null)
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
        void ITypeElementConfigurator.HasValueSetter(PropertyInfo property, bool overrided)
        {
            //如果覆盖了既有配置，则直接设置设值器
            if (overrided)
            {
                HasValueSetter(property);
            }
            else
            {
                //不覆盖的情形 如果没有设置过设值器，则设置设值器
                if (ValueSetter == null)
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
            //如果覆盖了既有配置，则直接设置设值器
            if (overrided)
            {
                HasValueSetter(_name, memberType);
            }
            else
            {
                //不覆盖的情形 如果没有设置过设值器，则设置设值器
                if (ValueSetter == null)
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
            //如果覆盖了既有配置，则直接设置设值器
            if (overrided)
            {
                HasValueSetter(appendingMethod, EValueSettingMode.Assignment);
            }
            else
            {
                //不覆盖的情形 如果没有设置过设值器，则设置设值器
                if (ValueSetter == null)
                    HasValueSetter(appendingMethod, EValueSettingMode.Assignment);
            }
        }

        /// <summary>
        ///     使用与类型元素同名的属性访问器为类型元素创建设值器。
        /// </summary>
        public void HasValueSetter(bool overrided)
        {
            //如果覆盖了既有配置，则直接设置设值器
            if (overrided)
            {
                HasValueSetter(_name, MemberTypes.Property);
            }
            else
            {
                //不覆盖的情形 如果没有设置过设值器，则设置设值器
                if (ValueSetter == null)
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
        void ITypeElementConfigurator.HasValueSetter(FieldInfo field, bool overrided)
        {
            //如果覆盖了既有配置，则直接设置设值器
            if (overrided)
            {
                HasValueSetter(field);
            }
            else
            {
                //不覆盖的情形 如果没有设置过设值器，则设置设值器
                if (ValueSetter == null)
                    HasValueSetter(field);
            }
        }

        /// <summary>
        ///     进入当前元素所属类型的配置项。
        /// </summary>
        IStructuralTypeConfigurator ITypeElementConfigurator.Upward()
        {
            //返回所属类型的配置项
            return (IStructuralTypeConfigurator)_typeConfiguration;
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
        void ITypeElementConfigurator.HasValueSetter(MethodInfo method, EValueSettingMode mode, bool overrided)
        {
            //如果覆盖了既有配置，则直接设置设值器
            if (overrided)
            {
                HasValueSetter(method, mode);
            }
            else
            {
                //不覆盖的情形 如果没有设置过设值器，则设置设值器
                if (ValueSetter == null)
                    HasValueSetter(method, mode);
            }
        }

        /// <summary>
        ///     设置取值器。
        /// </summary>
        /// <param name="valueGetter"></param>
        public TConfiguration HasValueGetter(IValueGetter valueGetter)
        {
            //直接设置
            _valueGetter = valueGetter;
            return (TConfiguration)this;
        }

        /// <summary>
        ///     使用一个能够获取类型元素的值且返回值为单个对象的委托为不具备多重性的类型元素创建取值器。
        ///     注：当元素具备多重性时，该方法将引发异常。
        /// </summary>
        /// <typeparam name="TProperty">表示元素的类型。对于属性，它表示属性值类型；对于关联引用，它表示关联类型；对于关联端，它表示关联端的类型。</typeparam>
        /// <param name="getValue">获取元素值的委托。</param>
        public TConfiguration HasValueGetter<TProperty>(Func<TStructural, TProperty> getValue)
        {
            if (IsMultiple) throw new ArgumentException($"{Name}类型的设值器为多重性,不能设置单一设值器.");
            //创建一个委托取值器
            var valueGetter = new DelegateValueGetter<TStructural, TProperty>(getValue);
            return HasValueGetter(valueGetter);
        }

        /// <summary>
        ///     使用一个能够获取类型元素的值且返回值为对象序列的委托为具备多重性的类型元素创建取值器。
        ///     注：当元素不具备多重性时，该方法将引发异常。
        /// </summary>
        /// <typeparam name="TProperty">表示元素的类型。对于属性，它表示属性值类型；对于关联引用，它表示关联类型；对于关联端，它表示关联端的类型。</typeparam>
        /// <param name="getValue">获取元素值的委托。</param>
        public TConfiguration HasValueGetter<TProperty>(Func<TStructural, IEnumerable<TProperty>> getValue)
        {
            if (!IsMultiple) throw new ArgumentException($"{Name}类型的设值器为单一性,不能设置多重设值器.");
            //创建一个委托取值器
            var valueGetter = new DelegateValueGetter<TStructural, IEnumerable<TProperty>>(getValue);
            return HasValueGetter(valueGetter);
        }

        /// <summary>
        ///     使用一个能够获取类型元素值的方法为类型元素创建取值器。
        ///     如果该方法的返回值类型与元素的IsMultiple属性不匹配，则引发异常。
        ///     实施建议：
        ///     调用MethodInfo类的CreateDelegate方法创建代表该方法的委托，然后创建委托取值器。
        /// </summary>
        /// <param name="method">获取元素值的方法。</param>
        public TConfiguration HasValueGetter(MethodInfo method)
        {
            if (IsMultiple)
            {
                //包装要取的值
                var ienumableType = typeof(IEnumerable<>).MakeGenericType(method.ReturnType.GetGenericArguments()[0]);
                var delegateType = typeof(Func<,>).MakeGenericType(typeof(TStructural), ienumableType);
                //创建委托
                var delegateFunc = method.CreateDelegate(delegateType);
                if (delegateFunc == null) throw new ArgumentException($"{typeof(TStructural)}的{method.Name}方法无法创建委托.");
                //创建取值器
                var valueGetter = typeof(DelegateValueGetter<,>).MakeGenericType(typeof(TStructural), ienumableType);
                var valueGetterInstance = Activator.CreateInstance(valueGetter, delegateFunc) as IValueGetter;
                return HasValueGetter(valueGetterInstance);
            }
            else
            {
                var delegateType = typeof(Func<,>).MakeGenericType(typeof(TStructural), method.ReturnType);
                //创建委托
                var delegateFunc = method.CreateDelegate(delegateType);
                if (delegateFunc == null) throw new ArgumentException($"{typeof(TStructural)}的{method.Name}方法无法创建委托.");
                //创建取值器
                var valueGetter =
                    typeof(DelegateValueGetter<,>).MakeGenericType(typeof(TStructural), method.ReturnType);
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
        public TConfiguration HasValueGetter(PropertyInfo property)
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
                var getter = typeof(DelegateValueGetter<,>).MakeGenericType(typeof(TStructural), property.PropertyType);
                var getterObj = Activator.CreateInstance(getter, exp.Compile()) as IValueGetter;
                return HasValueGetter(getterObj);
            }

            //不是结构 普通的方法取值
            return HasValueGetter(property.GetMethod);
        }

        /// <summary>
        ///     使用表示类型元素的字段为类型元素创建取值器。
        ///     如果字段的数据类型与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        /// <param name="field">表示类型元素的字段。</param>
        public TConfiguration HasValueGetter(FieldInfo field)
        {
            //是IEnumable并且不是string 则认为是多重的
            var filedIsMulty = field.FieldType.GetInterface("IEnumerable") != null && field.FieldType != typeof(string);

            if (filedIsMulty != IsMultiple) throw new ArgumentException($"{field.Name}与目标的多重性不一致.");
            //构造一个字段取值器
            var filedSetter = new FieldValueGetter(field);

            return HasValueGetter(filedSetter);
        }

        /// <summary>
        ///     使用指定的类成员为类型元素创建取值器。
        ///     如果该成员与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="memberType">成员的类型。</param>
        public TConfiguration HasValueGetter(string memberName, MemberTypes memberType)
        {
            //获取公开的实例成员
            var memberInfo = typeof(TStructural).GetMember(memberName, memberType,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (memberInfo.Length <= 0) throw new ArgumentException($"无法获取到成员{memberName}.");
            //根据成员类型判断
            switch (memberType)
            {
                case MemberTypes.Field:
                {
                    return HasValueGetter((FieldInfo)memberInfo[0]);
                }
                case MemberTypes.Property:
                {
                    return HasValueGetter((PropertyInfo)memberInfo[0]);
                }
                case MemberTypes.Method:
                {
                    return HasValueGetter((MethodInfo)memberInfo[0]);
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(memberType), memberType, $"取值器不支持此成员类型{memberType}");
            }
        }

        /// <summary>
        ///     使用与类型元素同名的类成员为类型元素创建取值器。
        ///     如果该成员与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        /// <typeparam name="TProperty">表示元素的类型。对于属性，它表示属性值类型；对于关联引用，它表示关联类型；对于关联端，它表示关联端的类型。</typeparam>
        /// <param name="memberType">同名成员的类型。</param>
        public TConfiguration HasValueGetter<TProperty>(MemberTypes memberType)
        {
            return HasValueGetter(typeof(TProperty).Name, memberType);
        }

        /// <summary>
        ///     使用与类型元素同名的属性访问器为类型元素创建取值器。
        ///     如果该成员与元素的IsMultiple属性不匹配，则引发异常。
        /// </summary>
        /// <typeparam name="TProperty">表示元素的类型。对于属性，它表示属性值类型；对于关联引用，它表示关联类型；对于关联端，它表示关联端的类型。</typeparam>
        public TConfiguration HasValueGetter<TProperty>()
        {
            return HasValueGetter(typeof(TProperty).Name, MemberTypes.Field);
        }

        /// <summary>
        ///     为lambda表达式指示的元素创建设值器，该lambda表达式的主体须为MemberExpression，其访问的成员代表要设值的元素。
        /// </summary>
        /// <typeparam name="TProperty">作为lambda表达式主体的MemberExpression的类型，亦即元素值的类型。</typeparam>
        /// <param name="expression">表达式</param>
        /// 实施说明
        /// 从Lambda表达式取出Body，将其转换为MemberExpression，然后通过其Member属性获取PropertyInfo。
        public TConfiguration HasValueSetter<TProperty>(Expression<Func<TStructural, TProperty>> expression)
        {
            //解析表达式
            var member = (MemberExpression)expression.Body;
            //获取反射属性
            //获取实体模型对应类型的属性
            var property = typeof(TStructural).GetProperty(member.Member.Name);
            if (property == null)
                throw new ArgumentException($"{typeof(TStructural).FullName}无法找到{member.Member.Name},不能配置设值器.");
            return HasValueSetter(property);
        }

        /// <summary>
        ///     为lambda表达式指示的元素创建设值器，该lambda表达式的主体须为MemberExpression，其访问的成员代表要设值的元素。
        /// </summary>
        /// <param name="propertyExp">表示属性访问器的Lambda表达式。</param>
        /// <param name="valueCreator"></param>
        /// <typeparam name="TProperty">作为lambda表达式主体的MemberExpression的类型，亦即元素值的类型。</typeparam>
        /// <typeparam name="TElement">值序列项的类型。</typeparam>
        /// 实施说明:
        /// 从Lambda表达式取出Body，将其置换为MemberExpression，然后通过其Member属性获取PropertyInfo。
        /// 从上述PropertyInfo获取SetMethod，然后调用CreateDelegate方法创建Action
        /// {TStructural, TProperty}
        /// 委托。
        /// 使用上述委托实例化DelegateEnumerableValueSetter{TStructural, TProperty, TElement}。
        public TConfiguration HasValueSetter<TProperty, TElement>(Expression<Func<TStructural, TProperty>> propertyExp,
            Func<IEnumerable<TElement>, TProperty> valueCreator)
        {
            if (propertyExp.Body is MemberExpression member)
            {
                var property = typeof(TStructural).GetProperty(member.Member.Name);
                if (property != null)
                {
                    var setMethod = property.SetMethod;
                    //根据设值方法的参数类型创建委托
                    var setDlegate = setMethod.CreateDelegate(typeof(Action<TStructural, TProperty>));
                    var setterType =
                        typeof(DelegateEnumerableValueSetter<,,>).MakeGenericType(typeof(TStructural),
                            typeof(TProperty),
                            typeof(TElement));
                    return HasValueSetter((ValueSetter)Activator.CreateInstance(setterType, setDlegate, valueCreator));
                }
            }

            throw new Exception("属性访问器的Lambda表达式主体须为MemberExpression。");
        }

        /// <summary>
        ///     使用能够修改元素值的委托为类型元素创建设值器,
        ///     当设值模式为Assignment时该委托为元素赋值，
        ///     当设值模式为Appending时该委托在元素值序列尾部追加项。
        /// </summary>
        /// <typeparam name="TValue">
        ///     Assignment模式下为元素值的类型，Appending模式下为元素值序列项的类型。
        /// </typeparam>
        /// <param name="setValue">表示属性访问器的Lambda表达式。</param>
        /// <param name="mode">设值模式。</param>
        /// 实施说明
        /// 使用ValueSetter的Create方法创建设值器。
        public TConfiguration HasValueSetter<TValue>(Action<TStructural, TValue> setValue, EValueSettingMode mode)
        {
            return HasValueSetter(Odm.ValueSetter.Create(setValue, mode));
        }

        /// <summary>
        ///     使用能够为元素赋值的委托为类型元素创建设值器。
        /// </summary>
        /// <param name="setValue">表示属性访问器的Lambda表达式。</param>
        /// <param name="valueCreator"></param>
        /// <typeparam name="TValue">元素值的类型。</typeparam>
        /// <typeparam name="TElement">值序列项的类型。</typeparam>
        /// 实施说明:
        /// 实例化DelegateEnumerableValueSetter
        /// {TStructural, TValue, TElement}
        public TConfiguration HasValueSetter<TValue, TElement>(Action<TStructural, TValue> setValue,
            Func<IEnumerable<TElement>, TValue> valueCreator)
            where TValue : IEnumerable<TElement>
        {
            //使用委托构造委托的可枚举设值器
            var type = typeof(DelegateEnumerableValueSetter<,,>).MakeGenericType(typeof(TStructural), typeof(TValue),
                typeof(TElement));
            return HasValueSetter((ValueSetter)Activator.CreateInstance(type, setValue, valueCreator));
        }

        /// <summary>
        ///     设置设值器。
        /// </summary>
        /// <param name="valueSetter">对象设值器接口</param>
        public TConfiguration HasValueSetter(IValueSetter valueSetter)
        {
            _valueSetter = valueSetter;
            return (TConfiguration)this;
        }

        /// <summary>
        ///     使用一个能够为类型元素设值的方法为类型元素创建设值器。
        /// </summary>
        /// <param name="method">为类型元素设值的方法。</param>
        /// <param name="mode">设值模式。</param>
        /// 检测方法的DeclaringType，如果为引用类型，使用MethodInfo.CreateDelegate方法创建Action
        /// {TStructural, TElement}
        /// 委托；
        /// 如果是结构体，使用Emit创建SetValue
        /// {TStructural, TElement}
        /// 委托。
        /// 使用上述委托，调用ValueSetter的Create方法创建设值器。
        public TConfiguration HasValueSetter(MethodInfo method, EValueSettingMode mode)
        {
            //只支持单参数的设值方法
            if (method.GetParameters().Length != 1) throw new ArgumentException("设值器方法只能有一个参数.");
            Delegate setDelegate;
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
                setDelegate = method.CreateDelegate(setValueFuncType);
            }
            //引用类型
            else
            {
                var typeAction = typeof(Action<,>).MakeGenericType(types);
                setDelegate = method.CreateDelegate(typeAction);
            }

            return HasValueSetter(Odm.ValueSetter.Create(setDelegate, mode));
        }

        /// <summary>
        ///     使用一个能够为类型元素设值的Property为类型元素创建设值器。
        /// </summary>
        /// <param name="property">为类型元素设值的属性访问器。</param>
        /// 实施说明
        /// 取出Property的Set方法，然后调用HasValueSetter(methodInfo, mode)方法。
        public TConfiguration HasValueSetter(PropertyInfo property)
        {
            var setMethod = property.GetSetMethod(true);
            if (setMethod == null) throw new Exception($"Property({property.Name})没有Set方法");
            //用Set方法创建设值器
            return HasValueSetter(setMethod, EValueSettingMode.Assignment);
        }

        /// <summary>
        ///     使用表示类型元素的字段为类型元素创建设值器。
        /// </summary>
        /// <param name="field">表示类型元素的字段。</param>
        /// 实施说明
        /// 使用ValueSetter类的Create方法创建设值器。
        public TConfiguration HasValueSetter(FieldInfo field)
        {
            return HasValueSetter(Odm.ValueSetter.Create(field));
        }

        /// <summary>
        ///     使用指定的类成员为类型元素创建设值器。
        /// </summary>
        /// <param name="memberName">成员的名称。</param>
        /// <param name="memberType">成员的类型。</param>
        public TConfiguration HasValueSetter(string memberName, MemberTypes memberType)
        {
            //获取公开的实例成员
            var memberInfo = typeof(TStructural).GetMember(memberName, memberType,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (memberInfo.Length <= 0) throw new ArgumentException($"无法获取到成员{memberName}.");
            //根据成员类型判断
            switch (memberType)
            {
                case MemberTypes.Field:
                {
                    return HasValueSetter((FieldInfo)memberInfo[0]);
                }
                case MemberTypes.Property:
                {
                    return HasValueSetter((PropertyInfo)memberInfo[0]);
                }
                case MemberTypes.Method:
                {
                    //此处无法确定eValueSettingMode
                    throw new InvalidOperationException("不支持只用Method和MethodName构造设值器");
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(memberType), memberType, $"设值器不支持此成员类型{memberType}");
            }
        }

        /// <summary>
        ///     使用与类型元素同名的类成员为类型元素创建设值器。
        /// </summary>
        /// <param name="memberType">成员的类型。</param>
        /// <typeparam name="TProperty">表示元素的类型。对于属性，它表示属性值类型；对于关联引用，它表示关联类型；对于关联端，它表示关联端的类型。</typeparam>
        public TConfiguration HasValueSetter<TProperty>(MemberTypes memberType)
        {
            return HasValueSetter(typeof(TProperty).Name, memberType);
        }

        /// <summary>
        ///     使用与类型元素同名的属性访问器为类型元素创建设值器。
        /// </summary>
        /// <typeparam name="TProperty">表示元素的类型。对于属性，它表示属性值类型；对于关联引用，它表示关联类型；对于关联端，它表示关联端的类型。</typeparam>
        public TConfiguration HasValueSetter<TProperty>()
        {
            return HasValueSetter(typeof(TProperty).Name, MemberTypes.Field);
        }

        /// <summary>
        ///     为元素配置项设置一个指定类型的扩展配置器，如果指定类型的配置器已存在，返回该配置器。
        /// </summary>
        /// <typeparam name="TExtensionConfiguration">元素扩展配置器</typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public TExtensionConfiguration HasExtension<TExtensionConfiguration>()
            where TExtensionConfiguration : ElementExtensionConfiguration, new()
        {
            //获取扩展配置器类型
            var extensionConfigurationType = typeof(TExtensionConfiguration);
            try
            {
                //调用基类方法配置
                return (TExtensionConfiguration)HasExtension(extensionConfigurationType);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"添加扩展配置器失败,{extensionConfigurationType.Name}没有适合的无参构造函数",
                    nameof(extensionConfigurationType), e);
            }
        }
    }
}