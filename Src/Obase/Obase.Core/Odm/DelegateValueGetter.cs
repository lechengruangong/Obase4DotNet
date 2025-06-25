/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：委托取值器,使用指定的委托获取属性值.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:50:07
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     委托取值器，使用指定的委托获取属性值。
    /// </summary>
    public class DelegateValueGetter<TObject, TValue> : IValueGetter
    {
        /// <summary>
        ///     用于获取属性值的委托。
        /// </summary>
        private readonly Func<TObject, TValue> _delegate;

        /// <summary>
        ///     创建DelegateValueGetter实例
        /// </summary>
        /// <param name="delegateFunction">用于获取属性值的委托。</param>
        public DelegateValueGetter(Func<TObject, TValue> delegateFunction)
        {
            _delegate = delegateFunction;
        }

        /// <summary>
        ///     从指定对象取值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        public object GetValue(object obj)
        {
            return _delegate((TObject)obj);
        }
    }
}