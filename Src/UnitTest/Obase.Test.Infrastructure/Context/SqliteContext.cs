using Obase.Core;
using Obase.Core.Odm.Builder;
using Obase.Providers.Sql;
using Obase.Providers.Sqlite;
using Obase.Test.Infrastructure.Configuration;
using Obase.Test.Infrastructure.ModelRegister;

namespace Obase.Test.Infrastructure.Context;

/// <summary>
///     测试用Sqlite数据源的上下文
/// </summary>
public class SqliteContext : ObjectContext
{
    /// <summary>
    ///     构造Sqlite上下文
    /// </summary>
    public SqliteContext() : base(new SqliteContextConfiger())
    {
    }
}

/// <summary>
///     测试用Sqlite数据上下文配置器
/// </summary>
public class SqliteContextConfiger : SqliteContextConfigProvider
{
    /// <summary>
    ///     构造测试用Sqlite数据上下文配置器
    /// </summary>
    public SqliteContextConfiger()
    {
        EnableStructMapping = RelationshipDataBaseConfigurationManager.NeedStructMapping != null &&
                              RelationshipDataBaseConfigurationManager.NeedStructMapping.Value;
    }

    /// <summary>
    ///     由派生类实现，获取数据库连接字符串。
    /// </summary>
    protected override string ConnectionString => RelationshipDataBaseConfigurationManager.SqliteConnectionString;

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
        //注册核心模型
        CoreModelRegister.Regist(EDataSource.Sqlite, modelBuilder);
    }
}