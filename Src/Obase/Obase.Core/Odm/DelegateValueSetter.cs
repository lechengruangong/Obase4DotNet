/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：委托设值器,使用指定的委托为元素设置值.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:14:31
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Common;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     委托设值器，使用指定的委托为元素设置值。
    /// </summary>
    /// <typeparam name="TObject">要设值的元素的属主类型。</typeparam>
    /// <typeparam name="TValue">在Assignment模式下为值序列的类型，在Appending模式下为值序列项的类型。</typeparam>
    /// 实施说明:
    /// 将调用方传入的值强制转换为TValue，然后调用委托。
    internal class DelegateValueSetter<TObject, TValue> : ValueSetter
    {
        /// <summary>
        ///     为属性设值的委托。
        /// </summary>
        private readonly Action<TObject, TValue> _delegate;

        /// <summary>
        ///     创建DelegateValueSetter实例。
        /// </summary>
        /// <param name="delegate">为属性设值的委托。</param>
        /// <param name="mode">设值模式。</param>
        public DelegateValueSetter(Action<TObject, TValue> @delegate, EValueSettingMode mode)
        {
            _delegate = @delegate;
            Mode = mode;
        }

        /// <summary>
        ///     获取设值模式。
        /// </summary>
        public override EValueSettingMode Mode { get; }

        /// <summary>
        ///     执行为对象设值的核心逻辑。由派生类实现。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="value">值对象</param>
        protected override void SetValueCore(object obj, object value)
        {
            var tValueType = typeof(TValue);
            var isNullable = tValueType.IsGenericType;
            //目标对象和值对象空判断
            if (obj == null || value == null || value is DBNull) return;
            //nullable
            if (isNullable)
                tValueType = tValueType.GenericTypeArguments[0];
            value = Utils.ConvertDbValue(value, tValueType);
            _delegate((TObject)obj, (TValue)value);
        }
    }
}