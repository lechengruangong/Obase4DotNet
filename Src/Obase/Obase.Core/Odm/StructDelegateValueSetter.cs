/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：适用于结构体的委托设值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:46:34
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Common;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     接收结构体引用并为其设值的委托。
    ///     类型参数：
    ///     TStruct	要为其设值的结构体的类型
    ///     TValue	值的类型
    /// </summary>
    /// <param name="structObj">要为其设值的结构体。</param>
    /// <param name="value">值对象。</param>
    public delegate void SetValue<TStruct, in TValue>(ref TStruct structObj, TValue value);

    /// <summary>
    ///     适用于结构体的委托设值器。
    /// </summary>
    public class StructDelegateValueSetter<TStruct, TValue> : StructValueSetter
        where TStruct : struct
    {
        /// <summary>
        ///     为属性设值的委托。
        /// </summary>
        private readonly SetValue<TStruct, TValue> _delegate;


        /// <summary>
        ///     创建StructDelegateValueSetter实例。
        /// </summary>
        /// <param name="delegateFunction">为属性设值的委托。</param>
        public StructDelegateValueSetter(SetValue<TStruct, TValue> delegateFunction)
        {
            _delegate = delegateFunction;
        }

        /// <summary>
        ///     获取设值模式。
        ///     注：本属性总是返回Assignment。
        /// </summary>
        public override EValueSettingMode Mode => EValueSettingMode.Assignment;

        /// <summary>
        ///     使用设值委托为结构体设值。
        /// </summary>
        /// <param name="structObj">目标结构体。</param>
        /// <param name="value">值对象</param>
        protected override void SetStructValue(ref object structObj, object value)
        {
            var tValueType = typeof(TValue);
            var isNullable = tValueType.IsGenericType;
            //目标对象和值对象空判断
            if (structObj == null || value == null || value is DBNull) return;
            //nullable
            if (isNullable)
                tValueType = tValueType.GenericTypeArguments[0];
            //统一转换值类型
            value = Utils.ConvertDbValue(value, tValueType);

            var struce = (TStruct)structObj;
            _delegate(ref struce, (TValue)value);
            structObj = struce;
        }
    }
}