/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：文本序列化程序接口,为文本序列化程序定义调用规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-10-14 17:57:43
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Common
{
    /// <summary>
    ///     文本序列化程序接口，为文本序列化程序定义调用规范。
    /// </summary>
    public interface ITextSerializer : ISerializer
    {
        /// <summary>
        ///     对给定的文本（以UTF-8编码）实施反序列化，以重建对象（图）。
        /// </summary>
        /// <param name="serializationText">序列化文本。</param>
        /// <param name="objType">要反序列化的对象的类型。</param>
        object Deserialize(string serializationText, Type objType);

        /// <summary>
        ///     对指定的对象或以该对象为根的对象图实施文本序列化（以UTF-8编码）。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        string Serialize(object obj);
    }
}