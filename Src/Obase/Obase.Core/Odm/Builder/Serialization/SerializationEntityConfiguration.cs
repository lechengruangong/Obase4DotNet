/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化实体配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-25 17:39:12
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Obase.Core.Common;
using Obase.Core.Odm.Serialization;

namespace Obase.Core.Odm.Builder.Serialization
{
    /// <summary>
    ///     序列化实体基础配置
    /// </summary>
    public abstract class SerializationEntityConfiguration
    {
        /// <summary>
        ///     创建序列化实体方法
        /// </summary>
        /// <returns></returns>
        internal SerializationEntity Create()
        {
            //调用实现类的CreateReally方法构建模型类型
            return CreateReally();
        }

        /// <summary>
        ///     根据类型配置项中的元数据构建模型类型
        ///     本方法由派生类实现
        /// </summary>
        /// <returns>序列化实体类型</returns>
        protected abstract SerializationEntity CreateReally();
    }

    /// <summary>
    ///     序列化实体配置
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class SerializationEntityConfiguration<T> : SerializationEntityConfiguration
    {
        /// <summary>
        ///     忽略的属性集合
        /// </summary>
        private readonly HashSet<string> _ignoredProperties = new HashSet<string>();

        /// <summary>
        ///     建模器
        /// </summary>
        private readonly ModelBuilder _modelBuilder;

        /// <summary>
        ///     序列化元素的字典
        ///     对于属性 key为属性名称 value为属性的配置项
        ///     对于构造参数 key为参数Index value为参数的配置项
        ///     对于引用 key为引用名称 value为引用的配置项
        /// </summary>
        private readonly Dictionary<string, SerializationTypeElementConfiguration<T>>
            _serializeTypeElementConfigurations = new Dictionary<string, SerializationTypeElementConfiguration<T>>();

        /// <summary>
        ///     使用的构造器
        /// </summary>
        private SerializationConstructorConfiguration<T> _constructor;

        /// <summary>
        ///     初始化序列化实体配置
        /// </summary>
        /// <param name="modelBuilder">建模器</param>
        public SerializationEntityConfiguration(ModelBuilder modelBuilder)
        {
            _modelBuilder = modelBuilder;
        }

        /// <summary>
        ///     手动配置属性方法
        ///     根据名称和值类型创建一个属性配置项并添加到序列化元素的字典中
        ///     不会设置取值器和设值器 需要用户手动设置
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="valueType">值类型</param>
        /// <returns>属性配置项</returns>
        public SerializationAttributeConfiguration<T> Attribute(string name, Type valueType)
        {
            if (!PrimitiveType.IsObasePrimitiveType(valueType))
                throw new ArgumentException("只有Obase的基元类型可以作为序列化实体属性.");

            //如果有 从字典中取出 否则创建一个新的属性配置项并添加到字典中
            SerializationAttributeConfiguration<T> result;
            if (_serializeTypeElementConfigurations.TryGetValue(name, out var configuration))
            {
                result = (SerializationAttributeConfiguration<T>)configuration;
            }
            else
            {
                result = new SerializationAttributeConfiguration<T>(name, valueType);
                _serializeTypeElementConfigurations[name] = result;
            }

            return result;
        }

        /// <summary>
        ///     自动配置属性方法
        ///     根据名称和自动侦测的类型创建一个属性配置项并添加到序列化元素的字典中
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>属性配置项</returns>
        public SerializationAttributeConfiguration<T> Attribute(string name)
        {
            var property = typeof(T).GetProperty(name);
            if (property == null)
                throw new ArgumentException($"未找到名称为{name}的属性.");
            //进行配置
            var attribute = Attribute(name, property.PropertyType);
            //取值器和设值器
            attribute.HasValueGetter(MakeValueGetter(property));
            attribute.HasValueSetter(MakeValueSetter(property));
            return attribute;
        }

        /// <summary>
        ///     自动配置属性方法
        ///     根据表达式代表的名称和自动侦测的类型创建一个属性配置项并添加到序列化元素的字典中
        /// </summary>
        /// <typeparam name="TResult">目标类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>属性配置项</returns>
        public SerializationAttributeConfiguration<T> Attribute<TResult>(
            Expression<Func<T, TResult>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
                //获取表达式代表的属性名称
                return Attribute(memberExpression.Member.Name);

            throw new ArgumentException("不能使用非属性访问表达式配置属性.");
        }

        /// <summary>
        ///     启动一个序列化构造器配置
        /// </summary>
        /// <param name="constructor">构造函数</param>
        /// <returns></returns>
        public SerializationConstructorConfiguration<T> HasConstructor(ConstructorInfo constructor)
        {
            if (constructor == null)
                throw new ArgumentException("不能使用空的构造函数配置序列化构造器.");
            _constructor = new SerializationConstructorConfiguration<T>(constructor);
            return _constructor;
        }

        /// <summary>
        ///     手动配置引用方法
        ///     根据名称和是否为多重的创建一个引用配置项并添加到序列化元素的字典中
        ///     不会设置取值器和设值器 需要用户手动设置
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="isMultiple">引用是单值的还是多重的</param>
        /// <returns>引用配置项</returns>
        public SerializationReferenceConfiguration<T> Reference(string name, bool isMultiple)
        {
            //如果有 从字典中取出 否则创建一个新的属性配置项并添加到字典中
            SerializationReferenceConfiguration<T> result;
            if (_serializeTypeElementConfigurations.TryGetValue(name, out var configuration))
            {
                result = (SerializationReferenceConfiguration<T>)configuration;
            }
            else
            {
                result = new SerializationReferenceConfiguration<T>(name, isMultiple);
                _serializeTypeElementConfigurations[name] = result;
            }

            return result;
        }

        /// <summary>
        ///     自动配置引用方法
        ///     根据名称和自动侦测的多重性创建一个引用配置项并添加到序列化元素的字典中
        ///     不会设置取值器和设值器 需要用户手动设置
        /// </summary>
        /// <param name="name">名称</param>
        /// <returns>引用配置项</returns>
        public SerializationReferenceConfiguration<T> Reference(string name)
        {
            var property = typeof(T).GetProperty(name);
            if (property == null)
                throw new ArgumentException($"未找到名称为{name}的属性.");
            var isMultiple = Utils.GetIsMultiple(property, out _);
            //进行配置
            var reference = Reference(name, isMultiple);
            //取值器和设值器
            reference.HasValueGetter(MakeValueGetter(property));
            reference.HasValueSetter(MakeValueSetter(property));
            return reference;
        }

        /// <summary>
        ///     自动配置引用方法
        /// </summary>
        /// <typeparam name="TResult">目标类型</typeparam>
        /// <param name="expression">表达式</param>
        /// <returns>引用配置项</returns>
        public SerializationReferenceConfiguration<T> Reference<TResult>(
            Expression<Func<T, TResult>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                //获取表达式代表的属性名称
                var name = memberExpression.Member.Name;

                return Reference(name);
            }

            throw new ArgumentException("不能使用非属性访问表达式配置引用.");
        }

        /// <summary>
        ///     忽略属性
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <returns>自身</returns>
        public SerializationEntityConfiguration<T> Ignore(string name)
        {
            var property = typeof(T).GetProperty(name);
            if (property == null)
                throw new ArgumentException($"未找到名称为{name}的属性.");

            _ignoredProperties.Add(name);
            _serializeTypeElementConfigurations.Remove(name);

            return this;
        }

        /// <summary>
        ///     忽略属性
        /// </summary>
        /// <param name="expression">属性表达式</param>
        /// <returns>自身</returns>
        public SerializationEntityConfiguration<T> Ignore<TResult>(
            Expression<Func<T, TResult>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                //获取表达式代表的属性名称
                var name = memberExpression.Member.Name;
                return Ignore(name);
            }

            throw new ArgumentException("不能使用非属性访问表达式配置忽略属性.");
        }

        /// <summary>
        ///     根据类型配置项中的元数据构建模型类型
        ///     本方法由派生类实现
        /// </summary>
        /// <returns>序列化实体类型</returns>
        protected override SerializationEntity CreateReally()
        {
            //在这里创建和配置好所有的元素集合 包含属性和构造参数
            //属性部分 所有属性访问器中 属性类型为Obase基元类型的 设定为属性
            var propertyInfos = typeof(T).GetProperties();
            var simpleProperties = propertyInfos
                .Where(property => PrimitiveType.IsObasePrimitiveType(property.PropertyType)).ToList();

            //反射配置属性
            foreach (var propertyInfo in simpleProperties)
            {
                //如果属性被用户配置为忽略 则跳过
                if (_ignoredProperties.Contains(propertyInfo.Name))
                    continue;
                //创建配置
                var attributeConfig = Attribute(propertyInfo.Name, propertyInfo.PropertyType);
                //配置取值器和设值器
                if (attributeConfig.ValueGetter == null)
                    attributeConfig.HasValueGetter(MakeValueGetter(propertyInfo));
                if (attributeConfig.ValueSetter == null)
                    attributeConfig.HasValueSetter(MakeValueSetter(propertyInfo));
            }

            //取出构造函数
            var constructors =
                typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (constructors.Length > 0)
            {
                //如果没有用户配置的构造器 则默认使用无参构造器
                var constructor = constructors.FirstOrDefault(p => p.GetParameters().Length == 0);
                if (constructor != null && _constructor == null) HasConstructor(constructor);
            }

            var complexProperties = propertyInfos
                .Where(property => !PrimitiveType.IsObasePrimitiveType(property.PropertyType)).ToList();
            //处理引用
            foreach (var complexProperty in complexProperties)
            {
                //如果属性被用户配置为忽略 则跳过
                if (_ignoredProperties.Contains(complexProperty.Name))
                    continue;
                //取出真实类型
                Utils.GetIsMultiple(complexProperty, out var realType);
                //如果此类型已经被注册过了 则表示这个属性是引用类型 需要配置一个引用元素
                if (_modelBuilder.ExistSerializationEntityConfiguration(realType))
                {
                    //创建配置
                    var referenceConfiguration = Reference(complexProperty.Name);
                    //配置取值器和设值器
                    if (referenceConfiguration.ValueGetter == null)
                        referenceConfiguration.HasValueGetter(MakeValueGetter(complexProperty));
                    if (referenceConfiguration.ValueSetter == null)
                        referenceConfiguration.HasValueSetter(MakeValueSetter(complexProperty));
                }
            }


            //构造一个序列化实体类型
            var serializationEntity = new SerializationEntity(typeof(T))
            {
                Constructor = _constructor?.Create()
            };

            //加入配置的元素
            foreach (var typeElement in _serializeTypeElementConfigurations.Values)
            {
                var element = typeElement.Create();
                serializationEntity.Elements.Add(element);
            }

            //返回
            return serializationEntity;
        }

        /// <summary>
        ///     制作取值器
        /// </summary>
        /// <param name="property">属性</param>
        /// <returns>取值器</returns>
        private IValueGetter MakeValueGetter(PropertyInfo property)
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
                var getter = typeof(DelegateValueGetter<,>).MakeGenericType(typeof(T), property.PropertyType);
                var getterObj = Activator.CreateInstance(getter, exp.Compile()) as IValueGetter;
                return getterObj;
            }

            //判断多重性
            var isMultiple = Utils.GetIsMultiple(property, out _);
            var method = property.GetMethod;
            if (method != null)
            {
                //不是结构 普通的方法取值
                if (isMultiple)
                {
                    //包装要取的值
                    var ienumableType =
                        typeof(IEnumerable<>).MakeGenericType(method.ReturnType.GetGenericArguments()[0]);
                    var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), ienumableType);
                    //创建委托
                    var delegateFunc = method.CreateDelegate(delegateType);
                    if (delegateFunc != null)
                    {
                        //创建取值器
                        var valueGetter = typeof(DelegateValueGetter<,>).MakeGenericType(typeof(T), ienumableType);
                        var valueGetterInstance = Activator.CreateInstance(valueGetter, delegateFunc) as IValueGetter;
                        return valueGetterInstance;
                    }
                }
                else
                {
                    var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), method.ReturnType);
                    //创建委托
                    var delegateFunc = method.CreateDelegate(delegateType);
                    if (delegateFunc != null)
                    {
                        //创建取值器
                        var valueGetter =
                            typeof(DelegateValueGetter<,>).MakeGenericType(typeof(T), method.ReturnType);
                        var valueGetterInstance = Activator.CreateInstance(valueGetter, delegateFunc) as IValueGetter;
                        return valueGetterInstance;
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     制作设值器
        /// </summary>
        /// <param name="property">属性</param>
        /// <returns>设值器</returns>
        private IValueSetter MakeValueSetter(PropertyInfo property)
        {
            var method = property.GetSetMethod(true);
            if (method != null)
                //只支持单参数的设值方法
                if (method.GetParameters().Length == 1)
                {
                    Delegate setDelegate;
                    Type[] types = { method.DeclaringType, method.GetParameters()[0].ParameterType };
                    //值类型（即非引用类型）
                    if (method.DeclaringType != null && method.DeclaringType.IsValueType)
                    {
                        //调用IL
                        //定义方法 TStruct 引用传递 TValue 值传递 设定Owner为结构类型 跳过JIT检查
                        var dynamicMethod = new DynamicMethod(method.Name, null,
                            new[] { types[0].MakeByRefType(), types[1] },
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

                    return ValueSetter.Create(setDelegate, EValueSettingMode.Assignment);
                }

            return null;
        }
    }
}