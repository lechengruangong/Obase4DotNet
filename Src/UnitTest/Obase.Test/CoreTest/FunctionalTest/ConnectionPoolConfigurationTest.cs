using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Obase.Core;
using Obase.Core.DependencyInjection;
using Obase.Providers.Sql.ConnectionPool;
using Obase.Test.Infrastructure.Configuration;
using Obase.Test.Infrastructure.Context;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     连接池配置测试
///     连接池按连接字符串缓存且不主动移除 故每个用例使用独立的上下文类型与独立的连接字符串 避免相互干扰
///     测试不依赖任何真实数据库 统一使用SQLite的内存库
/// </summary>
[TestFixture]
public class ConnectionPoolConfigurationTest
{
    /// <summary>
    ///     本次测试创建的连接池
    /// </summary>
    private readonly List<DbConnectionPool> _pools = new List<DbConnectionPool>();

    /// <summary>
    ///     释放测试创建的连接池
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        foreach (var pool in _pools) pool.Dispose();
        _pools.Clear();
    }

    /// <summary>
    ///     生成一个唯一的连接字符串 保证每个用例拿到的是新建的连接池
    /// </summary>
    private static string CreateConnectionString()
    {
        return $"Data Source=obase_pool_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
    }

    /// <summary>
    ///     为指定上下文类型注册连接池配置并创建容器
    /// </summary>
    private static void RegisterConfiguration<TContext>(IObaseConnectionPoolConfiguration configuration)
        where TContext : ObjectContext
    {
        var builder = ObaseDependencyInjection.CreateBuilder(typeof(TContext));
        builder.AddSingleton<IObaseConnectionPoolConfiguration>(_ => configuration);
        builder.Build();
    }

    /// <summary>
    ///     按连接池配置建池并返回
    /// </summary>
    private DbConnectionPool CreatePool(Type contextType)
    {
        var pool = ObaseConnectionPool.Current.GetPool(CreateConnectionString(), SqliteFactory.Instance, contextType);
        _pools.Add(pool);
        return pool;
    }

    /// <summary>
    ///     配置项有效时 连接池策略应当采用配置的值
    /// </summary>
    [Test]
    public void PoolPolicy_AppliesConfiguredValues()
    {
        RegisterConfiguration<CustomConfigurationContext>(new PoolConfiguration
        {
            Name = "自定义连接池",
            MaximumPoolSize = 42,
            MinimumIdle = 2,
            ConnectionTimeout = TimeSpan.FromSeconds(7),
            IdleTimeout = TimeSpan.FromSeconds(33),
            ValidationTimeout = TimeSpan.FromSeconds(9),
            KeepaliveTime = TimeSpan.FromSeconds(13),
            ConnectionTestQuery = "SELECT 1 WHERE 1 = 1"
        });

        var policy = CreatePool(typeof(CustomConfigurationContext)).Policy;

        Assert.Multiple(() =>
        {
            Assert.That(policy.Name, Is.EqualTo("自定义连接池"));
            Assert.That(policy.PoolSize, Is.EqualTo(42));
            Assert.That(policy.MinimumIdle, Is.EqualTo(2));
            //连接超时对应策略中的同步获取超时
            Assert.That(policy.SyncGetTimeout, Is.EqualTo(TimeSpan.FromSeconds(7)));
            Assert.That(policy.IdleTimeout, Is.EqualTo(TimeSpan.FromSeconds(33)));
            Assert.That(policy.ValidationTimeout, Is.EqualTo(TimeSpan.FromSeconds(9)));
            Assert.That(policy.ConnectionTestQuery, Is.EqualTo("SELECT 1 WHERE 1 = 1"));
            Assert.That(policy.CheckAvailableInterval, Is.EqualTo(13));
        });
    }

    /// <summary>
    ///     配置项为无效值(空字符串 零 负数 空引用)时 连接池策略应当回落到默认值
    /// </summary>
    [Test]
    public void PoolPolicy_FallsBackToDefaultsWhenConfiguredValuesAreInvalid()
    {
        RegisterConfiguration<InvalidConfigurationContext>(new PoolConfiguration
        {
            Name = "",
            MaximumPoolSize = 0,
            MinimumIdle = -1,
            ConnectionTimeout = TimeSpan.Zero,
            IdleTimeout = TimeSpan.FromSeconds(-1),
            ValidationTimeout = TimeSpan.Zero,
            KeepaliveTime = TimeSpan.Zero,
            ConnectionTestQuery = ""
        });

        var policy = CreatePool(typeof(InvalidConfigurationContext)).Policy;

        Assert.Multiple(() =>
        {
            //名称回落到带序号的内置名称 序号取决于建池顺序 故只断言前缀
            Assert.That(policy.Name, Does.StartWith("Obase ConnectionPool"));
            Assert.That(policy.PoolSize, Is.EqualTo(100));
            Assert.That(policy.MinimumIdle, Is.EqualTo(5));
            Assert.That(policy.SyncGetTimeout, Is.EqualTo(TimeSpan.FromSeconds(10)));
            Assert.That(policy.IdleTimeout, Is.EqualTo(TimeSpan.FromSeconds(20)));
            Assert.That(policy.ValidationTimeout, Is.EqualTo(TimeSpan.FromSeconds(5)));
            Assert.That(policy.ConnectionTestQuery, Is.EqualTo("SELECT 1"));
            Assert.That(policy.CheckAvailableInterval, Is.EqualTo(5));
        });
    }

    /// <summary>
    ///     未注册连接池配置时 连接池策略应当使用默认值
    /// </summary>
    [Test]
    public void PoolPolicy_UsesDefaultsWhenNoConfigurationRegistered()
    {
        var policy = CreatePool(typeof(NoConfigurationContext)).Policy;

        Assert.Multiple(() =>
        {
            Assert.That(policy.Name, Does.StartWith("Obase ConnectionPool"));
            Assert.That(policy.PoolSize, Is.EqualTo(100));
            Assert.That(policy.MinimumIdle, Is.EqualTo(5));
            Assert.That(policy.SyncGetTimeout, Is.EqualTo(TimeSpan.FromSeconds(10)));
            Assert.That(policy.IdleTimeout, Is.EqualTo(TimeSpan.FromSeconds(20)));
            Assert.That(policy.ValidationTimeout, Is.EqualTo(TimeSpan.FromSeconds(5)));
            Assert.That(policy.ConnectionTestQuery, Is.EqualTo("SELECT 1"));
            Assert.That(policy.CheckAvailableInterval, Is.EqualTo(5));
        });
    }

    /// <summary>
    ///     .NET版连接池不支持最大生存期 泄漏检测 自动提交与只读 配置了也不影响建池与其他策略值
    /// </summary>
    [Test]
    public void PoolPolicy_IgnoresOptionsUnsupportedByDotNetPool()
    {
        RegisterConfiguration<UnsupportedOptionContext>(new PoolConfiguration
        {
            MaximumPoolSize = 20,
            MaxLifetime = TimeSpan.FromMinutes(30),
            LeakDetectionThreshold = TimeSpan.FromSeconds(15),
            AutoCommit = true,
            ReadOnly = true
        });

        var pool = CreatePool(typeof(UnsupportedOptionContext));
        var policy = pool.Policy;

        Assert.Multiple(() =>
        {
            //支持的配置项照常生效
            Assert.That(policy.PoolSize, Is.EqualTo(20));
            //不支持的配置项不影响其他值
            Assert.That(policy.Name, Does.StartWith("Obase ConnectionPool"));
            Assert.That(policy.MinimumIdle, Is.EqualTo(5));
            Assert.That(policy.SyncGetTimeout, Is.EqualTo(TimeSpan.FromSeconds(10)));
            Assert.That(policy.ConnectionTestQuery, Is.EqualTo("SELECT 1"));
            //连接池仍然正常建立
            Assert.That(policy.ConnectionString, Is.Not.Empty);
            Assert.That(pool.ProviderFactory, Is.SameAs(SqliteFactory.Instance));
        });
    }

    /// <summary>
    ///     保活时间换算为后台检查间隔 不足一秒按五秒计算 其余向上取整到秒
    /// </summary>
    [Test]
    public void PoolPolicy_ConvertsKeepaliveTimeToCheckAvailableInterval()
    {
        RegisterConfiguration<ShortKeepaliveContext>(new PoolConfiguration
        {
            KeepaliveTime = TimeSpan.FromMilliseconds(500)
        });
        RegisterConfiguration<ThreeSecondKeepaliveContext>(new PoolConfiguration
        {
            KeepaliveTime = TimeSpan.FromSeconds(3)
        });
        RegisterConfiguration<FractionKeepaliveContext>(new PoolConfiguration
        {
            KeepaliveTime = TimeSpan.FromMilliseconds(1500)
        });

        var shortInterval = CreatePool(typeof(ShortKeepaliveContext)).Policy.CheckAvailableInterval;
        var threeSecondsInterval = CreatePool(typeof(ThreeSecondKeepaliveContext)).Policy.CheckAvailableInterval;
        var fractionInterval = CreatePool(typeof(FractionKeepaliveContext)).Policy.CheckAvailableInterval;

        Assert.Multiple(() =>
        {
            Assert.That(shortInterval, Is.EqualTo(5));
            Assert.That(threeSecondsInterval, Is.EqualTo(3));
            Assert.That(fractionInterval, Is.EqualTo(2));
        });
    }

    /// <summary>
    ///     测试基础设施中的连接池配置实现 其属性值应当与构造参数一致
    /// </summary>
    [Test]
    public void ObaseConnectionPoolConfiguration_ExposesExpectedValues()
    {
        var configuration = new ObaseConnectionPoolConfiguration("测试连接池");

        Assert.Multiple(() =>
        {
            Assert.That(configuration.Name, Is.EqualTo("测试连接池"));
            Assert.That(configuration.MaximumPoolSize, Is.EqualTo(50));
            //最小空闲数为零 表示不额外预热连接
            Assert.That(configuration.MinimumIdle, Is.EqualTo(0));
            Assert.That(configuration.ConnectionTimeout, Is.EqualTo(TimeSpan.Zero));
            Assert.That(configuration.IdleTimeout, Is.EqualTo(TimeSpan.Zero));
            Assert.That(configuration.ValidationTimeout, Is.EqualTo(TimeSpan.Zero));
            Assert.That(configuration.KeepaliveTime, Is.EqualTo(TimeSpan.Zero));
            Assert.That(configuration.MaxLifetime, Is.EqualTo(TimeSpan.Zero));
            Assert.That(configuration.LeakDetectionThreshold, Is.EqualTo(TimeSpan.Zero));
            //可空项与字符串项默认为空 表示不设置
            Assert.That(configuration.AutoCommit, Is.Null);
            Assert.That(configuration.ReadOnly, Is.Null);
            Assert.That(configuration.ConnectionTestQuery, Is.Null);
        });
    }

    #region 测试用上下文与配置

    /// <summary>
    ///     使用自定义配置的上下文
    /// </summary>
    private sealed class CustomConfigurationContext : ObjectContext<SqliteContextConfiger>
    {
    }

    /// <summary>
    ///     使用无效配置的上下文
    /// </summary>
    private sealed class InvalidConfigurationContext : ObjectContext<SqliteContextConfiger>
    {
    }

    /// <summary>
    ///     未注册配置的上下文
    /// </summary>
    private sealed class NoConfigurationContext : ObjectContext<SqliteContextConfiger>
    {
    }

    /// <summary>
    ///     使用不支持选项的上下文
    /// </summary>
    private sealed class UnsupportedOptionContext : ObjectContext<SqliteContextConfiger>
    {
    }

    /// <summary>
    ///     保活时间不足一秒的上下文
    /// </summary>
    private sealed class ShortKeepaliveContext : ObjectContext<SqliteContextConfiger>
    {
    }

    /// <summary>
    ///     保活时间为整秒的上下文
    /// </summary>
    private sealed class ThreeSecondKeepaliveContext : ObjectContext<SqliteContextConfiger>
    {
    }

    /// <summary>
    ///     保活时间带小数的上下文
    /// </summary>
    private sealed class FractionKeepaliveContext : ObjectContext<SqliteContextConfiger>
    {
    }

    /// <summary>
    ///     可写全部配置项的连接池配置
    ///     未赋值的项取无效值 用于验证默认值回落
    /// </summary>
    private sealed class PoolConfiguration : IObaseConnectionPoolConfiguration
    {
        public string Name { get; set; }

        public int MaximumPoolSize { get; set; }

        public int MinimumIdle { get; set; }

        public TimeSpan ConnectionTimeout { get; set; }

        public TimeSpan IdleTimeout { get; set; }

        public TimeSpan ValidationTimeout { get; set; }

        public TimeSpan KeepaliveTime { get; set; }

        public TimeSpan MaxLifetime { get; set; }

        public TimeSpan LeakDetectionThreshold { get; set; }

        public bool? AutoCommit { get; set; }

        public bool? ReadOnly { get; set; }

        public string ConnectionTestQuery { get; set; }
    }

    #endregion
}
