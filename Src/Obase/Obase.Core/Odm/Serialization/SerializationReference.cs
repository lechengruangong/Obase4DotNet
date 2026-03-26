/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化实体的引用.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-25 16:23:32
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Odm.Serialization
{
    /// <summary>
    ///     序列化实体的引用
    /// </summary>
    public class SerializationReference : SerializationTypeElement
    {
        /// <summary>
        ///     引用的名称
        /// </summary>
        private readonly string _name;

        /// <summary>
        ///     初始化序列化实体的类型元素
        /// </summary>
        /// <param name="name">引用的名称</param>
        /// <param name="valueType">类型元素的值类型</param>
        public SerializationReference(string name, Type valueType) : base(valueType)
        {
            _name = name;
        }

        /// <summary>
        ///     获取是否需要存储
        ///     如果是需要存储 则在序列化时调用ValueGetter获取值并存储到序列化结果中 此时会在IValueGetter中传入当前需要序列化的对象以供获取值时使用
        ///     如果不需要存储 则在反序列化时调用ValueGetter获取值并赋值到对象中 此时IValueGetter中传入的对象为null
        /// </summary>
        public override bool NeedStorage => false;

        /// <summary>
        ///     引用的名称
        /// </summary>
        public string Name => _name;
    }
}