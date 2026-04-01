/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化实体类型构造器参数配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-26 16:34:21
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm.Serialization;

namespace Obase.Core.Odm.Builder.Serialization
{
    /// <summary>
    ///     序列化实体类型构造器参数配置
    /// </summary>
    public class SerializationConstructorParameterConfiguration : SerializationElementConfiguration
    {
        /// <summary>
        ///     对应的构造参数索引
        ///     从#0开始
        /// </summary>
        private readonly string _index;

        /// <summary>
        ///     是否需要存储
        /// </summary>
        private readonly bool _needStorage;

        /// <summary>
        ///     初始化序列化实体类型构造器参数配置
        /// </summary>
        /// <param name="index">对应的构造参数索引</param>
        /// <param name="needStorage">是否需要存储</param>
        /// <param name="valueGetter">取值器</param>
        /// <param name="valueType">类型元素的值类型</param>
        public SerializationConstructorParameterConfiguration(string index, bool needStorage, IValueGetter valueGetter,
            Type valueType) : base(valueType)
        {
            _index = index;
            _needStorage = needStorage;
            _valueGetter = valueGetter;
        }

        /// <summary>
        ///     创建对应的序列化元素
        /// </summary>
        /// <returns>序列化元素</returns>
        public override SerializationElement Create()
        {
            //创建序列化实体类型构造器参数
            var result = new SerializationConstructorParameter(_needStorage, _index, ValueType)
            {
                ValueGetter = _valueGetter
            };

            return result;
        }
    }
}