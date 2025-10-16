using System;
using Newtonsoft.Json;
using Obase.Core.Common;

namespace Obase.Test.Infrastructure.ModelRegister;

/// <summary>
///     普通的JSON序列化器
///     通过TextSerializer简化实现 只需要处理字符串和对象的转换
///     大多数场景下推荐使用此类
/// </summary>
public class JsonSerializer : TextSerializer
{
    /// <summary>
    ///     对给定的文本（以UTF-8编码）实施反序列化，以重建对象（图）。
    /// </summary>
    /// <param name="serializationText">序列化文本。</param>
    /// <param name="objType">要反序列化的对象的类型。</param>
    protected override object DoDeserialize(string serializationText, Type objType)
    {
        return JsonConvert.DeserializeObject(serializationText, objType);
    }

    /// <summary>
    ///     对指定的对象或以该对象为根的对象图实施文本序列化（以UTF-8编码）。
    /// </summary>
    /// <param name="obj">要序列化的对象。</param>
    protected override string DoSerialize(object obj)
    {
        return JsonConvert.SerializeObject(obj);
    }
}