using System;
using System.Linq;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.SimpleType;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     测试旧对象主动附加
/// </summary>
[TestFixture]
public class OldObjectAttachTest
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
            context.CreateSet<JavaBean>().Delete(p => p.IntNumber > 0);

            //添加新对象
            for (var i = 1; i < 21; i++)
                context.Attach(new JavaBean
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
    ///     销毁
    /// </summary>
    [OneTimeTearDown]
    public void Dispose()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //销毁所有旧对象
            context.CreateSet<JavaBean>().Delete(p => p.IntNumber > 0);
        }
    }

    /// <summary>
    ///     测试方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Test(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //一个新的对象
        var newObj = new JavaBean
        {
            Bool = false,
            DateTime = DateTime.Now,
            DecimalNumber = (decimal)Math.Pow(Math.PI, -20),
            LongNumber = 21,
            ByteNumber = 21,
            CharNumber = '\u006A',
            FloatNumber = (float)Math.Pow(Math.PI, -20),
            DoubleNumber = (float)Math.Pow(Math.PI, -20),
            Date = DateTime.Now,
            Time = new TimeSpan(0, 8, 40, 0),
            IntNumber = 21,
            String = "21号字符串",
            Guid = Guid.NewGuid(),
            Strings = new[] { "20", "21", "22" }
        };
        //一个旧的对象
        var oldObj = context.CreateSet<JavaBean>().Last();
        //都附加
        context.Attach(newObj);
        context.Attach(oldObj);
        //保存
        context.SaveChanges();
        //因为旧对象已经存在，所以不会插入新的对象
        var count = context.CreateSet<JavaBean>().Count();
        //只新增了一个
        Assert.That(count, Is.EqualTo(21));
    }
}