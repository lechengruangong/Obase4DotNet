/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：HashSet的委托设值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:45:32
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     特定于HashSet{}类型的委托设值器，基于一个IEnumerable{}序列构造HashSet{}实例，然后使用指定的委托将该实例赋值给元素。
    /// </summary>
    /// <typeparam name="TObject">要设值的元素的属主类型。</typeparam>
    /// <typeparam name="TElement">值序列项的类型。</typeparam>
    internal class
        DelegateHashValueSetter<TObject, TElement> : DelegateEnumerableValueSetter<TObject, HashSet<TElement>, TElement>
        where TObject : class
        where TElement : class
    {
        /// <summary>
        ///     创建DelegateHashValueSetter实例。
        /// </summary>
        /// <param name="delegate">为元素设值的委托。</param>
        public DelegateHashValueSetter(Action<TObject, HashSet<TElement>> @delegate) : base(@delegate,
            value => new HashSet<TElement>(value))
        {
        }
    }
}