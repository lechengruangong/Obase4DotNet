using System;
using System.Collections.Generic;
using System.Linq;
using Obase.Core.Saving;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Functional;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     复杂属性测试并发冲突策略
/// </summary>
[TestFixture]
public class ComplexAttributeConcurrentConflictTest
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
            context.CreateSet<ComplexIgnoreKeyValue>().Delete(p => p.Id > 0);
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
            context.CreateSet<ComplexIgnoreKeyValue>().Delete(p => p.Id > 0);
        }
    }

    /// <summary>
    ///     测试 复杂属性 合并策略 属性累加处理
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ComplexAttributeAccumulateCombineConcurrentConflict(EDataSource dataSource)
    {
        //构造对象
        var accumulateCombineKeyValue = new ComplexAccumulateCombineKeyValue
        {
            Id = 1,
            KeyValue = new AccumulateCombineComplexKeyValue
            {
                Key = "Key",
                Value = 5
            },
            VersionKey = 1
        };

        //附加
        var context = ContextUtils.CreateContext(dataSource);
        context.CreateSet<ComplexAccumulateCombineKeyValue>().Attach(accumulateCombineKeyValue);
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);
        //重复插入
        accumulateCombineKeyValue = new ComplexAccumulateCombineKeyValue
        {
            Id = 1,
            KeyValue = new AccumulateCombineComplexKeyValue
            {
                Key = "Key",
                Value = 2
            },
            VersionKey = 1
        };

        ComplexAccumulateCombineKeyValue queryAccumulateKeyValue;
        switch (dataSource)
        {
            case EDataSource.MySql or EDataSource.Sqlite or EDataSource.SqlServer:
                //对象被合并 属性被累加
                context.CreateSet<ComplexAccumulateCombineKeyValue>().Attach(accumulateCombineKeyValue);
                context.SaveChanges();
                //查出来验证
                context = ContextUtils.CreateContext(dataSource);
                queryAccumulateKeyValue =
                    context.CreateSet<ComplexAccumulateCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
                //被累加至7
                Assert.That(queryAccumulateKeyValue, Is.Not.Null);
                Assert.That(queryAccumulateKeyValue.KeyValue.Value, Is.EqualTo(7));
                break;
            case EDataSource.PostgreSql:
                //如果是PostgreSql 因为PostgreSql不支持发生异常后继续修改对象 会抛出一个特定的异常
                Assert.Throws<UnSupportedException>(() =>
                {
                    context.CreateSet<ComplexAccumulateCombineKeyValue>().Attach(accumulateCombineKeyValue);
                    context.SaveChanges();
                });

                //查出来验证
                context = ContextUtils.CreateContext(dataSource);
                queryAccumulateKeyValue =
                    context.CreateSet<ComplexAccumulateCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
                //仍然是5
                Assert.That(queryAccumulateKeyValue, Is.Not.Null);
                Assert.That(queryAccumulateKeyValue.KeyValue.Value, Is.EqualTo(5));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, "不支持的数据库类型");
        }

        //修改
        queryAccumulateKeyValue.VersionKey = 9;
        //用就地修改方法 模拟一个版本键被其他线程修改
        context.CreateSet<ComplexAccumulateCombineKeyValue>().SetAttributes(
            [new KeyValuePair<string, object>("VersionKey", 2)],
            p => p.Id == 1);
        //会被合并 属性被累加
        context.SaveChanges();

        //重新查出来
        context = ContextUtils.CreateContext(dataSource);
        queryAccumulateKeyValue = context.CreateSet<ComplexAccumulateCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
        //累加至9
        Assert.That(queryAccumulateKeyValue, Is.Not.Null);
        Assert.That(queryAccumulateKeyValue.VersionKey, Is.EqualTo(9));
        //清理数据 防止影响其他测试

        context.CreateSet<ComplexAccumulateCombineKeyValue>().Delete(p => p.Id > 0);
    }

    /// <summary>
    ///     测试 复杂属性 合并策略 属性忽略处理
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ComplexAttributeIgnoreCombineConcurrentConflict(EDataSource dataSource)
    {
        //构造对象
        var ignoreCombineKeyValue = new ComplexIgnoreCombineKeyValue
        {
            Id = 1,
            KeyValue = new IgnoreCombineComplexKeyValue
            {
                Key = "Key",
                Value = 1
            },
            VersionKey = 1
        };

        //附加
        var context = ContextUtils.CreateContext(dataSource);
        context.CreateSet<ComplexIgnoreCombineKeyValue>().Attach(ignoreCombineKeyValue);
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);
        //重复插入
        ignoreCombineKeyValue = new ComplexIgnoreCombineKeyValue
        {
            Id = 1,
            KeyValue = new IgnoreCombineComplexKeyValue
            {
                Key = "Key",
                Value = 2
            },
            VersionKey = 1
        };

        ComplexIgnoreCombineKeyValue queryIgnoreSimpleKeyValue;
        switch (dataSource)
        {
            case EDataSource.MySql or EDataSource.Sqlite:
            case EDataSource.SqlServer:
                //对象被合并 属性被忽略
                context.CreateSet<ComplexIgnoreCombineKeyValue>().Attach(ignoreCombineKeyValue);
                context.SaveChanges();
                //查出来验证
                context = ContextUtils.CreateContext(dataSource);
                queryIgnoreSimpleKeyValue =
                    context.CreateSet<ComplexIgnoreCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
                //仍然是1
                Assert.That(queryIgnoreSimpleKeyValue, Is.Not.Null);
                Assert.That(queryIgnoreSimpleKeyValue.KeyValue.Value, Is.EqualTo(1));
                break;
            case EDataSource.PostgreSql:
                //如果是PostgreSql 因为PostgreSql不支持发生异常后继续修改对象 会抛出一个特定的异常
                Assert.Throws<UnSupportedException>(() =>
                {
                    context.CreateSet<ComplexIgnoreCombineKeyValue>().Attach(ignoreCombineKeyValue);
                    context.SaveChanges();
                });
                //查出来验证
                context = ContextUtils.CreateContext(dataSource);
                queryIgnoreSimpleKeyValue =
                    context.CreateSet<ComplexIgnoreCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
                //仍然是1
                Assert.That(queryIgnoreSimpleKeyValue, Is.Not.Null);
                Assert.That(queryIgnoreSimpleKeyValue.KeyValue.Value, Is.EqualTo(1));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, "不支持的数据库类型");
        }

        //修改
        queryIgnoreSimpleKeyValue.VersionKey = 2;
        //用就地修改方法 模拟一个版本键被其他线程修改
        context.CreateSet<ComplexIgnoreCombineKeyValue>().SetAttributes(
            [new KeyValuePair<string, object>("VersionKey", 4)],
            p => p.Id == 1);
        //会被合并 属性被忽略
        context.SaveChanges();

        //重新查出来
        context = ContextUtils.CreateContext(dataSource);
        queryIgnoreSimpleKeyValue = context.CreateSet<ComplexIgnoreCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
        //仍然是1
        Assert.That(queryIgnoreSimpleKeyValue, Is.Not.Null);
        Assert.That(queryIgnoreSimpleKeyValue.KeyValue.Value, Is.EqualTo(1));
        //清理数据 防止影响其他测试
        context.CreateSet<ComplexIgnoreCombineKeyValue>().Delete(p => p.Id > 0);
    }

    /// <summary>
    ///     测试 复杂属性 合并策略 属性覆盖处理
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ComplexAttributeOverWriteCombineConcurrentConflict(EDataSource dataSource)
    {
        //构造对象
        var overwriteCombineKeyValue = new ComplexOverwriteCombineKeyValue
        {
            Id = 1,
            KeyValue = new OverWriteCombineComplexKeyValue
            {
                Key = "Key",
                Value = 1
            },
            VersionKey = 1
        };

        //附加
        var context = ContextUtils.CreateContext(dataSource);
        context.CreateSet<ComplexOverwriteCombineKeyValue>().Attach(overwriteCombineKeyValue);
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);
        //重复插入
        overwriteCombineKeyValue = new ComplexOverwriteCombineKeyValue
        {
            Id = 1,
            KeyValue = new OverWriteCombineComplexKeyValue
            {
                Key = "Key",
                Value = 2
            },
            VersionKey = 1
        };

        ComplexOverwriteCombineKeyValue queryOverWriteKeyValue;
        switch (dataSource)
        {
            case EDataSource.MySql or EDataSource.Sqlite or EDataSource.SqlServer:
                //对象被合并 属性被覆盖
                context.CreateSet<ComplexOverwriteCombineKeyValue>().Attach(overwriteCombineKeyValue);
                context.SaveChanges();
                //重新查出来
                context = ContextUtils.CreateContext(dataSource);
                queryOverWriteKeyValue =
                    context.CreateSet<ComplexOverwriteCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
                //是新对象的2
                Assert.That(queryOverWriteKeyValue, Is.Not.Null);
                Assert.That(queryOverWriteKeyValue.KeyValue.Value, Is.EqualTo(2));
                break;
            case EDataSource.PostgreSql:
                //如果是PostgreSql 因为PostgreSql不支持发生异常后继续修改对象 会抛出一个特定的异常
                Assert.Throws<UnSupportedException>(() =>
                {
                    context.CreateSet<ComplexOverwriteCombineKeyValue>().Attach(overwriteCombineKeyValue);
                    context.SaveChanges();
                });
                //查出来验证
                context = ContextUtils.CreateContext(dataSource);
                queryOverWriteKeyValue =
                    context.CreateSet<ComplexOverwriteCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
                //是之前的1
                Assert.That(queryOverWriteKeyValue, Is.Not.Null);
                Assert.That(queryOverWriteKeyValue.KeyValue.Value, Is.EqualTo(1));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, "不支持的数据库类型");
        }

        //修改
        queryOverWriteKeyValue.VersionKey = 9;
        //用就地修改方法 模拟一个版本键被其他线程修改
        context.CreateSet<ComplexOverwriteCombineKeyValue>().SetAttributes(
            [new KeyValuePair<string, object>("VersionKey", 2)],
            p => p.Id == 1);
        //会被合并 属性被覆盖
        context.SaveChanges();

        //重新查出来
        context = ContextUtils.CreateContext(dataSource);
        queryOverWriteKeyValue = context.CreateSet<ComplexOverwriteCombineKeyValue>().FirstOrDefault(p => p.Id == 1);
        //覆盖成修改后的9
        Assert.That(queryOverWriteKeyValue, Is.Not.Null);
        Assert.That(queryOverWriteKeyValue.VersionKey, Is.EqualTo(9));
        //清理数据 防止影响其他测试
        context.CreateSet<ComplexOverwriteCombineKeyValue>().Delete(p => p.Id > 0);
    }

    /// <summary>
    ///     测试 复杂属性 忽略策略
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ComplexAttributeIngoreConcurrentConflict(EDataSource dataSource)
    {
        //构造对象
        var ignoreComplexKeyValue = new ComplexIgnoreKeyValue
        {
            Id = 1,
            KeyValue = new ComplexKeyValue
            {
                Key = "Key",
                Value = 1
            },
            VersionKey = 1
        };

        //附加
        var context = ContextUtils.CreateContext(dataSource);
        context.CreateSet<ComplexIgnoreKeyValue>().Attach(ignoreComplexKeyValue);
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);
        //重复插入 异常忽略
        ignoreComplexKeyValue = new ComplexIgnoreKeyValue
        {
            Id = 1,
            KeyValue = new ComplexKeyValue
            {
                Key = "Key",
                Value = 2
            },
            VersionKey = 1
        };

        ComplexIgnoreKeyValue queryIgnoreComplexKeyValue;
        if (dataSource is EDataSource.MySql or EDataSource.Sqlite or EDataSource.SqlServer)
        {
            //忽略策略 不修改旧数据
            context.CreateSet<ComplexIgnoreKeyValue>().Attach(ignoreComplexKeyValue);
            context.SaveChanges();
            //查出来验证
            context = ContextUtils.CreateContext(dataSource);
            queryIgnoreComplexKeyValue = context.CreateSet<ComplexIgnoreKeyValue>().FirstOrDefault(p => p.Id == 1);
            //还是之前的1
            Assert.That(queryIgnoreComplexKeyValue, Is.Not.Null);
            Assert.That(queryIgnoreComplexKeyValue.KeyValue.Value, Is.EqualTo(1));
        }
        else
        {
            //如果是PostgreSql 因为PostgreSql不支持发生异常后继续修改对象 会抛出一个特定的异常
            Assert.Throws<UnSupportedException>(() =>
            {
                context.CreateSet<ComplexIgnoreKeyValue>().Attach(ignoreComplexKeyValue);
                context.SaveChanges();
            });
            //查出来验证
            context = ContextUtils.CreateContext(dataSource);
            queryIgnoreComplexKeyValue = context.CreateSet<ComplexIgnoreKeyValue>().FirstOrDefault(p => p.Id == 1);
            //还是之前的1
            Assert.That(queryIgnoreComplexKeyValue, Is.Not.Null);
            Assert.That(queryIgnoreComplexKeyValue.KeyValue.Value, Is.EqualTo(1));
        }

        //修改
        queryIgnoreComplexKeyValue.KeyValue = new ComplexKeyValue { Key = "Key", Value = 2 };
        queryIgnoreComplexKeyValue.VersionKey = 1;
        //模拟一个版本键被修改
        context.CreateSet<ComplexIgnoreKeyValue>().SetAttributes([new KeyValuePair<string, object>("VersionKey", 2)],
            p => p.Id == 1);
        //异常被忽略
        context.SaveChanges();

        //重新查出来
        context = ContextUtils.CreateContext(dataSource);
        queryIgnoreComplexKeyValue = context.CreateSet<ComplexIgnoreKeyValue>().FirstOrDefault(p => p.Id == 1);
        //被忽略 实际数据未改动 还是1
        Assert.That(queryIgnoreComplexKeyValue, Is.Not.Null);
        Assert.That(queryIgnoreComplexKeyValue.KeyValue.Value, Is.EqualTo(1));

        //清理数据  防止影响其他测试
        context.CreateSet<ComplexIgnoreKeyValue>().Delete(p => p.Id > 0);
    }

    /// <summary>
    ///     测试 复杂属性 覆盖策略
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ComplexAttributeOverWriteConcurrentConflict(EDataSource dataSource)
    {
        //构造对象
        var overWriteKeyValue = new ComplexOverwriteKeyValue
        {
            Id = 1,
            KeyValue = new ComplexKeyValue
            {
                Key = "Key",
                Value = 1
            },
            VersionKey = 1
        };

        //附加
        var context = ContextUtils.CreateContext(dataSource);
        context.CreateSet<ComplexOverwriteKeyValue>().Attach(overWriteKeyValue);
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);
        //重复插入
        overWriteKeyValue = new ComplexOverwriteKeyValue
        {
            Id = 1,
            KeyValue = new ComplexKeyValue
            {
                Key = "Key",
                Value = 2
            },
            VersionKey = 1
        };

        ComplexOverwriteKeyValue queryOverWriteKeyValue;
        switch (dataSource)
        {
            case EDataSource.MySql or EDataSource.Sqlite or EDataSource.SqlServer:
                //对象被覆盖
                context.CreateSet<ComplexOverwriteKeyValue>().Attach(overWriteKeyValue);
                context.SaveChanges();
                //查出来验证
                context = ContextUtils.CreateContext(dataSource);
                queryOverWriteKeyValue = context.CreateSet<ComplexOverwriteKeyValue>().FirstOrDefault(p => p.Id == 1);
                //被覆盖成2
                Assert.That(queryOverWriteKeyValue, Is.Not.Null);
                Assert.That(queryOverWriteKeyValue.KeyValue.Value, Is.EqualTo(2));
                break;
            case EDataSource.PostgreSql:
                //如果是PostgreSql 因为PostgreSql不支持发生异常后继续修改对象 会抛出一个特定的异常
                Assert.Throws<UnSupportedException>(() =>
                {
                    context.CreateSet<ComplexOverwriteKeyValue>().Attach(overWriteKeyValue);
                    context.SaveChanges();
                });
                //查出来验证
                context = ContextUtils.CreateContext(dataSource);
                queryOverWriteKeyValue = context.CreateSet<ComplexOverwriteKeyValue>().FirstOrDefault(p => p.Id == 1);
                //还是1 没改动
                Assert.That(queryOverWriteKeyValue, Is.Not.Null);
                Assert.That(queryOverWriteKeyValue.KeyValue.Value, Is.EqualTo(1));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, "不支持的数据库类型");
        }

        //修改
        queryOverWriteKeyValue.VersionKey = 3;
        //用就地修改方法 模拟一个版本键被其他线程修改
        context.CreateSet<ComplexOverwriteKeyValue>().SetAttributes([new KeyValuePair<string, object>("VersionKey", 2)],
            p => p.Id == 1);
        //会被覆盖
        context.SaveChanges();

        //重新查出来
        context = ContextUtils.CreateContext(dataSource);
        queryOverWriteKeyValue = context.CreateSet<ComplexOverwriteKeyValue>().FirstOrDefault(p => p.Id == 1);
        //覆盖成3
        Assert.That(queryOverWriteKeyValue, Is.Not.Null);
        Assert.That(queryOverWriteKeyValue.VersionKey, Is.EqualTo(3));
        //清理数据  防止影响其他测试
        context.CreateSet<ComplexOverwriteKeyValue>().Delete(p => p.Id > 0);
    }

    /// <summary>
    ///     测试 复杂属性 重建策略
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ComplexAttributeReconstructConcurrentConflict(EDataSource dataSource)
    {
        //构造对象
        var reconstructeKeyValue = new ComplexReconstructKeyValue
        {
            Id = 1,
            KeyValue = new ComplexKeyValue
            {
                Key = "Key",
                Value = 1
            },
            VersionKey = 1
        };

        //附加
        var context = ContextUtils.CreateContext(dataSource);
        context.CreateSet<ComplexReconstructKeyValue>().Attach(reconstructeKeyValue);
        context.SaveChanges();
        //重建对象策略只处理修改时并发
        var queryReconstructKeyValue = context.CreateSet<ComplexReconstructKeyValue>().FirstOrDefault(p => p.Id == 1);
        //取出对象
        Assert.That(queryReconstructKeyValue, Is.Not.Null);

        //修改
        queryReconstructKeyValue.KeyValue = new ComplexKeyValue { Key = "Key", Value = 4 };
        queryReconstructKeyValue.VersionKey = 4;
        //用就地修改方法 模拟一个主键被修改 相当于此对象不存在了
        context.CreateSet<ComplexReconstructKeyValue>().SetAttributes(
            [new KeyValuePair<string, object>("Id", 2)],
            p => p.Id == 1);
        //会重建新对象
        context.SaveChanges();
        //查询验证
        context = ContextUtils.CreateContext(dataSource);
        queryReconstructKeyValue = context.CreateSet<ComplexReconstructKeyValue>().FirstOrDefault(p => p.Id == 1);
        //新增了一个ID是1 Value是4的对象
        Assert.That(queryReconstructKeyValue, Is.Not.Null);
        Assert.That(queryReconstructKeyValue.KeyValue.Value, Is.EqualTo(4));

        //原对象
        queryReconstructKeyValue = context.CreateSet<ComplexReconstructKeyValue>().FirstOrDefault(p => p.Id == 2);
        //存在
        Assert.That(queryReconstructKeyValue, Is.Not.Null);
        Assert.That(queryReconstructKeyValue.KeyValue.Value, Is.EqualTo(1));
        //清理数据  防止影响其他测试
        context.CreateSet<ComplexReconstructKeyValue>().Delete(p => p.Id > 0);
    }

    /// <summary>
    ///     测试 复杂属性 抛出异常策略
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ComplexAttributeThrowExceptionConcurrentConflict(EDataSource dataSource)
    {
        //构造对象
        var throwExceptionComplexKeyValue = new ComplexThrowExceptionKeyValue
        {
            Id = 1,
            KeyValue = new ComplexKeyValue
            {
                Key = "Key",
                Value = 2
            },
            VersionKey = 1
        };

        //附加
        var context = ContextUtils.CreateContext(dataSource);
        context.CreateSet<ComplexThrowExceptionKeyValue>().Attach(throwExceptionComplexKeyValue);
        context.SaveChanges();

        //重复插入 异常会被抛出
        Assert.Throws<RepeatCreationException>(() =>
        {
            context = ContextUtils.CreateContext(dataSource);

            throwExceptionComplexKeyValue = new ComplexThrowExceptionKeyValue
            {
                Id = 1,
                KeyValue = new ComplexKeyValue
                {
                    Key = "Key",
                    Value = 1
                },
                VersionKey = 1
            };
            //异常被抛出
            context.CreateSet<ComplexThrowExceptionKeyValue>().Attach(throwExceptionComplexKeyValue);
            context.SaveChanges();
        });

        //版本键被修改 异常会被抛出
        Assert.Throws<VersionConflictException>(() =>
        {
            context = ContextUtils.CreateContext(dataSource);
            var queryThrowExceptionKeyValue =
                context.CreateSet<ComplexThrowExceptionKeyValue>().FirstOrDefault(p => p.Id == 1);
            //旧对象存在
            Assert.That(queryThrowExceptionKeyValue, Is.Not.Null);

            //修改
            queryThrowExceptionKeyValue.VersionKey = 2;

            //用就地修改方法 模拟一个版本键被其他现场修改
            context.CreateSet<ComplexThrowExceptionKeyValue>().SetAttributes(
                [new KeyValuePair<string, object>("VersionKey", 3)],
                p => p.Id == 1);

            //异常被抛出
            context.SaveChanges();
        });

        //清理数据 防止影响其他测试
        context.CreateSet<ComplexThrowExceptionKeyValue>().Delete(p => p.Id > 0);
    }
}