/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化实体的需要设值类型元素配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-26 11:59:22
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Reflection;

namespace Obase.Core.Odm.Builder.Serialization
{
    /// <summary>
    ///     序列化实体的需要设值类型元素配置
    /// </summary>
    public abstract class SerializationTypeElementConfiguration<TStructural> : SerializationElementConfiguration
    {
        /// <summary>
        ///     属性的设值器
        /// </summary>
        protected IValueSetter _valueSetter;

        /// <summary>
        ///     初始化序列化实体的类型元素配置
        /// </summary>
        /// <param name="valueType">类型元素的值类型</param>
        protected SerializationTypeElementConfiguration(Type valueType) : base(valueType)
        {
        }

        /// <summary>
        ///     属性的设值器
        /// </summary>
        public IValueSetter ValueSetter => _valueSetter;

        /// <summary>
        ///     使用表示类型元素的字段为类型元素创建取值器。
        /// </summary>
        /// <param name="field">表示类型元素的字段。</param>
        /// <returns>自身</returns>
        public SerializationTypeElementConfiguration<TStructural> HasValueGetter(FieldInfo field)
        {
            //构造一个字段取值器
            var filedSetter = new FieldValueGetter(field);
            return HasValueGetter(filedSetter);
        }

        /// <summary>
        ///     用委托设置取值器
        /// </summary>
        /// <typeparam name="TProperty">要取的值类型</typeparam>
        /// <param name="getValue">取值委托</param>
        /// <returns>自身</returns>
        public SerializationTypeElementConfiguration<TStructural> HasValueGetter<TProperty>(
            Func<TStructural, TProperty> getValue)
        {
            //创建一个委托取值器
            var valueGetter = new DelegateValueGetter<TStructural, TProperty>(getValue);
            return HasValueGetter(valueGetter);
        }

        /// <summary>
        ///     设置取值器
        /// </summary>
        /// <param name="getter">取值器</param>
        /// <returns>自身</returns>
        public SerializationTypeElementConfiguration<TStructural> HasValueGetter(IValueGetter getter)
        {
            _valueGetter = getter;
            return this;
        }

        /// <summary>
        ///     使用表示类型元素的字段为类型元素创建设值器
        /// </summary>
        /// <param name="field">表示类型元素的字段</param>
        /// <returns></returns>
        public SerializationTypeElementConfiguration<TStructural> HasValueSetter(FieldInfo field)
        {
            return HasValueSetter(Odm.ValueSetter.Create(field));
        }

        /// <summary>
        ///     用委托设置设值器
        /// </summary>
        /// <typeparam name="TValue">要设置的值类型</typeparam>
        /// <param name="setValue">设值委托</param>
        /// <param name="mode">设值模式</param>
        /// <returns>自身</returns>
        public SerializationTypeElementConfiguration<TStructural> HasValueSetter<TValue>(
            Action<TStructural, TValue> setValue, EValueSettingMode mode)
        {
            return HasValueSetter(Odm.ValueSetter.Create(setValue, mode));
        }

        /// <summary>
        ///     设置设值器
        /// </summary>
        /// <param name="valueSetter">设值器</param>
        /// <returns>自身</returns>
        public SerializationTypeElementConfiguration<TStructural> HasValueSetter(IValueSetter valueSetter)
        {
            _valueSetter = valueSetter;
            return this;
        }
    }
}