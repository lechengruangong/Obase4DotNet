using System;
using System.Linq;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.SimpleType;

namespace Obase.Test.CoreTest.SimpleTypeTest;

/// <summary>
///     测试只有带参构造的简单类型
/// </summary>
[TestFixture]
public class SimpleTypeWithConstructorArgsEnumerableTest
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
            context.CreateSet<JavaBeanWithConstructorArgs>().Delete(p => p.IntNumber > 0);

            //添加新对象
            for (var i = 1; i < 21; i++)
                context.Attach(
                    new JavaBeanWithConstructorArgs(i, i, (byte)i, '\u006A', (float)Math.Pow(Math.PI, i),
                        Math.Pow(Math.PI, i),
                        new decimal(Math.Pow(Math.PI, i)), DateTime.Now, new TimeSpan(0, 8, 40, 0), DateTime.Now,
                        $"{i}号字符串", i % 2 == 0, new[] { $"{i - 1}", $"{i}", $"{i + 1}" }));

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
            context.CreateSet<JavaBeanWithConstructorArgs>().Delete(p => p.IntNumber > 0);
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
        var list = context.CreateSet<JavaBeanWithConstructorArgs>().ToList();

        //有20个
        Assert.That(list.Count, Is.EqualTo(20));
    }
}