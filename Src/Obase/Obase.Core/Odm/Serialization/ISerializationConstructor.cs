/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化实体类型构造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-25 15:36:15
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.Core.Odm.Serialization
{
    /// <summary>
    ///     序列化实体类型构造器
    /// </summary>
    public interface ISerializationConstructor
    {
        /// <summary>
        ///     获取构造函数的形式参数。
        /// </summary>
        List<SerializationConstructorParameter> Parameters { get; }

        /// <summary>
        ///     构造对象。
        /// </summary>
        /// <returns>构造出的对象。</returns>
        /// <param name="arguments">构造函数参数。</param>
        object Construct(object[] arguments = null);
    }
}