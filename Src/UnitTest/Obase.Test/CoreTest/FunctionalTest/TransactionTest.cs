using System;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Npgsql;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.SimpleType;
using Obase.Test.Infrastructure.Configuration;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     事务测试
/// </summary>
[TestFixture]
public class TransactionTest
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
            //清理可能的冗余数据
            context.CreateSet<NullableJavaBean>().Delete(p => p.IntNumber > 0);

            //添加新对象
            for (var i = 1; i < 11; i++)
                context.CreateSet<NullableJavaBean>().Attach(new NullableJavaBean
                {
                    Bool = i % 2 == 0,
                    DateTime = DateTime.Now,
                    DecimalNumber = new decimal(Math.Pow(Math.PI, i)),
                    IntNumber = i,
                    LongNumber = i,
                    ByteNumber = (byte)i,
                    CharNumber = '\u006A',
                    FloatNumber = (float)Math.Pow(Math.PI, i),
                    DoubleNumber = (float)Math.Pow(Math.PI, i),
                    Time = new TimeSpan(0, 8, 40, 0),
                    Date = DateTime.Now,
                    Guid = Guid.NewGuid()
                });

            context.SaveChanges();
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
            //清理可能的冗余数据
            context.CreateSet<NullableJavaBean>().Delete(p => p.IntNumber > 0);
        }
    }

    /// <summary>
    ///     自动事务测试
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void AutoTransactionTest(EDataSource dataSource)
    {
        //自动事务 指的是Obase自动附加的事务
        //在Obase中 每次SaveChanges时会把此次保存和上次保存之间的所有上下文管理的对象修改操作包含在一个事务块内
        //如果是首次SaveChanges则是从构造上下文开始的所有上下文管理的对象修改
        //此处的上下文管理的对象指的是附加到上下文的新对象和由上下文查询得到的旧对象

        var context = ContextUtils.CreateContext(dataSource);

        //此时 有10个对象 都查出来
        var list = context.CreateSet<NullableJavaBean>().OrderBy(p => p.IntNumber).ToList();
        //10个
        Assert.That(list, Is.Not.Null);
        Assert.That(list.Count, Is.EqualTo(10));

        //此时修改其中的首个的Guid属性
        list[0].Guid = new Guid();
        list[1].Guid = new Guid();
        list[2].Guid = new Guid();

        //保存 这三个都被修改
        context.SaveChanges();

        var emptyGuid = new Guid().ToString();
        context = ContextUtils.CreateContext(dataSource);
        list = context.CreateSet<NullableJavaBean>().OrderBy(p => p.IntNumber).ToList();
        //10个
        Assert.That(list, Is.Not.Null);
        Assert.That(list.Count, Is.EqualTo(10));
        //前三个被修改
        Assert.That(list[0].Guid.ToString(), Is.EqualTo(emptyGuid));
        Assert.That(list[1].Guid.ToString(), Is.EqualTo(emptyGuid));
        Assert.That(list[2].Guid.ToString(), Is.EqualTo(emptyGuid));

        //此时模拟一个修改失败 第二个数值会超过数据库的限制 所以这三个修改都没有被保存
        list[0].IntNumber = 11;
        list[1].IntNumber = null;
        list[2].IntNumber = 12;

        try
        {
            context.SaveChanges();
        }
        catch (Exception)
        {
            // 忽略掉异常
        }

        context = ContextUtils.CreateContext(dataSource);
        list = context.CreateSet<NullableJavaBean>().OrderBy(p => p.IntNumber).ToList();
        //10个
        Assert.That(list, Is.Not.Null);
        Assert.That(list.Count, Is.EqualTo(10));
        //前三值没有变化
        Assert.That(list[0].IntNumber, Is.EqualTo(1));
        Assert.That(list[1].IntNumber, Is.EqualTo(2));
        Assert.That(list[2].IntNumber, Is.EqualTo(3));
    }

    /// <summary>
    ///     手动事务测试
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ManualTransactionTest(EDataSource dataSource)
    {
        //手动事务 指的是调用Obase的手动事务方法自己控制事务
        //Obase的手动事务方法遵循ADO.NET的try-Begin-Commit-Catch-RollBack-Finally-Release模式

        var context = ContextUtils.CreateContext(dataSource);

        //此时 有10个对象 都查出来
        var list = context.CreateSet<NullableJavaBean>().OrderBy(p => p.IntNumber).ToList();
        //10个
        Assert.That(list, Is.Not.Null);
        Assert.That(list.Count, Is.EqualTo(10));

        try
        {
            //手动开启事务
            context.BeginTransaction();

            //在这个事务里还可查询其他对象
            var emptyList = context.CreateSet<NullableJavaBean>().Where(p => p.IntNumber > 20).ToList();
            //没有满足条件的
            Assert.That(emptyList, Is.Not.Null);
            Assert.That(emptyList.Count, Is.EqualTo(0));

            //修改前三个的LongNumber
            list[0].LongNumber = 11;
            list[1].LongNumber = 12;
            list[2].LongNumber = 13;
            //保存之前的修改
            context.SaveChanges();
            //调用模拟的外部方法 此处传入的是偶数 不会抛异常
            OuterMethod(2);

            //提交修改
            context.Commit();
        }
        catch (Exception)
        {
            //发生异常 回滚
            context.RollbackTransaction();
        }
        finally
        {
            //最后释放资源
            context.Release();
        }

        context = ContextUtils.CreateContext(dataSource);
        list = context.CreateSet<NullableJavaBean>().OrderBy(p => p.IntNumber).ToList();
        //10个
        Assert.That(list, Is.Not.Null);
        Assert.That(list.Count, Is.EqualTo(10));

        //前三个被修改
        Assert.That(list[0].LongNumber, Is.EqualTo(11));
        Assert.That(list[1].LongNumber, Is.EqualTo(12));
        Assert.That(list[2].LongNumber, Is.EqualTo(13));

        try
        {
            //手动开启事务
            context.BeginTransaction();

            //在这个事务里还可查询其他对象
            var emptyList = context.CreateSet<NullableJavaBean>().Where(p => p.IntNumber > 20).ToList();
            //没有满足条件的
            Assert.That(emptyList, Is.Not.Null);
            Assert.That(emptyList.Count, Is.EqualTo(0));

            //修改前三个的LongNumber
            list[0].LongNumber = 14;
            list[1].LongNumber = 15;
            list[2].LongNumber = 16;
            //保存之前的修改
            context.SaveChanges();
            //调用模拟的外部方法 此处传入的是奇数 会抛异常
            OuterMethod(1);

            //提交修改
            context.Commit();
        }
        catch (Exception)
        {
            //发生异常 回滚
            context.RollbackTransaction();
        }
        finally
        {
            //最后释放资源
            context.Release();
        }

        context = ContextUtils.CreateContext(dataSource);
        list = context.CreateSet<NullableJavaBean>().OrderBy(p => p.IntNumber).ToList();
        //10个
        Assert.That(list, Is.Not.Null);
        Assert.That(list.Count, Is.EqualTo(10));

        //前三个没有被修改
        Assert.That(list[0].LongNumber, Is.EqualTo(11));
        Assert.That(list[1].LongNumber, Is.EqualTo(12));
        Assert.That(list[2].LongNumber, Is.EqualTo(13));
    }

    /// <summary>
    ///     模拟的外部方法
    ///     当传入的i为奇数时会抛异常
    /// </summary>
    /// <param name="i">int</param>
    private void OuterMethod(int i)
    {
        if (i % 2 == 1)
            throw new ArgumentException("只允许偶数!");
    }

    /// <summary>
    ///     已存在连接的事务测试
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ExistingConnectionTransactionTest(EDataSource dataSource)
    {
        //测试和连接提供方使用同一个事物一起提交的情形
        //模拟获取一个连接 此连接实际由连接提供方负责管理
        var connection = GetDbProviderFactory(dataSource).CreateConnection();
        if (connection == null)
            Assert.Fail("无法获取连接.");
        //设置连接字符串
        connection.ConnectionString = GetConnectionString(dataSource);
        //打开连接
        connection.Open();

        //构造一个插入语句 并且开启事务 模拟是连接提供方的逻辑
        var transaction = connection.BeginTransaction();
        var sqlCommand = connection.CreateCommand();
        sqlCommand.CommandText = dataSource == EDataSource.PostgreSql
            ? "INSERT INTO \"NullableJavaBean\" (\"IntNumber\") VALUES(21)"
            : "INSERT INTO NullableJavaBean (IntNumber) VALUES(21)";
        sqlCommand.Transaction = transaction;

        //普通的上下文 实质上是一个数据源 此上下文用于验证数据
        var context = ContextUtils.CreateContext(dataSource);
        //当前没有插入
        var count = context.CreateSet<NullableJavaBean>().Count(p => p.IntNumber == 21 || p.IntNumber == 22);
        Assert.That(count, Is.EqualTo(0));

        //此下的所有开启事务 提交事务 回滚事务 关闭连接在实际使用时都是由连接提供方负责
        try
        {
            //执行Sql 模拟是连接提供方的逻辑
            sqlCommand.ExecuteNonQuery();

            //创建已有连接的上下文 此处构造用的连接是提供方的
            //因为想要和提供方使用同一个事务 所以此处还需要传入事务对象
            var exContext = ContextUtils.CreateExistingConnectionContext(connection, dataSource, transaction);
            //在这里执行Obase的逻辑
            exContext.CreateSet<NullableJavaBean>().Attach(new NullableJavaBean
            {
                IntNumber = 22
            });
            //Obase保存
            exContext.SaveChanges();

            //连接提供方的提交事务
            transaction.Commit();
            sqlCommand.Dispose();
        }
        catch (Exception e)
        {
            //连接提供方的回滚事务
            transaction.Rollback();
            Assert.Fail($"已存在连接的事务发生异常:{e.Message}.");
        }
        finally
        {
            //连接提供方的关闭连接
            connection.Close();
            connection.Dispose();
        }

        //当前插入了两条数据
        count = context.CreateSet<NullableJavaBean>().Count(p => p.IntNumber == 21 || p.IntNumber == 22);
        Assert.That(count, Is.EqualTo(2));

        //测试和连接提供方使用同一个事物一起回滚的情形
        //模拟获取一个连接 此连接实际由连接提供方负责管理
        connection = GetDbProviderFactory(dataSource).CreateConnection();
        if (connection == null)
            Assert.Fail("无法获取连接.");
        //设置连接字符串
        connection.ConnectionString = GetConnectionString(dataSource);
        //打开连接
        connection.Open();

        //构造一个插入语句 并且开启事务 模拟是连接提供方的逻辑
        transaction = connection.BeginTransaction();
        sqlCommand = connection.CreateCommand();
        sqlCommand.CommandText = dataSource == EDataSource.PostgreSql
            ? "INSERT INTO \"NullableJavaBean\" (\"IntNumber\") VALUES(23)"
            : "INSERT INTO NullableJavaBean (IntNumber) VALUES(23)";
        sqlCommand.Transaction = transaction;

        //此下的所有开启事务 回滚事务 关闭连接在实际使用时都是由连接提供方负责
        try
        {
            //执行Sql 模拟是连接提供方的逻辑
            sqlCommand.ExecuteNonQuery();

            //创建已有连接的上下文 此处构造用的连接是提供方的
            //因为想要和提供方使用同一个事务 所以此处还需要传入事务对象
            var exContext = ContextUtils.CreateExistingConnectionContext(connection, dataSource, transaction);
            //在这里执行Obase的逻辑
            exContext.CreateSet<NullableJavaBean>().Attach(new NullableJavaBean
            {
                IntNumber = 24
            });
            //Obase保存
            exContext.SaveChanges();

            //连接提供方的回滚事务
            transaction.Rollback();
            sqlCommand.Dispose();
        }
        catch (Exception e)
        {
            Assert.Fail($"已存在连接的事务发生异常:{e.Message}.");
        }
        finally
        {
            //连接提供方的关闭连接
            connection.Close();
            connection.Dispose();
        }

        //都回滚了 没有插入数据
        count = context.CreateSet<NullableJavaBean>().Count(p => p.IntNumber == 23 || p.IntNumber == 24);
        Assert.That(count, Is.EqualTo(0));

        //测试连接提供方不提供事务的情形
        connection = GetDbProviderFactory(dataSource).CreateConnection();
        if (connection == null)
            Assert.Fail("无法获取连接.");
        //设置连接字符串
        connection.ConnectionString = GetConnectionString(dataSource);
        //此时就是没有事务的 就直接创建命令 打开连接
        sqlCommand = connection.CreateCommand();
        sqlCommand.CommandText = dataSource == EDataSource.PostgreSql
            ? "INSERT INTO \"NullableJavaBean\" (\"IntNumber\") VALUES(25)"
            : "INSERT INTO NullableJavaBean (IntNumber) VALUES(25)";
        //打开连接
        connection.Open();
        //执行Sql 模拟是连接提供方的逻辑
        sqlCommand.ExecuteNonQuery();

        //创建已有连接的上下文 此处构造用的连接是提供方的
        //没有事务 不需要传事务对象
        var exContextNoTransation = ContextUtils.CreateExistingConnectionContext(connection, dataSource);
        //在这里执行Obase的逻辑
        exContextNoTransation.CreateSet<NullableJavaBean>().Attach(new NullableJavaBean
        {
            IntNumber = 26
        });
        //Obase保存
        exContextNoTransation.SaveChanges();

        //连接提供方的关闭连接
        connection.Close();
        connection.Dispose();

        //普通的插入了两条数据
        count = context.CreateSet<NullableJavaBean>().Count(p => p.IntNumber == 25 || p.IntNumber == 26);
        Assert.That(count, Is.EqualTo(2));

        //清理一下数据 防止污染其它的测试
        context.CreateSet<NullableJavaBean>().Delete(p => p.IntNumber > 20);
    }

    /// <summary>
    ///     获取数据库客户端工厂
    /// </summary>
    /// <param name="dataSource">数据源类型</param>
    /// <returns></returns>
    private DbProviderFactory GetDbProviderFactory(EDataSource dataSource)
    {
        switch (dataSource)
        {
            case EDataSource.SqlServer:
                return SqlClientFactory.Instance;
            case EDataSource.MySql:
                return MySqlClientFactory.Instance;
            case EDataSource.Sqlite:
                return SqliteFactory.Instance;
            case EDataSource.PostgreSql:
                return NpgsqlFactory.Instance;
            case EDataSource.Oracle:
            case EDataSource.Oledb:
            case EDataSource.Other:
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, $"暂无{dataSource}对应的数据库客户端工厂.");
        }
    }

    /// <summary>
    ///     获取数据源的连接字符串
    /// </summary>
    /// <param name="dataSource">数据源类型</param>
    /// <returns></returns>
    private string GetConnectionString(EDataSource dataSource)
    {
        switch (dataSource)
        {
            case EDataSource.SqlServer:
                return RelationshipDataBaseConfigurationManager.SqlServerConnectionString;
            case EDataSource.MySql:
                return RelationshipDataBaseConfigurationManager.MySqlConnectionString;
            case EDataSource.Sqlite:
                return RelationshipDataBaseConfigurationManager.SqliteConnectionString;
            case EDataSource.PostgreSql:
                return RelationshipDataBaseConfigurationManager.PostgreSqlConnectionString;
            case EDataSource.Oracle:
            case EDataSource.Oledb:
            case EDataSource.Other:
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, $"暂无{dataSource}对应的数据库连接字符串.");
        }
    }
}