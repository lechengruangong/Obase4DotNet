using Obase.Core;
using Obase.Core.Odm.Builder;
using Obase.Providers.PostgreSql;
using Obase.Providers.Sql;
using Obase.Test.Infrastructure.Configuration;
using Obase.Test.Infrastructure.ModelRegister;

namespace Obase.Test.Infrastructure.Context;

/// <summary>
///     测试用PostgreSql数据源的上下文
/// </summary>
public class PostgreSqlContext : ObjectContext
{
    /// <summary>
    ///     构造测试用PostgreSql数据源的上下文
    /// </summary>
    public PostgreSqlContext() : base(new PostgreSqlContextConfiger())
    {
    }
}

/// <summary>
///     测试用PostgreSql数据上下文配置器
/// </summary>
public class PostgreSqlContextConfiger : PostgreSqlContextConfigProvider
{
    /// <summary>
    ///     构造测试用PostgreSql数据上下文配置器
    /// </summary>
    public PostgreSqlContextConfiger()
    {
        EnableStructMapping = RelationshipDataBaseConfigurationManager.NeedStructMapping != null &&
                              RelationshipDataBaseConfigurationManager.NeedStructMapping.Value;
    }

    /// <summary>
    ///     由派生类实现，获取数据库连接字符串。
    /// </summary>
    protected override string ConnectionString => RelationshipDataBaseConfigurationManager.PostgreSqlConnectionString;

    /// <summary>
    ///     获取一个值，该值指示是否启用存储结构映射
    /// </summary>
    protected override bool EnableStructMapping { get; }


    /// <summary>
    ///     使用指定的建模器创建对象数据模型。
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void CreateModel(ModelBuilder modelBuilder)
    {
        //注册核心模型
        CoreModelRegister.Regist(EDataSource.PostgreSql, modelBuilder);
    }
}