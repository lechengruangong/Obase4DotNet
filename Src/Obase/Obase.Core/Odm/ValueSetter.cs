/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：设值器的基础实现.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:15:54
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     为设值器提供基础实现。
    /// </summary>
    public abstract class ValueSetter : IValueSetter
    {
        /// <summary>
        ///     获取设值模式。
        /// </summary>
        public abstract EValueSettingMode Mode { get; }

        /// <summary>
        ///     首先执行验证逻辑：
        ///     如果obj不为引用类型，引发异常“为结构体设值时请使用StructWrapper对其进行包装”
        ///     如果验证成功，调用SetValueCore。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="value">值对象</param>
        public void SetValue(object obj, object value)
        {
            //值类型 需要特殊的处理
            if (obj.GetType().IsValueType)
                throw new InvalidOperationException("为结构体设值时请使用StructWrapper对其进行包装");
            SetValueCore(obj, value);
        }

        /// <summary>
        ///     执行为对象设值的核心逻辑。由派生类实现。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="value">值对象</param>
        protected abstract void SetValueCore(object obj, object value);


        /// <summary>
        ///     为指定字段所代表的元素创建设值器实例。
        /// </summary>
        /// <returns>设值器实例。</returns>
        /// <param name="fieldInfo">代表元素的字段。</param>
        /// 实施说明:
        /// 如果定义字段的类型为结构体，创建StructFieldValueSetter实例，否则创建FieldValueSetter实例。
        public static ValueSetter Create(FieldInfo fieldInfo)
        {
            var declaringType = fieldInfo.DeclaringType;
            //定义字段的类型为结构体 使用结构体设值器 否则使用普通的字段设值器
            if (declaringType != null && declaringType.IsValueType)
                return new StructFieldValueSetter(fieldInfo);
            return new FieldValueSetter(fieldInfo);
        }

        /// <summary>
        ///     创建一个设值器实例，该设值器通过调用指定的委托为元素设值。
        /// </summary>
        /// <returns>设值器实例。</returns>
        /// <param name="delegate">为元素设值的委托。</param>
        /// <param name="mode">设值模式。</param>
        /// 实施说明: 
        /// 获取委托代表的方法，检测其第一个参数的类型，根据该类型为引用类型或结构体调度到相应的私有方法。
        public static ValueSetter Create(Delegate @delegate, EValueSettingMode mode)
        {
            var typeArguments = GetSetterDelegateTypeArguments(@delegate);
            //基元类型或结构体 使用结构体设值器 否则使用普通的字段设值器
            if (IsPrimitive(typeArguments[0]) || typeArguments[0].IsValueType)
                return StructCreate(@delegate, mode);
            return ObjectCreate(@delegate, mode);
        }

        /// <summary>
        ///     创建适用于引用类型的元素设值器实例，该设值器通过调用指定的委托为元素设值。
        /// </summary>
        /// <returns>设值器实例。</returns>
        /// <param name="delegate">为元素设值的委托。</param>
        /// <param name="mode">设值模式。</param>
        /// 实施说明:
        /// 参见活动图“创建委托设值器”。
        private static ValueSetter ObjectCreate(Delegate @delegate, EValueSettingMode mode)
        {
            var typeArguments = GetSetterDelegateTypeArguments(@delegate);
            //默认条件 是否是基元类型或集合类型
            var defaultCondition = IsPrimitive(typeArguments[1]) || typeArguments[1] == typeof(IEnumerable) ||
                                   typeArguments[1] == typeof(IEnumerable<object>);
            defaultCondition = defaultCondition || (typeArguments[1].GetInterface("IEnumerable") == null &&
                                                    typeArguments[1].GetInterface("ICollection") == null);
            //委托设置器类型
            Type setterType;
            //非“默认集合类型委托设值器”覆盖类型
            if (defaultCondition)
            {
                setterType = typeof(DelegateValueSetter<,>).MakeGenericType(typeArguments);
                return (ValueSetter)Activator.CreateInstance(setterType, @delegate, mode);
            }

            //数组类型 集合
            if (typeArguments[1].BaseType == typeof(Array))
            {
                var fullName = typeArguments[1].FullName;
                if (fullName != null)
                    typeArguments[1] = typeArguments[1].Assembly
                        .GetType(fullName.Replace("[]", string.Empty));
                setterType = typeArguments[1].IsValueType
                    ? typeof(DelegateStrcutArrayValueSetter<,>).MakeGenericType(typeArguments)
                    : typeof(DelegateArrayValueSetter<,>).MakeGenericType(typeArguments);
            }
            //其他类型 集合
            else
            {
                //实现接口IEnumerable并且不是string类型
                var paraTypeDef = typeArguments[1].GetGenericTypeDefinition();
                typeArguments[1] = typeArguments[1].GetGenericArguments()[0];
                if (paraTypeDef == typeof(IEnumerable<>))
                    setterType = typeof(DelegateEnumerableValueSetter<,>).MakeGenericType(typeArguments);
                //已有的设值器 根据类型创建
                else if (paraTypeDef == typeof(List<>) || typeof(List<>).GetInterface(paraTypeDef.Name) != null)
                    setterType = typeof(DelegateListValueSetter<,>).MakeGenericType(typeArguments);
                else if (paraTypeDef == typeof(Queue<>))
                    setterType = typeof(DelegateQueueValueSetter<,>).MakeGenericType(typeArguments);
                else if (paraTypeDef == typeof(Stack<>))
                    setterType = typeof(DelegateStackValueSetter<,>).MakeGenericType(typeArguments);
                else if (paraTypeDef == typeof(HashSet<>))
                    setterType = typeof(DelegateHashValueSetter<,>).MakeGenericType(typeArguments);
                else
                    throw new ArgumentException("为集合类型的元素设值时未匹配到合适的集合构造器，请在配置设值器时显式指定构造器。");
            }

            return (ValueSetter)Activator.CreateInstance(setterType, @delegate);
        }

        /// <summary>
        ///     创建适用于引用类型的元素设值器实例，该设值器通过调用指定的委托为元素设值。
        /// </summary>
        /// <param name="delegate">为元素设值的委托。</param>
        /// <param name="mode">设值模式。</param>
        /// 实施说明:
        /// 实例化StuctDelegateValueSetter
        /// {TStruct, TValue}
        /// 。
        /// 获取委托代表的方法，然后获取其第一个参数的类型作为TStruct的实参，获取第二个参数的类型作为TValue的实参。
        /// <returns>设值器实例。</returns>
        private static ValueSetter StructCreate(Delegate @delegate, EValueSettingMode mode)
        {
            var typeArguments = GetSetterDelegateTypeArguments(@delegate);
            //创建结构体委托设值器
            var setterType = typeof(StructDelegateValueSetter<,>).MakeGenericType(typeArguments);
            return (ValueSetter)Activator.CreateInstance(setterType, @delegate);
        }

        /// <summary>
        ///     判断类型是否为“基元类型(obase定义的基元类型)”。
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        private static bool IsPrimitive(Type type)
        {
            //如果是泛型的类型 则从中拆出来
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return PrimitiveType.IsObasePrimitiveType(type.GetGenericArguments()[0]);
            return PrimitiveType.IsObasePrimitiveType(type);
        }

        /// <summary>
        ///     获取委托的参数类型
        /// </summary>
        /// <param name="deleg">委托</param>
        /// <returns></returns>
        private static Type[] GetSetterDelegateTypeArguments(Delegate deleg)
        {
            var argumentTypes = new Type[2];
            var delType = deleg.GetType();
            //Action<,>有两个参数
            if (delType.IsGenericType && delType.GetGenericTypeDefinition() == typeof(Action<,>))
            {
                var args = delType.GetGenericArguments();
                args.CopyTo(argumentTypes, 0);
            }
            else
            {
                //没有Targe的委托
                if (deleg.Target == null)
                {
                    //从MethodInfo中获取参数类型
                    argumentTypes[0] = deleg.Method.DeclaringType;
                    argumentTypes[1] = deleg.Method.GetParameters()[0].ParameterType;
                }
                else
                {
                    //否则 直接从Method的参数里获取
                    var paramInfos = deleg.Method.GetParameters();
                    if (paramInfos.Length < 2) throw new ArgumentException("创建设值器的委托不合法,应为个参数的委托");
                    argumentTypes[0] = paramInfos[0].ParameterType;
                    argumentTypes[1] = paramInfos[1].ParameterType;
                }
            }

            return argumentTypes;
        }
    }
}