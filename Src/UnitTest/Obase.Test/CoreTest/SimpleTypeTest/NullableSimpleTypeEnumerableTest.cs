using System;
using System.Linq;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.SimpleType;

namespace Obase.Test.CoreTest.SimpleTypeTest;

/// <summary>
///     测试可空类型的简单类型
/// </summary>
[TestFixture]
public class NullableSimpleTypeEnumerableTest
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

            //销毁所有旧对象
            context.CreateSet<NullableJavaBean>().Delete(p => p.IntNumber > 0);

            //添加新对象
            for (var i = 1; i < 21; i++)
                context.Attach(
                    new NullableJavaBean
                    {
                        Bool = i % 2 == 0,
                        DateTime = DateTime.Now,
                        DecimalNumber = new decimal(Math.Pow(Math.PI, i)),
                        IntNumber = i,
                        LongNumber = i,
                        ByteNumber = (byte)i,
                        Guid = Guid.NewGuid(),
                        CharNumber = '\u006A',
                        FloatNumber = (float)Math.Pow(Math.PI, i),
                        DoubleNumber = (float)Math.Pow(Math.PI, i),
                        Time = new TimeSpan(0, 8, 40, 0),
                        Date = DateTime.Now
                    });
            //添加一个各项为空的
            context.Attach(
                new NullableJavaBean
                {
                    Bool = null,
                    DateTime = null,
                    DecimalNumber = null,
                    IntNumber = 21,
                    LongNumber = null,
                    ByteNumber = null,
                    Guid = null,
                    CharNumber = null,
                    FloatNumber = null,
                    DoubleNumber = null,
                    Time = null,
                    Date = null
                });

            context.SaveChanges();
        }
    }

    /// <summary>
    ///     销毁
    /// </summary>
    [OneTimeTearDown]
    public void Dispose()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //销毁所有旧对象
            context.CreateSet<NullableJavaBean>().Delete(p => p.IntNumber > 0);
        }
    }

    /// <summary>
    ///     简单查询
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void QueryTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //无条件查询
        var list = context.CreateSet<NullableJavaBean>().ToList();

        //有21个
        Assert.That(list.Count, Is.EqualTo(21));
        //第一个有值
        var first = list[0].IntNumber;
        Assert.That(first, Is.Not.Null);
        Assert.That(first.Value, Is.EqualTo(1));
        //最后一个没有值
        var last = list[20].DateTime;
        Assert.That(last, Is.Null);
    }
}