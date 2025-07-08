using System;
using System.Linq;
using Obase.AddonTest.Domain.Annotation;
using Obase.Providers.Sql;
using Obase.Test.Configuration;

namespace Obase.Test.AddonTest.AnnotationTest;

/// <summary>
///     标注建模简单类型但包含自定义属性测试
/// </summary>
[TestFixture]
public class AnnotationSimpleTypeWithCustomAttributesTest
{
    /// <summary>
    ///     构造实例 为上下文赋值
    /// </summary>
    [OneTimeSetUp]
    public void SetUp()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateAddonContext(dataSource);

            //销毁所有对象
            context.CreateSet<AnnotationJavaBeanWithCustomAttribute>().Delete(p => p.IntNumber > 0);

            //添加新对象
            for (var i = 1; i < 21; i++)
                context.Attach(new AnnotationJavaBeanWithCustomAttribute(i, Math.Pow(Math.PI, i), DateTime.Now,
                    $"{i}号字符串", i % 2 == 0, [$"{i - 1}", $"{i}", $"{i + 1}"]));

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
            var context = ContextUtils.CreateAddonContext(dataSource);
            //销毁所有对象
            context.CreateSet<AnnotationJavaBeanWithCustomAttribute>().Delete(p => p.IntNumber > 0);
        }
    }

    /// <summary>
    ///     简单查询
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void QueryTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateAddonContext(dataSource);

        //无条件查询
        var list = context.CreateSet<AnnotationJavaBeanWithCustomAttribute>().ToList();

        //有20个
        Assert.That(list.Count, Is.EqualTo(20));
    }
}