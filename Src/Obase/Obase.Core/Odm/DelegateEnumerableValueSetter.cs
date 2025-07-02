/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：可枚举类型的委托设值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:33:12
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     特定于可枚举类型的委托设值器，使用指定的委托为可枚举类型的元素设置值。
    /// </summary>
    /// <typeparam name="TObject">要设值的元素的属主类型。</typeparam>
    /// <typeparam name="TElement">值序列项的类型。</typeparam>
    /// 实施说明
    /// 参见顺序图“为可枚举类型元素设值（一）”。
    internal class
        DelegateEnumerableValueSetter<TObject, TElement> : DelegateValueSetter<TObject, IEnumerable<TElement>>
        where TObject : class
        where TElement : class
    {
        /// <summary>
        ///     一个委托，代表为可枚举类型的元素设置值的方法。
        /// </summary>
        private readonly Action<TObject, IEnumerable<TElement>> _delegate;

        /// <summary>
        ///     创建DelegateEnumerableValueSetter实例。
        /// </summary>
        /// <param name="delegate">为元素设值的委托。</param>
        public DelegateEnumerableValueSetter(Action<TObject, IEnumerable<TElement>> @delegate) : base(@delegate,
            EValueSettingMode.Assignment)
        {
            _delegate = @delegate;
        }

        /// <summary>
        ///     执行为对象设值的核心逻辑。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="value">值对象</param>
        protected override void SetValueCore(object obj, object value)
        {
            if (value == null) return;
            //将值转换为指定类型的可枚举序列
            var newValue = ((IEnumerable<object>)value).Cast<TElement>();
            //调用委托为对象设值
            _delegate((TObject)obj, newValue);
        }
    }


    /// <summary>
    ///     特定于可枚举类型的委托设值器，使用指定的委托创建可枚举类型的值，然后使用指定的委托将该值赋给元素。
    /// </summary>
    /// <typeparam name="TObject">要设值的元素的属主类型。</typeparam>
    /// <typeparam name="TValue">值的类型。</typeparam>
    /// <typeparam name="TElement">值序列项的类型。</typeparam>
    /// 实施说明
    /// 参见顺序图“为可枚举类型元素设值（二）”。
    internal class DelegateEnumerableValueSetter<TObject, TValue, TElement> : DelegateValueSetter<TObject, TValue>
        where TObject : class
        where TValue : IEnumerable<TElement>
        where TElement : class
    {
        /// <summary>
        ///     一个委托，代表基于IEnumerable序列创建可枚举类型值的方法。
        /// </summary>
        private readonly Func<IEnumerable<TElement>, TValue> _valueCreator;

        /// <summary>
        ///     创建DelegateEnumerableValueSetter实例。
        /// </summary>
        /// <param name="delegate">为元素设值的委托。</param>
        /// <param name="valueCreator">一个委托，代表基于IEnumerable序列创建可枚举类型值的方法。</param>
        public DelegateEnumerableValueSetter(Action<TObject, TValue> @delegate,
            Func<IEnumerable<TElement>, TValue> valueCreator) : base(@delegate, EValueSettingMode.Assignment)
        {
            _valueCreator = valueCreator;
        }

        /// <summary>
        ///     执行为对象设值的核心逻辑。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="value">值对象</param>
        protected override void SetValueCore(object obj, object value)
        {
            if (value == null) return;
            //将值转换为指定类型的可枚举序列
            var values = ((IEnumerable<object>)value).Cast<TElement>();
            //调用基类设置值
            base.SetValueCore(obj, _valueCreator(values));
        }
    }

    /// <summary>
    ///     特定于可枚举结构类型的委托设值器，使用指定的委托创建可枚举类型的值，然后使用指定的委托将该值赋给元素。
    /// </summary>
    /// <typeparam name="TObject">要设值的元素的属主类型。</typeparam>
    /// <typeparam name="TValue">值的类型。</typeparam>
    /// <typeparam name="TElement">值序列项的类型。</typeparam>
    internal class DelegateEnumerableStructValueSetter<TObject, TValue, TElement> : DelegateValueSetter<TObject, TValue>
        where TObject : class
        where TValue : IEnumerable<TElement>
        where TElement : struct
    {
        /// <summary>
        ///     一个委托，代表基于IEnumerable序列创建可枚举类型值的方法。
        /// </summary>
        private readonly Func<IEnumerable<TElement>, TValue> _valueCreator;

        /// <summary>
        ///     创建DelegateEnumerableValueSetter实例。
        /// </summary>
        /// <param name="delegate">为元素设值的委托。</param>
        /// <param name="valueCreator">一个委托，代表基于IEnumerable序列创建可枚举类型值的方法。</param>
        public DelegateEnumerableStructValueSetter(Action<TObject, TValue> @delegate,
            Func<IEnumerable<TElement>, TValue> valueCreator) : base(@delegate, EValueSettingMode.Assignment)
        {
            _valueCreator = valueCreator;
        }

        /// <summary>
        ///     执行为对象设值的核心逻辑。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="value">值对象</param>
        protected override void SetValueCore(object obj, object value)
        {
            if (value == null) return;
            //将值转换为指定类型的可枚举序列
            var values = ((IEnumerable<object>)value).Cast<TElement>();
            //调用基类设置值
            base.SetValueCore(obj, _valueCreator(values));
        }
    }
}