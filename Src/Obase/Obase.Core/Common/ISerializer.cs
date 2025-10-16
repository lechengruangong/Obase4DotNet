/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化程序接口,为序列化程序定义调用规范.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-10-14 17:55:29
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.IO;

namespace Obase.Core.Common
{
    /// <summary>
    ///     序列化程序接口，为序列化程序定义调用规范。
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        ///     对给定的数据实施反序列化，以重建对象（图）。
        /// </summary>
        /// <param name="serializationStream">提供序列化数据的流，它可以指代多种后备存储区，如内存、文件、网络等。</param>
        /// <param name="objType">反序列化的对象的类型。</param>
        object Deserialize(Stream serializationStream, Type objType);

        /// <summary>
        ///     对指定的对象或以该对象为根的对象图实施序列化。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        /// <param name="serializationStream">存储序列化数据的流，它可以指代多种后备存储区，如内存、文件、网络等。</param>
        void Serialize(object obj, Stream serializationStream);
    }
}