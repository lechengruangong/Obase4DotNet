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
}