using System.Collections.Generic;

namespace Obase.Test.Domain.Functional.Serialization;

/// <summary>
///     组件类 用于测试循环引用的序列化和反序列化
/// </summary>
public class Component : IComponent
{
    /// <summary>
    ///     引用的组件集合
    /// </summary>
    public List<IComponent> Components { get; set; }

    /// <summary>
    ///     组件名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     返回字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}";
    }
}