using Obase.Core;
using Obase.Core.Odm.Builder;
using Obase.LogicDeletion;
using Obase.MultiTenant;
using Obase.Providers.PostgreSql;
using Obase.Test.Infrastructure.Configuration;
using Obase.Test.Infrastructure.ModelRegister;

namespace Obase.Test.Infrastructure.Context;

/// <summary>
///     PostgreSql数据源的插件测试上下文
/// </summary>
public class PostgreSqlAddonContext : ObjectContext<PostgreSqlAddonContextConfigProvider>
{
    /// <summary>
    ///     初始化PostgreSql数据源的插件测试上下文
    /// </summary>
    public PostgreSqlAddonContext()
    {
        //启用逻辑删除
        this.EnableLogicDeletion();
        //启用多租户
        this.EnableMultiTenant();
    }
}

/// <summary>
///     PostgreSql数据源的插件测试上下文配置提供者
/// </summary>
public class PostgreSqlAddonContextConfigProvider : PostgreSqlContextConfigProvider
{
    /// <summary>
    ///     初始化PostgreSql数据源的插件测试上下文配置提供者
    /// </summary>
    public PostgreSqlAddonContextConfigProvider()
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
    /// <param name="modelBuilder">对象数据模型建造器</param>
    protected override void CreateModel(ModelBuilder modelBuilder)
    {
        //调用插件的模型注册器
        AddonModelRegister.Regist(modelBuilder);
    }
}