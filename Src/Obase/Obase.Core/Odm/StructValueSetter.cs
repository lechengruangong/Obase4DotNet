/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：结构体设值器基础实现.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 10:17:57
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     为结构体设值器提供基础实现。
    /// </summary>
    public abstract class StructValueSetter : ValueSetter
    {
        /// <summary>
        ///     执行为对象设值的核心逻辑。由派生类实现。
        ///     首先判定obj是否为StructWrapper类型，如果不是引发异常“为结构体设值时请使用StructWrapper对其进行包装”；如果是，调用SetStruc
        ///     tValue(ref obj.Struct, value)。
        /// </summary>
        /// <param name="obj">目标对象</param>
        /// <param name="value">值对象</param>
        protected override void SetValueCore(object obj, object value)
        {
            //值类型 需要特殊的处理
            if (!(obj is StructWrapper objStructWrapper))
                throw new InvalidOperationException("为结构体设值时请使用StructWrapper对其进行包装");
            //值类型 需要引用传递
            SetStructValue(ref objStructWrapper.Struct, value);
        }

        /// <summary>
        ///     为结构体设值。
        /// </summary>
        /// <param name="structObj">目标结构体。</param>
        /// <param name="value">值对象</param>
        protected abstract void SetStructValue(ref object structObj, object value);
    }
}