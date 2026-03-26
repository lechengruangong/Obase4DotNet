using System;

namespace Obase.Test.Domain.Functional.Serialization;

/// <summary>
///     某种组件
/// </summary>
public interface IComponent
{
    /// <summary>
    ///     组件代码
    /// </summary>
    Guid Code { get; }

    /// <summary>
    ///     组件名称
    /// </summary>
    string Name { get; }
}