/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：具体判别标记的取值器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:02:22
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     具体判别标记的取值器
    /// </summary>
    public class ConcreteTypeSignValueGetter : IValueGetter
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
        ///     具体判别标记的取值器
        /// </summary>
        /// <param name="values1">判别标识集合1 内存代理类型</param>
        /// <param name="values2">判别标识集合1 内存Clr类型</param>
        public ConcreteTypeSignValueGetter(Dictionary<Type, object> values1, Dictionary<Type, object> values2)
        {
            _rebuildingTypeValues = values1;
            _clrTypeValues = values2;
        }

        /// <summary>
        ///     从指定对象取值。
        /// </summary>
        /// <param name="obj">目标对象</param>
        public object GetValue(object obj)
        {
            //在两个字典中查找具体类型的值
            if (_rebuildingTypeValues.ContainsKey(obj.GetType()))
                return _rebuildingTypeValues[obj.GetType()];
            if (_clrTypeValues.ContainsKey(obj.GetType()))
                return _clrTypeValues[obj.GetType()];
            return null;
        }
    }
}