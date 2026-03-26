/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化实体的属性配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-26 10:51:35
└──────────────────────────────────────────────────────────────┘
*/


using System;
using Obase.Core.Odm.Serialization;

namespace Obase.Core.Odm.Builder.Serialization
{
    /// <summary>
    ///     序列化实体的属性配置
    /// </summary>
    /// <typeparam name="TStructural">序列化实体类型</typeparam>
    public class
        SerializationAttributeConfiguration<TStructural> : SerializationTypeElementConfiguration<TStructural>
    {
        /// <summary>
        ///     属性名称
        /// </summary>
        private readonly string _name;

        /// <summary>
        ///     初始化序列化实体的属性配置
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <param name="valueType">值类型</param>
        public SerializationAttributeConfiguration(string name, Type valueType) : base(valueType)
        {
            _name = name;
        }

        /// <summary>
        ///     创建对应的序列化元素
        /// </summary>
        /// <returns>序列化元素</returns>
        public override SerializationElement Create()
        {
            //创建序列化属性
            var result = new SerializationAttribute(_name, ValueType)
            {
                ValueGetter = _valueGetter,
                ValueSetter = _valueSetter
            };

            return result;
        }
    }
}