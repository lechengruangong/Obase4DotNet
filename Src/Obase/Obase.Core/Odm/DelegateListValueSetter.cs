/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：List的委托设值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:39:01
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     特定于List{}类型的委托设值器，基于一个IEnumerable序列构造List{}实例，然后使用指定的委托将该实例赋值给元素。
    /// </summary>
    /// <typeparam name="TObject">要设值的元素的属主类型。</typeparam>
    /// <typeparam name="TElement">值序列项的类型。</typeparam>
    internal class
        DelegateListValueSetter<TObject, TElement> : DelegateEnumerableValueSetter<TObject, List<TElement>, TElement>
        where TObject : class
        where TElement : class
    {
        /// <summary>
        ///     创建DelegateListValueSetter实例。
        /// </summary>
        /// <param name="delegate">为元素设值的委托。</param>
        public DelegateListValueSetter(Action<TObject, List<TElement>> @delegate) : base(@delegate,
            value => new List<TElement>(value))
        {
        }
    }

    /// <summary>
    ///     特定于List{}类型的委托设值器，基于一个IEnumerable序列构造List{}实例，然后使用指定的委托将该实例赋值给元素。
    /// </summary>
    /// <typeparam name="TObject">要设值的元素的属主类型。</typeparam>
    /// <typeparam name="TElement">值序列项的类型。</typeparam>
    /// 实施说明
    /// 不限定TElement为struct,否则可空值类型(long?等)无法使用此设值器。
    /// 本类仅在值序列项为值类型时被选用(参见ValueSetter.ObjectCreate)。
    internal class
        DelegateStructListValueSetter<TObject, TElement> : DelegateEnumerableStructValueSetter<TObject, List<TElement>,
        TElement>
        where TObject : class
    {
        /// <summary>
        ///     创建DelegateStructListValueSetter实例。
        /// </summary>
        /// <param name="delegate">为元素设值的委托。</param>
        public DelegateStructListValueSetter(Action<TObject, List<TElement>> @delegate) : base(@delegate,
            value => new List<TElement>(value))
        {
        }
    }
}