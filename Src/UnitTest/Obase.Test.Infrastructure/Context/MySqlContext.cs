using Obase.Core;
using Obase.Core.Odm.Builder;
using Obase.Providers.MySql;
using Obase.Test.Infrastructure.Configuration;

namespace Obase.Test.Infrastructure.Context;

/// <summary>
///     测试用MySql数据上下文
/// </summary>
public class MySqlContext : ObjectContext
{
    /// <summary>
    ///     构造测试用MySql数据上下文
    /// </summary>
    public MySqlContext() : base(new MySqlContextConfiger())
    {
    }
}

/// <summary>
///     测试用MySql数据上下文配置器
/// </summary>
public class MySqlContextConfiger : MySqlContextConfigProvider
{
    /// <summary>
    ///     构造测试用MySql数据上下文配置器
    /// </summary>
    public MySqlContextConfiger()
    {
        EnableStructMapping = RelationshipDataBaseConfigurationManager.NeedStructMapping != null &&
                              RelationshipDataBaseConfigurationManager.NeedStructMapping.Value;
    }

    /// <summary>
    ///     由派生类实现，获取数据库连接字符串。
    /// </summary>
    protected override string ConnectionString => RelationshipDataBaseConfigurationManager.MySqlConnectionString;

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