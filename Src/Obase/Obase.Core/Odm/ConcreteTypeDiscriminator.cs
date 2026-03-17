/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-17 19:33:26
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     默认的具体类型区分器，根据类型代码选择一个具体类型。
    ///     如果没有配置具体类型区分器，则使用本类作为默认的区分器。
    /// </summary>
    public class ConcreteTypeDiscriminator : IConcreteTypeDiscriminator
    {
        /// <summary>
        ///     具体类型区分字典
        /// </summary>
        private readonly Dictionary<string, StructuralType> _dictionary;

        /// <summary>
        ///     初始化默认的具体类型区分器
        /// </summary>
        /// <param name="dictionary">具体类型区分字典</param>
        public ConcreteTypeDiscriminator(Dictionary<string, StructuralType> dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>
        ///     根据类型代码选择一个具体类型。
        /// </summary>
        /// <param name="typeCode">类型代码</param>
        public StructuralType Discriminate(object typeCode)
        {
            return _dictionary.TryGetValue(typeCode?.ToString() ?? string.Empty, out var structuralType)
                ? structuralType
                : null;
        }
    }
}