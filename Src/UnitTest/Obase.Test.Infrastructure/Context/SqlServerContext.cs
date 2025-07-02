using Obase.Core;
using Obase.Core.Odm.Builder;
using Obase.Providers.SqlServer;
using Obase.Test.Infrastructure.Configuration;

namespace Obase.Test.Infrastructure.Context;

/// <summary>
///     SqlServer数据源的上下文
/// </summary>
public class SqlServerContext : ObjectContext
{
    /// <summary>
    ///     构造上下文
    /// </summary>
    public SqlServerContext() : base(new SqlServerContextConfiger())
    {
    }
}

/// <summary>
///     测试用SqlServer数据上下文配置器
/// </summary>
public class SqlServerContextConfiger : SqlServerContextConfigProvider
{
    /// <summary>
    ///     测试用SqlServer数据上下文配置器
    /// </summary>
    public SqlServerContextConfiger()
    {
        EnableStructMapping = RelationshipDataBaseConfigurationManager.NeedStructMapping != null &&
                              RelationshipDataBaseConfigurationManager.NeedStructMapping.Value;
    }

    /// <summary>
    ///     由派生类实现，获取数据库连接字符串。
    /// </summary>
    protected override string ConnectionString => RelationshipDataBaseConfigurationManager.SqlServerConnectionString;

    /// <summary>
    ///     获取一个值，该值指示是否启用存储结构映射
    /// </summary>
    protected override bool EnableStructMapping { get; }


    /// <summary>
    ///     使用指定的建模器创建对象数据模型。
    /// </summary>
    /// <param name="modelBuilder">建模器</param>
    protected override void CreateModel(ModelBuilder modelBuilder)
    {
    }
}