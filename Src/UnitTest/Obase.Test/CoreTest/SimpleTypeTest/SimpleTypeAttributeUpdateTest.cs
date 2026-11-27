using System;
using System.Collections.Generic;
using System.Linq;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.SimpleType;

namespace Obase.Test.CoreTest.SimpleTypeTest;

/// <summary>
///     JavaBean 与 NullableJavaBean 的属性即时更新的集成测试
///     覆盖在真实数据源上按属性名对属性做增量更新与设值更新
/// </summary>
[TestFixture]
public class SimpleTypeAttributeUpdateTest
{
    /// <summary>
    ///     测试用的主键值
    /// </summary>
    private const int Key = 1;

    /// <summary>
    ///     每个测试方法执行前重建数据
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);

            //销毁可能存在的旧对象
            context.CreateSet<JavaBean>().Delete(p => p.IntNumber == Key);
            context.CreateSet<NullableJavaBean>().Delete(p => p.IntNumber == Key);

            //添加新对象
            context.Attach(new JavaBean
            {
                Bool = false,
                DateTime = DateTime.Now,
                DecimalNumber = 1.5m,
                FieldMappingNumber = 10,
                IntNumber = Key,
                LongNumber = 100L,
                ByteNumber = 1,
                Guid = Guid.NewGuid(),
                CharNumber = '\u006A',
                FloatNumber = 1.5F,
                DoubleNumber = 1.5D,
                Time = new TimeSpan(0, 8, 40, 0),
                Date = DateTime.Now,
                String = "初始字符串",
                Strings = new[] { "A" },
                Numbers = new List<long> { 1L },
                NullableNumbers = new List<long?> { 1L }
            });
            context.Attach(new NullableJavaBean
            {
                IntNumber = Key,
                LongNumber = 7L,
                DoubleNumber = 1.5D,
                DecimalNumber = 1.5m
            });

            context.SaveChanges();
        }
    }

    /// <summary>
    ///     每个测试方法执行后清理数据
    /// </summary>
    [TearDown]
    public void Dispose()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            context.CreateSet<JavaBean>().Delete(p => p.IntNumber == Key);
            context.CreateSet<NullableJavaBean>().Delete(p => p.IntNumber == Key);
        }
    }

    /// <summary>
    ///     测试为属性设置原值加上增量值
    ///     同时覆盖属性名为映射字段名的情形 以及不存在的属性名
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void IncreaseAttributes(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //多个属性同时增量 键为属性名称
        var affected = context.CreateSet<JavaBean>().IncreaseAttributes(new[]
        {
            new KeyValuePair<string, object>(nameof(JavaBean.LongNumber), 5L),
            new KeyValuePair<string, object>(nameof(JavaBean.DoubleNumber), 0.5D)
        }, p => p.IntNumber == Key);
        Assert.That(affected, Is.EqualTo(1));

        //键也可以是映射字段名
        var fieldAffected = context.CreateSet<JavaBean>().IncreaseAttributes(new[]
        {
            new KeyValuePair<string, object>("field_mapping_number", 5)
        }, p => p.IntNumber == Key);
        Assert.That(fieldAffected, Is.EqualTo(1));

        //可空值类型的属性同样支持增量
        var nullableAffected = context.CreateSet<NullableJavaBean>().IncreaseAttributes(new[]
        {
            new KeyValuePair<string, object>(nameof(NullableJavaBean.LongNumber), 3L)
        }, p => p.IntNumber == Key);
        Assert.That(nullableAffected, Is.EqualTo(1));

        //不存在的属性名 应抛出参数异常
        var exception = Assert.Throws<ArgumentException>(() => context.CreateSet<JavaBean>()
            .IncreaseAttributes([new KeyValuePair<string, object>("NoSuchProperty", 1)],
                p => p.IntNumber == Key));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Does.Contain("无法与"));

        //从新的上下文读取 确认已写入数据库
        var readContext = ContextUtils.CreateContext(dataSource);
        var javaBean = readContext.CreateSet<JavaBean>().FirstOrDefault(p => p.IntNumber == Key);
        Assert.That(javaBean, Is.Not.Null);
        Assert.That(javaBean.LongNumber, Is.EqualTo(105L));
        Assert.That(javaBean.DoubleNumber, Is.EqualTo(2D).Within(0.0001D));
        Assert.That(javaBean.FieldMappingNumber, Is.EqualTo(15));

        var nullableJavaBean = readContext.CreateSet<NullableJavaBean>().FirstOrDefault(p => p.IntNumber == Key);
        Assert.That(nullableJavaBean, Is.Not.Null);
        Assert.That(nullableJavaBean.LongNumber, Is.EqualTo(10L));
    }

    /// <summary>
    ///     测试为属性设置新值
    ///     同时覆盖属性名为映射字段名的情形 以及不存在的属性名
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void SetAttributes(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //多个属性同时设值 键为属性名称
        var affected = context.CreateSet<JavaBean>().SetAttributes(new[]
        {
            new KeyValuePair<string, object>(nameof(JavaBean.String), "更新后的字符串"),
            new KeyValuePair<string, object>(nameof(JavaBean.Bool), true)
        }, p => p.IntNumber == Key);
        Assert.That(affected, Is.EqualTo(1));

        //键也可以是映射字段名
        var fieldAffected = context.CreateSet<JavaBean>().SetAttributes(new[]
        {
            new KeyValuePair<string, object>("field_mapping_number", 100)
        }, p => p.IntNumber == Key);
        Assert.That(fieldAffected, Is.EqualTo(1));

        //可空值类型的属性同样支持设值
        var nullableAffected = context.CreateSet<NullableJavaBean>().SetAttributes(new[]
        {
            new KeyValuePair<string, object>(nameof(NullableJavaBean.LongNumber), 42L)
        }, p => p.IntNumber == Key);
        Assert.That(nullableAffected, Is.EqualTo(1));

        //不存在的属性名 应抛出参数异常
        var exception = Assert.Throws<ArgumentException>(() => context.CreateSet<JavaBean>()
            .SetAttributes([new KeyValuePair<string, object>("NoSuchProperty", 1)],
                p => p.IntNumber == Key));
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Does.Contain("无法与"));

        //从新的上下文读取 确认已写入数据库
        var readContext = ContextUtils.CreateContext(dataSource);
        var javaBean = readContext.CreateSet<JavaBean>().FirstOrDefault(p => p.IntNumber == Key);
        Assert.That(javaBean, Is.Not.Null);
        Assert.That(javaBean.String, Is.EqualTo("更新后的字符串"));
        Assert.That(javaBean.Bool, Is.True);
        Assert.That(javaBean.FieldMappingNumber, Is.EqualTo(100));

        var nullableJavaBean = readContext.CreateSet<NullableJavaBean>().FirstOrDefault(p => p.IntNumber == Key);
        Assert.That(nullableJavaBean, Is.Not.Null);
        Assert.That(nullableJavaBean.LongNumber, Is.EqualTo(42L));
    }
}
