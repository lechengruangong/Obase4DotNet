using System;
using Obase.Providers.Sql.ConnectionPool;

namespace Obase.Test.Infrastructure.Configuration;

/// <summary>
///     Obase连接池配置
/// </summary>
public class ObaseConnectionPoolConfiguration : IObaseConnectionPoolConfiguration
{
    /// <summary>
    ///     初始化Obase连接池配置
    /// </summary>
    /// <param name="name">连接池名称</param>
    public ObaseConnectionPoolConfiguration(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     连接池名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     连接池大小
    /// </summary>
    public int MaximumPoolSize => 50;

    /// <summary>
    ///     最小空闲连接数（预热连接数）
    /// </summary>
    public int MinimumIdle => 0;

    /// <summary>
    ///     获取连接的超时时间
    /// </summary>
    public TimeSpan ConnectionTimeout => TimeSpan.Zero;

    /// <summary>
    ///     空闲连接的超时时间
    /// </summary>
    public TimeSpan IdleTimeout => TimeSpan.Zero;

    /// <summary>
    ///     连接有效性校验的超时时间
    /// </summary>
    public TimeSpan ValidationTimeout => TimeSpan.Zero;

    /// <summary>
    ///     连接保活的时间间隔
    /// </summary>
    public TimeSpan KeepaliveTime => TimeSpan.Zero;

    /// <summary>
    ///     连接的最大存活时间
    /// </summary>
    public TimeSpan MaxLifetime => TimeSpan.Zero;

    /// <summary>
    ///     连接泄漏检测的阈值
    /// </summary>
    public TimeSpan LeakDetectionThreshold => TimeSpan.Zero;

    /// <summary>
    ///     连接是否自动提交事务
    /// </summary>
    public bool? AutoCommit => null;

    /// <summary>
    ///     连接是否只读
    /// </summary>
    public bool? ReadOnly => null;

    /// <summary>
    ///     连接有效性校验使用的语句
    /// </summary>
    public string ConnectionTestQuery => null;
}
