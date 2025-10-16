/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：为文本序列化程序提供基础实现.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-10-14 17:58:51
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.IO;

namespace Obase.Core.Common
{
    /// <summary>
    ///     为文本序列化程序提供基础实现
    /// </summary>
    public abstract class TextSerializer : ITextSerializer
    {
        /// <summary>
        ///     对给定的数据实施反序列化，以重建对象（图）。
        /// </summary>
        /// <param name="serializationStream">提供序列化数据的流，它可以指代多种后备存储区，如内存、文件、网络等。</param>
        /// <param name="objType">反序列化的对象的类型。</param>
        public object Deserialize(Stream serializationStream, Type objType)
        {
            //读取流 调用DoDeserialize
            return DoDeserialize(Utils.GetUtf8StringFromStream(serializationStream), objType);
        }

        /// <summary>
        ///     对指定的对象或以该对象为根的对象图实施序列化。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        /// <param name="serializationStream">存储序列化数据的流，它可以指代多种后备存储区，如内存、文件、网络等。</param>
        public void Serialize(object obj, Stream serializationStream)
        {
            //调用DoSerialize 写入流
            Utils.WriteUtf8StringToStream(DoSerialize(obj), serializationStream);
        }

        /// <summary>
        ///     对给定的文本（以UTF-8编码）实施反序列化，以重建对象（图）。
        /// </summary>
        /// <param name="serializationText">序列化文本。</param>
        /// <param name="objType">要反序列化的对象的类型。</param>
        public object Deserialize(string serializationText, Type objType)
        {
            //调用DoDeserialize
            return DoDeserialize(serializationText, objType);
        }

        /// <summary>
        ///     对指定的对象或以该对象为根的对象图实施文本序列化（以UTF-8编码）。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        public string Serialize(object obj)
        {
            //调用DoSerialize
            return DoSerialize(obj);
        }

        /// <summary>
        ///     对给定的文本（以UTF-8编码）实施反序列化，以重建对象（图）。
        /// </summary>
        /// <param name="serializationText">序列化文本。</param>
        /// <param name="objType">要反序列化的对象的类型。</param>
        protected abstract object DoDeserialize(string serializationText, Type objType);

        /// <summary>
        ///     对指定的对象或以该对象为根的对象图实施文本序列化（以UTF-8编码）。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        protected abstract string DoSerialize(object obj);
    }
}