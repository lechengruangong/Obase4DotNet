using System;
using System.IO;
using Obase.Core.Common;

namespace Obase.Test.Infrastructure.ModelRegister;

/// <summary>
///     逗号分隔的序列化器
///     实现ITextSerializer接口 可以较为精细的控制流的读写
///     在需要对流进行特殊处理的场景下可以使用 本类仅展示了实现方法 未对流进行处理
/// </summary>
public class CommaSplitSerializer : ITextSerializer
{
    /// <summary>
    ///     对给定的数据实施反序列化，以重建对象（图）。
    /// </summary>
    /// <param name="serializationStream">提供序列化数据的流，它可以指代多种后备存储区，如内存、文件、网络等。</param>
    /// <param name="objType">反序列化的对象的类型。</param>
    public object Deserialize(Stream serializationStream, Type objType)
    {
        //此处使用的是Utils内提供的流转字符串方法 此方法会将流全部读入内存 适用于数据量较小的场景
        //如果需要自定义处理 可以直接操作流
        return Utils.GetUtf8StringFromStream(serializationStream).Split(",");
    }

    /// <summary>
    ///     对指定的对象或以该对象为根的对象图实施序列化。
    /// </summary>
    /// <param name="obj">要序列化的对象。</param>
    /// <param name="serializationStream">存储序列化数据的流，它可以指代多种后备存储区，如内存、文件、网络等。</param>
    public void Serialize(object obj, Stream serializationStream)
    {
        //此处使用的是Utils内提供的字符串写入流的方法 此方法会将流全部读入内存 适用于数据量较小的场景
        //如果需要自定义处理 可以直接操作流
        //此处obj肯定是string[]
        Utils.WriteUtf8StringToStream(string.Join(",", (string[])obj), serializationStream);
    }

    /// <summary>
    ///     对给定的文本（以UTF-8编码）实施反序列化，以重建对象（图）。
    /// </summary>
    /// <param name="serializationText">序列化文本。</param>
    /// <param name="objType">要反序列化的对象的类型。</param>
    public object Deserialize(string serializationText, Type objType)
    {
        //直接分隔即可 不需要考虑objType
        return serializationText.Split(",");
    }

    /// <summary>
    ///     对指定的对象或以该对象为根的对象图实施文本序列化（以UTF-8编码）。
    /// </summary>
    /// <param name="obj">要序列化的对象。</param>
    public string Serialize(object obj)
    {
        //此处obj肯定是string[]
        return string.Join(",", (string[])obj);
    }
}