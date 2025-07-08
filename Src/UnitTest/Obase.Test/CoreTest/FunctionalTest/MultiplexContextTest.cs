using System;
using System.Linq;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.SimpleType;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     复用上下文测试
/// </summary>
[TestFixture]
public class MultiplexContextTest
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
            context.CreateSet<JavaBean>().Delete(p => p.IntNumber > 0);

            //添加新对象
            for (var i = 1; i < 21; i++)
                context.CreateSet<JavaBean>().Attach(new JavaBean
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
                    String = $"{i}号字符串",
                    Guid = Guid.NewGuid(),
                    Strings = new[] { $"{i - 1}", $"{i}", $"{i + 1}" }
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
            context.CreateSet<JavaBean>().Delete(p => p.IntNumber > 0);
        }
    }

    /// <summary>
    ///     测试复用上下文
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void MultiplexTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //随意查询一部分
        var querySet = context.CreateSet<JavaBean>().Where(p => !p.Bool).ToList();

        Assert.That(querySet, Is.Not.Null);
        Assert.That(querySet.Count, Is.EqualTo(10));

        //修改部分数据
        for (var i = 0; i < querySet.Count; i++)
            if (i % 2 == 0)
                querySet[i].String = $"{querySet[i].String}_{i++}";

        context.SaveChanges();
        //查出另外一部分数据 修改
        querySet = context.CreateSet<JavaBean>().Where(p => !p.Bool && p.String.EndsWith("2")).ToList();
        Assert.That(querySet, Is.Not.Null);
        Assert.That(querySet.Count, Is.EqualTo(1));

        foreach (var javaBean in querySet) javaBean.DecimalNumber++;

        context.SaveChanges();
        //查询所有数据
        querySet = context.CreateSet<JavaBean>().ToList();
        Assert.That(querySet, Is.Not.Null);
        Assert.That(querySet.Count, Is.EqualTo(20));
    }
}