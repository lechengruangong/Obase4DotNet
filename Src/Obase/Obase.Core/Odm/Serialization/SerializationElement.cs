/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化实体的类型元素.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-25 12:46:05
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm.Serialization
{
    /// <summary>
    ///     序列化实体的类型元素
    /// </summary>
    public abstract class SerializationElement
    {
        /// <summary>
        ///     指示元素是否具有多重性，即其值是否为集合类型。
        /// </summary>
        private readonly bool _isMultiple;

        /// <summary>
        ///     类型元素的值类型
        /// </summary>
        private readonly Type _valueType;

        /// <summary>
        ///     类型元素的值获取器
        /// </summary>
        private IValueGetter _valueGetter;

        /// <summary>
        ///     初始化序列化实体的类型元素
        /// </summary>
        /// <param name="valueType">类型元素的值类型</param>
        /// <param name="isMultiple">指示元素是否具有多重性</param>
        protected SerializationElement(Type valueType, bool isMultiple)
        {
            _valueType = valueType;
            _isMultiple = isMultiple;
        }

        /// <summary>
        ///     类型元素的值类型
        /// </summary>
        public Type ValueType => _valueType;

        /// <summary>
        ///     指示元素是否具有多重性，即其值是否为集合类型。
        /// </summary>
        public bool IsMultiple => _isMultiple;

        /// <summary>
        ///     类型元素的值获取器
        /// </summary>
        public IValueGetter ValueGetter
        {
            get => _valueGetter;
            set => _valueGetter = value;
        }

        /// <summary>
        ///     获取是否需要存储
        ///     如果是需要存储 则在序列化时调用ValueGetter获取值并存储到序列化结果中 此时会在IValueGetter中传入当前需要序列化的对象以供获取值时使用
        ///     如果不需要存储 则在反序列化时调用ValueGetter获取值并赋值到对象中 此时IValueGetter中传入的对象为null
        /// </summary>
        public abstract bool NeedStorage { get; }

        /// <summary>
        ///     从指定对象取出当前元素的值
        /// </summary>
        /// <param name="targetObj">要取其元素值的对象</param>
        /// <returns>如果元素具有多重性，返回IEnumerable{T}，否则返回object</returns>
        public object GetValue(object targetObj)
        {
            return ValueGetter.GetValue(targetObj);
        }
    }
}