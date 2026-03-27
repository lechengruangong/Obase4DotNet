/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化实体的引用配置.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-26 17:57:05
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm.Serialization;

namespace Obase.Core.Odm.Builder.Serialization
{
    /// <summary>
    ///     序列化实体的引用配置
    /// </summary>
    /// <typeparam name="TStructural">序列化实体类型</typeparam>
    public class SerializationReferenceConfiguration<TStructural> : SerializationTypeElementConfiguration<TStructural>
    {
        /// <summary>
        ///     引用是多重的还是单值的
        /// </summary>
        private readonly bool _multiple;

        /// <summary>
        ///     属性名称
        /// </summary>
        private readonly string _name;

        /// <summary>
        ///     初始化序列化实体的引用配置
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <param name="multiple">引用是多重的还是单值的</param>
        public SerializationReferenceConfiguration(string name, bool multiple) : base(null)
        {
            _name = name;
            _multiple = multiple;
        }

        /// <summary>
        ///     创建对应的序列化元素
        /// </summary>
        /// <returns>序列化元素</returns>
        public override SerializationElement Create()
        {
            //创建序列化引用
            var result = new SerializationReference(_name, _multiple, ValueType)
            {
                ValueGetter = _valueGetter,
                ValueSetter = _valueSetter
            };

            return result;
        }
    }
}