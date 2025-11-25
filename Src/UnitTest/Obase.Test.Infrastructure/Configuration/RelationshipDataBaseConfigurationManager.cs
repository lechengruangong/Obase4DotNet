using System.IO;
using Microsoft.Extensions.Configuration;

namespace Obase.Test.Infrastructure.Configuration;

/// <summary>
///     关系型数据库配置管理者
///     会从Obase.Test.Config.json文件内读取配置
/// </summary>
public static class RelationshipDataBaseConfigurationManager
{
    /// <summary>
    ///     关系型数据库的配置
    /// </summary>
    private static IConfiguration _relationshipDataBaseConfiguration;

    /// <summary>
    ///     MySql连接字符串的配置
    /// </summary>
    public static readonly string MySqlConnectionString =
        RelationshipDataBaseConfiguration["MySqlConnectionString"];

    /// <summary>
    ///     Sqlite连接字符串的配置
    /// </summary>
    public static readonly string SqliteConnectionString =
        RelationshipDataBaseConfiguration["SqliteConnectionString"];

    /// <summary>
    ///     SqlServer连接字符串的配置
    /// </summary>
    public static readonly string SqlServerConnectionString =
        RelationshipDataBaseConfiguration["SqlServerConnectionString"];

    /// <summary>
    ///     PostgreSql连接字符串的配置
    /// </summary>
    public static readonly string PostgreSqlConnectionString =
        RelationshipDataBaseConfiguration["PostgreSqlConnectionString"];

    /// <summary>
    ///     是否需要结构映射的配置
    /// </summary>
    public static readonly bool? NeedStructMapping =
        RelationshipDataBaseConfiguration.GetSection("NeedStructMapping").Get<bool?>();

    /// <summary>
    ///     关系型数据库的配置
    /// </summary>
    private static IConfiguration RelationshipDataBaseConfiguration =>
        _relationshipDataBaseConfiguration ??= BuildRdbConfig();

    /// <summary>
    ///     创建配置
    ///     使用当前文件夹上层目录下的Obase.Test.Config.json文件作为配置源
    /// </summary>
    /// <returns></returns>
    private static IConfiguration BuildRdbConfig()
    {
        //固定寻找当前路径上级的Obase.Test.Config.json 文件
        //代码库中不提供此文件 请参考Obase.Test.Config.example.json 创建此文件
        // 示例文件位于Obase.Test项目文件夹下
        var builder = new ConfigurationBuilder()
            .AddJsonFile($"{Directory.GetCurrentDirectory()}/../Obase.Test.Config.json", false);
        var config = builder.Build();

        return config;
    }
}