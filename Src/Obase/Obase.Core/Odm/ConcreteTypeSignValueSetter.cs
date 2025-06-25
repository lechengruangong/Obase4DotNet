/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：类型判别属性的设置器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:04:00
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     类型判别属性的设置器
    /// </summary>
    public class ConcreteTypeSignValueSetter : IValueSetter
    {
        /// <summary>
        ///     判别标识集合1 内存Clr类型
        /// </summary>
        private readonly Dictionary<Type, object> _clrTypeValues;

        /// <summary>
        ///     判别标识集合1 内存代理类型
        /// </summary>
        private readonly Dictionary<Type, object> _rebuildingTypeValues;

        /// <summary>
        ///     实际的设值器
        /// </summary>
        private readonly IValueSetter _setter;

        /// <summary>
        ///     类型判别属性的设置器
        /// </summary>
        /// <param name="values1">判别标识集合1 内存代理类型</param>
        /// <param name="values2">判别标识集合1 内存Clr类型</param>
        /// <param name="setter">实际的设值器</param>
        public ConcreteTypeSignValueSetter(Dictionary<Type, object> values1, Dictionary<Type, object> values2,
            IValueSetter setter)
        {
            _rebuildingTypeValues = values1;
            _clrTypeValues = values2;
            _setter = setter;
        }

        /// <summary>
        ///     获取设值模式。
        /// </summary>
        public EValueSettingMode Mode => EValueSettingMode.Assignment;

        /// <summary>
        ///     为对象设值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="value">值对象</param>
        public void SetValue(object obj, object value)
        {
            object reavalue = null;
            //在两个字典中查找值
            if (_rebuildingTypeValues.ContainsKey(obj.GetType()))
                reavalue = _rebuildingTypeValues[obj.GetType()];
            if (_clrTypeValues.ContainsKey(obj.GetType()))
                reavalue = _clrTypeValues[obj.GetType()];
            //找到值 就设置值
            if (_setter != null)
                _setter.SetValue(obj, reavalue);
        }
    }
}