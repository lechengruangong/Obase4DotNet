using System.Data.Common;
using Obase.Core;
using Obase.Core.Odm.Builder;
using Obase.Providers.Sql;
using Obase.Test.Infrastructure.ModelRegister;

namespace Obase.Test.Infrastructure.Context;

/// <summary>
///     使用已存在的Sqlite连接上下文
/// </summary>
public class SqliteExistingConnectionContext : ObjectContext
{
    /// <summary>
    ///     构造ObjectContext对象
    /// </summary>
    /// <param name="providerFactory">用于创建数据提供程序类实例的工厂</param>
    /// <param name="connection">连接</param>
    /// <param name="transaction">当前连接中已执行命令的事务,没有开启事务时不需要此参数</param>
    public SqliteExistingConnectionContext(DbProviderFactory providerFactory, DbConnection connection,
        DbTransaction transaction = null) : base(
        new SqliteExistingConnectionContextConfiger(
            new ExistingConnectionSqlExecutor(providerFactory, connection, EDataSource.Sqlite, transaction)))
    {
    }
}

/// <summary>
///     使用已存在的Sqlite连接上下文配置
/// </summary>
public class SqliteExistingConnectionContextConfiger : SqlContextConfigurator
{
    /// <summary>
    ///     使用已存在的Sqlite连接上下文配置。
    /// </summary>
    /// <param name="sqlExecutor">Sql语句执行器。</param>
    /// <param name="enableStructMapping">指示是否启用存储架构映射</param>
    public SqliteExistingConnectionContextConfiger(ISqlExecutor sqlExecutor, bool enableStructMapping = false) :
        base(sqlExecutor, enableStructMapping)
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