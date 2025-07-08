using System;
using System.Collections.Generic;
using System.Linq;
using Obase.Core.Saving;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Functional;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     简单属性测试并发冲突策略
/// </summary>
[TestFixture]
public class SimpleAttributeConcurrentConflictTest
{
    /// <summary>
    ///     构造实例 为上下文赋值
    /// </summary>
    [OneTimeSetUp]
    public void SetUp()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //都是一个表 清理一次即可
            context.CreateSet<IngoreKeyValue>().Delete(p => p.Id > 0);
        }
    }


    /// <summary>
    ///     销毁对象
    /// </summary>
    [OneTimeTearDown]
    public void Dispose()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //都是一个表 清理一次即可
            context.CreateSet<IngoreKeyValue>().Delete(p => p.Id > 0);
        }
    }

    /// <summary>
    ///     测试 简单属性 合并策略 属性累加处理
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void SimpleAttributeAccumulateCombineConcurrentConflict(EDataSource dataSource)
    {
        //构造对象
        var accumulateCombineKeyValue = new AccumulateCombineKeyValue
        {
            Id = 1,
            Key = "Key",
            Value = 5,
            VersionKey = 1
        };

        //附加
        var context = ContextUtils.CreateContext(dataSource);
        context.CreateSet<AccumulateCombineKeyValue>().Attach(accumulateCombineKeyValue);
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);
        //重复插入
        accumulateCombineKeyValue = new AccumulateCombineKeyValue
        {
            Id = 1,
            Key = "Key",
            Value = 2,
            VersionKey = 1
        };

        AccumulateCombineKeyValue queryAccumulateKeyValue;
        switch (dataSource)
        {
            case EDataSource.MySql or EDataSource.Sqlite or EDataSource.SqlServer:
                //对象被合并 属性被覆盖
                context.CreateSet<AccumulateCombineKeyValue>().Attach(accumulateCombineKeyValue);
                context.SaveChanges();

                context = ContextUtils.CreateContext(dataSource);
                queryAccumulateKeyValue = context.CreateSet<AccumulateCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
                //被累加至7
                Assert.That(queryAccumulateKeyValue, Is.Not.Null);
                Assert.That(queryAccumulateKeyValue.Value, Is.EqualTo(7));
                break;
            case EDataSource.PostgreSql:
                //如果是PostgreSql 因为PostgreSql不支持发生异常后继续修改对象 会抛出一个特定的异常
                Assert.Throws<UnSupportedException>(() =>
                {
                    context.CreateSet<AccumulateCombineKeyValue>().Attach(accumulateCombineKeyValue);
                    context.SaveChanges();
                });

                //查出来验证
                context = ContextUtils.CreateContext(dataSource);
                queryAccumulateKeyValue = context.CreateSet<AccumulateCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
                //仍然是5
                Assert.That(queryAccumulateKeyValue, Is.Not.Null);
                Assert.That(queryAccumulateKeyValue.Value, Is.EqualTo(5));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, "不支持的数据库类型");
        }

        //修改
        queryAccumulateKeyValue.Value = 9;
        queryAccumulateKeyValue.VersionKey = 2;
        //用就地修改方法 模拟一个版本键被其他线程修改
        context.CreateSet<AccumulateCombineKeyValue>().SetAttributes(
            new[] { new KeyValuePair<string, object>("VersionKey", 2) },
            p => p.Id == 1);
        //会被合并 属性被累加
        context.SaveChanges();

        //重新查出来
        context = ContextUtils.CreateContext(dataSource);
        queryAccumulateKeyValue = context.CreateSet<AccumulateCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
        //累加至9
        Assert.That(queryAccumulateKeyValue, Is.Not.Null);
        Assert.That(queryAccumulateKeyValue.Value, Is.EqualTo(9));
        //清理数据 防止影响其他测试
        context.CreateSet<AccumulateCombineKeyValue>().Delete(p => p.Id > 0);
    }

    /// <summary>
    ///     测试 简单属性 合并策略 属性忽略处理
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void SimpleAttributeIgnoreCombineConcurrentConflict(EDataSource dataSource)
    {
        //构造对象
        var ignoreCombineKeyValue = new IgnoreCombineKeyValue
        {
            Id = 1,
            Key = "Key",
            Value = 5,
            VersionKey = 1
        };

        //附加
        var context = ContextUtils.CreateContext(dataSource);
        context.CreateSet<IgnoreCombineKeyValue>().Attach(ignoreCombineKeyValue);
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);
        //重复插入
        ignoreCombineKeyValue = new IgnoreCombineKeyValue
        {
            Id = 1,
            Key = "Key",
            Value = 2,
            VersionKey = 1
        };

        IgnoreCombineKeyValue queryIngoreKeyValue;
        switch (dataSource)
        {
            case EDataSource.MySql or EDataSource.Sqlite or EDataSource.SqlServer:
                //对象被合并 属性被忽略
                context.CreateSet<IgnoreCombineKeyValue>().Attach(ignoreCombineKeyValue);
                context.SaveChanges();

                context = ContextUtils.CreateContext(dataSource);
                queryIngoreKeyValue = context.CreateSet<IgnoreCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
                //仍然是5
                Assert.That(queryIngoreKeyValue, Is.Not.Null);
                Assert.That(queryIngoreKeyValue.Value, Is.EqualTo(5));
                break;
            case EDataSource.PostgreSql:
                //如果是PostgreSql 因为PostgreSql不支持发生异常后继续修改对象 会抛出一个特定的异常
                Assert.Throws<UnSupportedException>(() =>
                {
                    context.CreateSet<IgnoreCombineKeyValue>().Attach(ignoreCombineKeyValue);
                    context.SaveChanges();
                });
                //查出来验证
                context = ContextUtils.CreateContext(dataSource);
                queryIngoreKeyValue = context.CreateSet<IgnoreCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
                //仍然是5
                Assert.That(queryIngoreKeyValue, Is.Not.Null);
                Assert.That(queryIngoreKeyValue.Value, Is.EqualTo(5));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, "不支持的数据库类型");
        }

        //修改
        queryIngoreKeyValue.Value = 9;
        queryIngoreKeyValue.VersionKey = 2;
        //用就地修改方法 模拟一个版本键被其他线程修改
        context.CreateSet<IgnoreCombineKeyValue>().SetAttributes(
            new[] { new KeyValuePair<string, object>("VersionKey", 2) },
            p => p.Id == 1);
        //会被合并 属性被忽略
        context.SaveChanges();

        //重新查出来
        context = ContextUtils.CreateContext(dataSource);
        queryIngoreKeyValue = context.CreateSet<IgnoreCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
        //仍然是5
        Assert.That(queryIngoreKeyValue, Is.Not.Null);
        Assert.That(queryIngoreKeyValue.Value, Is.EqualTo(5));
        //清理数据 防止影响其他测试
        context.CreateSet<IgnoreCombineKeyValue>().Delete(p => p.Id > 0);
    }

    /// <summary>
    ///     测试 简单属性 合并策略 属性覆盖处理
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void SimpleAttributeOverWriteCombineConcurrentConflict(EDataSource dataSource)
    {
        //构造对象
        var overwriteCombineKeyValue = new OverwriteCombineKeyValue
        {
            Id = 1,
            Key = "Key",
            Value = 1,
            VersionKey = 1
        };

        //附加
        var context = ContextUtils.CreateContext(dataSource);
        context.CreateSet<OverwriteCombineKeyValue>().Attach(overwriteCombineKeyValue);
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);
        //重复插入
        overwriteCombineKeyValue = new OverwriteCombineKeyValue
        {
            Id = 1,
            Key = "Key",
            Value = 2,
            VersionKey = 1
        };

        OverwriteCombineKeyValue queryOverWriteKeyValue;
        switch (dataSource)
        {
            case EDataSource.MySql or EDataSource.Sqlite:
            case EDataSource.SqlServer:
                //对象被合并 属性被覆盖
                context.CreateSet<OverwriteCombineKeyValue>().Attach(overwriteCombineKeyValue);
                context.SaveChanges();

                context = ContextUtils.CreateContext(dataSource);
                queryOverWriteKeyValue = context.CreateSet<OverwriteCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
                //是新对象的2
                Assert.That(queryOverWriteKeyValue, Is.Not.Null);
                Assert.That(queryOverWriteKeyValue.Value, Is.EqualTo(2));
                break;
            case EDataSource.PostgreSql:
                //如果是PostgreSql 因为PostgreSql不支持发生异常后继续修改对象 会抛出一个特定的异常
                Assert.Throws<UnSupportedException>(() =>
                {
                    context.CreateSet<OverwriteCombineKeyValue>().Attach(overwriteCombineKeyValue);
                    context.SaveChanges();
                });
                //查出来验证
                context = ContextUtils.CreateContext(dataSource);
                queryOverWriteKeyValue = context.CreateSet<OverwriteCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
                //是之前的1
                Assert.That(queryOverWriteKeyValue, Is.Not.Null);
                Assert.That(queryOverWriteKeyValue.Value, Is.EqualTo(1));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, "不支持的数据库类型");
        }


        //修改
        queryOverWriteKeyValue.Value = 9;
        queryOverWriteKeyValue.VersionKey = 2;
        //用就地修改方法 模拟一个版本键被其他线程修改
        context.CreateSet<OverwriteCombineKeyValue>().SetAttributes(
            new[] { new KeyValuePair<string, object>("VersionKey", 2) },
            p => p.Id == 1);
        //会被合并 属性被覆盖
        context.SaveChanges();

        //重新查出来
        context = ContextUtils.CreateContext(dataSource);
        queryOverWriteKeyValue = context.CreateSet<OverwriteCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
        //覆盖成修改后的9
        Assert.That(queryOverWriteKeyValue, Is.Not.Null);
        Assert.That(queryOverWriteKeyValue.Value, Is.EqualTo(9));
        //清理数据 防止影响其他测试
        context.CreateSet<OverwriteCombineKeyValue>().Delete(p => p.Id > 0);
    }

    /// <summary>
    ///     测试 简单属性 忽略策略
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void SimpleAttributeIngoreConcurrentConflict(EDataSource dataSource)
    {
        //构造对象
        var ingoreSimpleKeyValue = new IngoreKeyValue
        {
            Id = 1,
            Key = "Key",
            Value = 1,
            VersionKey = 1
        };

        //附加
        var context = ContextUtils.CreateContext(dataSource);
        context.CreateSet<IngoreKeyValue>().Attach(ingoreSimpleKeyValue);
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);
        //重复插入
        ingoreSimpleKeyValue = new IngoreKeyValue
        {
            Id = 1,
            Key = "Key",
            Value = 2,
            VersionKey = 1
        };

        IngoreKeyValue queryIngoreSimpleKeyValue;
        switch (dataSource)
        {
            case EDataSource.MySql or EDataSource.Sqlite:
            case EDataSource.SqlServer:
                //忽略策略 不修改旧数据
                context.CreateSet<IngoreKeyValue>().Attach(ingoreSimpleKeyValue);
                context.SaveChanges();
                //查出来验证
                context = ContextUtils.CreateContext(dataSource);
                queryIngoreSimpleKeyValue = context.CreateSet<IngoreKeyValue>().FirstOrDefault(p => p.Id == 1);
                //还是之前的1
                Assert.That(queryIngoreSimpleKeyValue, Is.Not.Null);
                Assert.That(queryIngoreSimpleKeyValue.Value, Is.EqualTo(1));
                break;
            case EDataSource.PostgreSql:
                //如果是PostgreSql 因为PostgreSql不支持发生异常后继续修改对象 会抛出一个特定的异常
                Assert.Throws<UnSupportedException>(() =>
                {
                    context.CreateSet<IngoreKeyValue>().Attach(ingoreSimpleKeyValue);
                    context.SaveChanges();
                });
                //查出来验证
                context = ContextUtils.CreateContext(dataSource);
                queryIngoreSimpleKeyValue = context.CreateSet<IngoreKeyValue>().FirstOrDefault(p => p.Id == 1);
                //还是之前的1
                Assert.That(queryIngoreSimpleKeyValue, Is.Not.Null);
                Assert.That(queryIngoreSimpleKeyValue.Value, Is.EqualTo(1));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, "不支持的数据库类型");
        }


        //修改
        queryIngoreSimpleKeyValue.Value = 2;
        queryIngoreSimpleKeyValue.VersionKey = 2;
        //用就地修改方法 模拟一个版本键被其他线程修改
        context.CreateSet<IngoreKeyValue>().SetAttributes(new[] { new KeyValuePair<string, object>("VersionKey", 2) },
            p => p.Id == 1);
        //会被忽略
        context.SaveChanges();

        //重新查出来
        context = ContextUtils.CreateContext(dataSource);
        queryIngoreSimpleKeyValue = context.CreateSet<IngoreKeyValue>().FirstOrDefault(p => p.Id == 1);
        //被忽略 实际数据未改动 还是1
        Assert.That(queryIngoreSimpleKeyValue, Is.Not.Null);
        Assert.That(queryIngoreSimpleKeyValue.Value, Is.EqualTo(1));

        //清理数据  防止影响其他测试
        context.CreateSet<IngoreKeyValue>().Delete(p => p.Id > 0);
    }

    /// <summary>
    ///     测试 简单属性 覆盖策略
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void SimpleAttributeOverWriteConcurrentConflict(EDataSource dataSource)
    {
        //构造对象
        var overWriteKeyValue = new OverwriteKeyValue
        {
            Id = 1,
            Key = "Key",
            Value = 1,
            VersionKey = 1
        };

        //附加
        var context = ContextUtils.CreateContext(dataSource);
        context.CreateSet<OverwriteKeyValue>().Attach(overWriteKeyValue);
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);
        //重复插入
        overWriteKeyValue = new OverwriteKeyValue
        {
            Id = 1,
            Key = "Key",
            Value = 2,
            VersionKey = 1
        };

        OverwriteKeyValue queryOverWriteKeyValue;
        switch (dataSource)
        {
            case EDataSource.MySql or EDataSource.Sqlite:
            case EDataSource.SqlServer:
                //对象被覆盖
                context.CreateSet<OverwriteKeyValue>().Attach(overWriteKeyValue);
                context.SaveChanges();
                //查出来验证
                context = ContextUtils.CreateContext(dataSource);
                queryOverWriteKeyValue = context.CreateSet<OverwriteKeyValue>().FirstOrDefault(p => p.Id == 1);
                //被覆盖成2
                Assert.That(queryOverWriteKeyValue, Is.Not.Null);
                Assert.That(queryOverWriteKeyValue.Value, Is.EqualTo(2));
                break;
            case EDataSource.PostgreSql:
                //如果是PostgreSql 因为PostgreSql不支持发生异常后继续修改对象 会抛出一个特定的异常
                Assert.Throws<UnSupportedException>(() =>
                {
                    context.CreateSet<OverwriteKeyValue>().Attach(overWriteKeyValue);
                    context.SaveChanges();
                });
                //查出来验证
                context = ContextUtils.CreateContext(dataSource);
                queryOverWriteKeyValue = context.CreateSet<OverwriteKeyValue>().FirstOrDefault(p => p.Id == 1);
                //还是1 没改动
                Assert.That(queryOverWriteKeyValue, Is.Not.Null);
                Assert.That(queryOverWriteKeyValue.Value, Is.EqualTo(1));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, "不支持的数据库类型");
        }

        //修改
        queryOverWriteKeyValue.Value = 3;
        queryOverWriteKeyValue.VersionKey = 2;
        //用就地修改方法 模拟一个版本键被其他线程修改
        context.CreateSet<OverwriteKeyValue>().SetAttributes(
            new[] { new KeyValuePair<string, object>("VersionKey", 2) },
            p => p.Id == 1);
        //会被覆盖
        context.SaveChanges();

        //重新查出来
        context = ContextUtils.CreateContext(dataSource);
        queryOverWriteKeyValue = context.CreateSet<OverwriteKeyValue>().FirstOrDefault(p => p.Id == 1);
        //覆盖成3
        Assert.That(queryOverWriteKeyValue, Is.Not.Null);
        Assert.That(queryOverWriteKeyValue.Value, Is.EqualTo(3));
        //清理数据  防止影响其他测试
        context.CreateSet<OverwriteKeyValue>().Delete(p => p.Id > 0);
    }

    /// <summary>
    ///     测试 简单属性 重建策略
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void SimpleAttributeReconstructConcurrentConflict(EDataSource dataSource)
    {
        //构造对象
        var reconstructeKeyValue = new ReconstructKeyValue
        {
            Id = 1,
            Key = "Key",
            Value = 1,
            VersionKey = 1
        };

        //附加
        var context = ContextUtils.CreateContext(dataSource);
        context.CreateSet<ReconstructKeyValue>().Attach(reconstructeKeyValue);
        context.SaveChanges();
        //重建对象策略只处理修改时并发
        var queryReconstructKeyValue = context.CreateSet<ReconstructKeyValue>().FirstOrDefault(p => p.Id == 1);
        //取出对象
        Assert.That(queryReconstructKeyValue, Is.Not.Null);
        //修改
        queryReconstructKeyValue.Value = 4;
        queryReconstructKeyValue.VersionKey = 2;
        //用就地修改方法 模拟一个主键被修改 相当于此对象不存在了
        context.CreateSet<IngoreKeyValue>().SetAttributes(new[] { new KeyValuePair<string, object>("Id", 2) },
            p => p.Id == 1);
        //会重建新对象
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);
        queryReconstructKeyValue = context.CreateSet<ReconstructKeyValue>().FirstOrDefault(p => p.Id == 1);
        //新增了一个ID是1 Value是4的对象
        Assert.That(queryReconstructKeyValue, Is.Not.Null);
        Assert.That(queryReconstructKeyValue.Value, Is.EqualTo(4));

        //原对象 主键改成了2
        queryReconstructKeyValue = context.CreateSet<ReconstructKeyValue>().FirstOrDefault(p => p.Id == 2);
        //存在
        Assert.That(queryReconstructKeyValue, Is.Not.Null);
        Assert.That(queryReconstructKeyValue.Value, Is.EqualTo(1));
        //清理数据  防止影响其他测试
        context.CreateSet<ReconstructKeyValue>().Delete(p => p.Id > 0);
    }

    /// <summary>
    ///     测试 简单属性 抛出异常策略
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void SimpleAttributeThrowExceptionConcurrentConflict(EDataSource dataSource)
    {
        //构造对象
        var throwExceptionSimpleKeyValue = new ThrowExceptionKeyValue
        {
            Id = 1,
            Key = "Key",
            Value = 1,
            VersionKey = 1
        };

        //附加
        var context = ContextUtils.CreateContext(dataSource);
        context.CreateSet<ThrowExceptionKeyValue>().Attach(throwExceptionSimpleKeyValue);
        context.SaveChanges();

        //重复插入 异常会被抛出
        Assert.Throws<RepeatCreationException>(() =>
        {
            context = ContextUtils.CreateContext(dataSource);

            throwExceptionSimpleKeyValue = new ThrowExceptionKeyValue
            {
                Id = 1,
                Key = "Key",
                Value = 1,
                VersionKey = 1
            };
            //异常被抛出
            context.CreateSet<ThrowExceptionKeyValue>().Attach(throwExceptionSimpleKeyValue);
            context.SaveChanges();
        });

        //重复插入 异常会被抛出
        Assert.Throws<VersionConflictException>(() =>
        {
            var queryThrowExceptionKeyValue =
                context.CreateSet<ThrowExceptionKeyValue>().FirstOrDefault(p => p.Id == 1);
            //旧对象存在
            Assert.That(queryThrowExceptionKeyValue, Is.Not.Null);

            //修改
            queryThrowExceptionKeyValue.Value = 2;
            queryThrowExceptionKeyValue.VersionKey = 2;

            //用就地修改方法 模拟一个版本键被其他现场修改
            context.CreateSet<ThrowExceptionKeyValue>().SetAttributes(
                new[] { new KeyValuePair<string, object>("VersionKey", 2) },
                p => p.Id == 1);
            //异常被抛出
            context.SaveChanges();
        });

        //清理数据 防止影响其他测试
        context.CreateSet<ThrowExceptionKeyValue>().Delete(p => p.Id > 0);
    }
}