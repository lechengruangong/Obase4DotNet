using System;
using System.Linq;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.SimpleType;

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

            //修改前三个的LongNumber
            list[0].LongNumber = 11;
            list[1].LongNumber = 12;
            list[2].LongNumber = 13;
            //调用模拟的外部方法 此处传入的是偶数 不会抛异常
            OuterMethod(2);

            //在这个事务里还可查询其他对象
            var emptyList = context.CreateSet<NullableJavaBean>().Where(p => p.IntNumber > 20).ToList();
            //没有满足条件的
            Assert.That(emptyList, Is.Not.Null);
            Assert.That(emptyList.Count, Is.EqualTo(0));
            //保存之前的修改
            context.SaveChanges();
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

            //修改前三个的LongNumber
            list[0].LongNumber = 14;
            list[1].LongNumber = 15;
            list[2].LongNumber = 16;
            //调用模拟的外部方法 此处传入的是奇数 会抛异常
            OuterMethod(1);

            //在这个事务里还可查询其他对象
            var emptyList = context.CreateSet<NullableJavaBean>().Where(p => p.IntNumber > 20).ToList();
            //没有满足条件的
            Assert.That(emptyList, Is.Not.Null);
            Assert.That(emptyList.Count, Is.EqualTo(0));
            //保存之前的修改
            context.SaveChanges();
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
}