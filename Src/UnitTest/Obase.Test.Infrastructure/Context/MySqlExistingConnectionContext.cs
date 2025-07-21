using System.Data.Common;
using Obase.Core;
using Obase.Core.Odm.Builder;
using Obase.Providers.Sql;
using Obase.Test.Infrastructure.ModelRegister;

namespace Obase.Test.Infrastructure.Context;

/// <summary>
///     使用已存在的MySql连接上下文
/// </summary>
public class MySqlExistingConnectionContext : ObjectContext
{
    /// <summary>
    ///     构造ObjectContext对象
    /// </summary>
    /// <param name="providerFactory">用于创建数据提供程序类实例的工厂</param>
    /// <param name="connection">连接</param>
    public MySqlExistingConnectionContext(DbProviderFactory providerFactory, DbConnection connection,
        DbTransaction transaction = null) : base(
        new MySqlExistingConnectionContextConfiger(
            new ExistingConnectionSqlExecutor(providerFactory, connection, EDataSource.MySql, transaction)))
    {
    }
}

/// <summary>
///     使用已存在的MySql连接上下文配置
/// </summary>
public class MySqlExistingConnectionContextConfiger : SqlContextConfigurator
{
    /// <summary>
    ///     使用已存在的MySql连接上下文配置。
    /// </summary>
    /// <param name="sqlExecutor">Sql语句执行器。</param>
    /// <param name="enableStructMapping">指示是否启用存储架构映射</param>
    public MySqlExistingConnectionContextConfiger(ISqlExecutor sqlExecutor, bool enableStructMapping = false) : base(
        sqlExecutor, enableStructMapping)
    {
    }

    /// <summary>
    ///     使用指定的建模器创建对象数据模型。
    /// </summary>
    /// <param name="modelBuilder">对象数据模型建造器</param>
    protected override void CreateModel(ModelBuilder modelBuilder)
    {
        //注册核心模型
        CoreModelRegister.Regist(modelBuilder);
    }
}