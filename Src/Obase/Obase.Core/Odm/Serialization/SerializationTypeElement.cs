/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化实体的需要设值类型元素.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-25 15:46:15
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;

namespace Obase.Core.Odm.Serialization
{
    /// <summary>
    ///     序列化实体的需要设值类型元素
    /// </summary>
    public abstract class SerializationTypeElement : SerializationElement
    {
        /// <summary>
        ///     属性的设值器
        /// </summary>
        private IValueSetter _valueSetter;

        /// <summary>
        ///     初始化序列化实体的类型元素
        /// </summary>
        /// <param name="valueType">类型元素的值类型</param>
        protected SerializationTypeElement(Type valueType) : base(valueType)
        {
        }

        /// <summary>
        ///     属性的设值器
        /// </summary>
        public IValueSetter ValueSetter
        {
            get => _valueSetter;
            set => _valueSetter = value;
        }

        /// <summary>
        ///     为指定对象的当前元素设置值，适用于具有多重性的元素。
        /// </summary>
        /// <param name="targetObj">要为其元素设值的对象。</param>
        /// <param name="value">元素的值。</param>
        public void SetValue(object targetObj, IEnumerable value)
        {
            var settinMode = _valueSetter.Mode;
            switch (settinMode)
            {
                case EValueSettingMode.Assignment:
                    _valueSetter.SetValue(targetObj, value);
                    break;
                case EValueSettingMode.Appending:
                    if (value == null) return;
                    foreach (var valueItem in value)
                        _valueSetter.SetValue(targetObj, valueItem);
                    break;
            }
        }

        /// <summary>
        ///     为指定对象的当前元素设置值，适用于不具多重性的元素。
        /// </summary>
        /// <param name="targetObj">要为其元素设值的对象。</param>
        /// <param name="value">元素的值。</param>
        public void SetValue(object targetObj, object value)
        {
            //前置过滤，如果value实现了IEnumerable或IEnumerable<>，调用另一重载。
            var valueType = value.GetType();
            if (valueType != typeof(string) && valueType.GetInterface("IEnumerable") != null)
            {
                var iEnumerableValue = (IEnumerable)value;
                SetValue(targetObj, iEnumerableValue);
            }
            else
            {
                _valueSetter.SetValue(targetObj, value);
            }
        }
    }
}