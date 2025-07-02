using System;

namespace Obase.Test.Domain.DependencyInjection;

/// <summary>
///     服务接口
/// </summary>
public interface IServiceS
{
    /// <summary>
    ///     服务的Code
    /// </summary>
    string Code { get; }

    /// <summary>
    ///     创建时间
    /// </summary>
    DateTime CreateTime { get; }
}

/// <summary>
///     服务接口
/// </summary>
public interface IServiceSo
{
    /// <summary>
    ///     创建时间
    /// </summary>
    DateTime CreateTime { get; }
}

/// <summary>
///     服务接口
/// </summary>
public interface IServiceT
{
    /// <summary>
    ///     服务的Code
    /// </summary>
    string Code { get; }

    /// <summary>
    ///     创建时间
    /// </summary>
    DateTime CreateTime { get; }
}

/// <summary>
///     服务接口
/// </summary>
public interface IServiceTo
{
    /// <summary>
    ///     创建时间
    /// </summary>
    DateTime CreateTime { get; }
}