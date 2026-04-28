/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化实体的类型元素配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-26 11:01:20
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm.Serialization;

namespace Obase.Core.Odm.Builder.Serialization
{
    /// <summary>
    ///     序列化实体的类型元素配置
    /// </summary>
    public abstract class SerializationElementConfiguration
    {
        /// <summary>
        ///     类型元素的值类型
        /// </summary>
        private readonly Type _valueType;

        /// <summary>
        ///     类型元素的值获取器
        /// </summary>
        protected IValueGetter _valueGetter;

        /// <summary>
        ///     初始化序列化实体的类型元素配置
        /// </summary>
        /// <param name="valueType">类型元素的值类型</param>
        protected SerializationElementConfiguration(Type valueType)
        {
            _valueType = valueType;
        }


        /// <summary>
        ///     类型元素的值类型
        /// </summary>
        public Type ValueType => _valueType;

        /// <summary>
        ///     类型元素的值获取器
        /// </summary>
        public IValueGetter ValueGetter => _valueGetter;

        /// <summary>
        ///     创建对应的序列化元素
        /// </summary>
        /// <returns>序列化元素</returns>
        public abstract SerializationElement Create();
    }
}