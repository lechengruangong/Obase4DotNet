using System;
using System.Collections.Generic;
using System.Linq;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.SimpleType;

namespace Obase.Test.CoreTest.SimpleTypeTest;

/// <summary>
///     JavaBean 的 List&lt;long&gt; 属性(Numbers)的集成测试
///     覆盖值类型元素集合的建模 序列化存储 读取 更新 以及在同一个对象上与其它属性共存
/// </summary>
[TestFixture]
public class SimpleTypeValueCollectionIntegrationTest
{
    /// <summary>
    ///     写入的对象数量
    /// </summary>
    private const int Count = 20;

    /// <summary>
    ///     构造实例 为上下文赋值
    /// </summary>
    [OneTimeSetUp]
    public void SetUp()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);

            //销毁所有对象
            context.CreateSet<JavaBean>().Delete(p => p.IntNumber > 0);

            //添加新对象
            for (var i = 1; i <= Count; i++)
                context.Attach(new JavaBean
                {
                    Bool = i % 2 == 0,
                    DateTime = DateTime.Now,
                    DecimalNumber = new decimal(Math.Pow(Math.PI, i)),
                    IntNumber = i,
                    LongNumber = i * 100L,
                    ByteNumber = (byte)i,
                    Guid = Guid.NewGuid(),
                    CharNumber = '\u006A',
                    FloatNumber = (float)Math.Pow(Math.PI, i),
                    DoubleNumber = (float)Math.Pow(Math.PI, i),
                    Time = new TimeSpan(0, 8, 40, 0),
                    Date = DateTime.Now,
                    String = $"{i}号字符串",
                    Strings = new[] { $"{i - 1}", $"{i}", $"{i + 1}" },
                    //值类型元素集合
                    Numbers = new List<long> { i, i * 10L, i * 100L },
                    //可空值类型元素集合 中间插入一个null
                    NullableNumbers = new List<long?> { i, null, i * 10L }
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
            //销毁所有对象
            context.CreateSet<JavaBean>().Delete(p => p.IntNumber > 0);
        }
    }

    /// <summary>
    ///     测试从数据库读取 List&lt;long&gt; 属性
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ReadValueCollection(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        var list = context.CreateSet<JavaBean>().OrderBy(p => p.IntNumber).ToList();

        Assert.That(list.Count, Is.EqualTo(Count));

        foreach (var bean in list)
        {
            Assert.That(bean.Numbers, Is.Not.Null, $"{bean.IntNumber}号对象的Numbers不应为空");
            Assert.That(bean.Numbers, Is.EqualTo(new List<long> { bean.IntNumber, bean.IntNumber * 10L,
                bean.IntNumber * 100L }), $"{bean.IntNumber}号对象的Numbers不正确");
        }

        //最小值与最大值都能正确往返
        Assert.That(list[0].Numbers, Is.EqualTo(new List<long> { 1L, 10L, 100L }));
        Assert.That(list[Count - 1].Numbers, Is.EqualTo(new List<long> { Count, Count * 10L, Count * 100L }));
    }

    /// <summary>
    ///     测试可空值类型元素集合(List&lt;long?&gt;)的往返
    ///     集合中允许存在null项
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ReadNullableValueCollection(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        var list = context.CreateSet<JavaBean>().OrderBy(p => p.IntNumber).ToList();

        Assert.That(list.Count, Is.EqualTo(Count));

        foreach (var bean in list)
        {
            Assert.That(bean.NullableNumbers, Is.Not.Null, $"{bean.IntNumber}号对象的NullableNumbers不应为空");
            Assert.That(bean.NullableNumbers.Count, Is.EqualTo(3));
            //null项必须原样保留
            Assert.That(bean.NullableNumbers, Is.EqualTo(new List<long?>
            {
                bean.IntNumber, null, bean.IntNumber * 10L
            }), $"{bean.IntNumber}号对象的NullableNumbers不正确");
        }

        Assert.That(list[0].NullableNumbers, Is.EqualTo(new List<long?> { 1L, null, 10L }));
    }

    /// <summary>
    ///     测试可空值类型元素集合的更新(含全部为null与全部非null)
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void UpdateNullableValueCollection(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        var bean = context.CreateSet<JavaBean>().First(p => p.IntNumber == 2);

        //全部为null
        bean.NullableNumbers = new List<long?> { null, null };
        context.SaveChanges();

        var newContext = ContextUtils.CreateContext(dataSource);
        var reloaded = newContext.CreateSet<JavaBean>().First(p => p.IntNumber == 2);
        Assert.That(reloaded.NullableNumbers, Is.EqualTo(new List<long?> { null, null }));

        //全部非null
        reloaded.NullableNumbers = new List<long?> { 20L, 21L };
        newContext.SaveChanges();

        var finalContext = ContextUtils.CreateContext(dataSource);
        var finalReloaded = finalContext.CreateSet<JavaBean>().First(p => p.IntNumber == 2);
        Assert.That(finalReloaded.NullableNumbers, Is.EqualTo(new List<long?> { 20L, 21L }));

        //还原 避免影响其它用例
        finalReloaded.NullableNumbers = new List<long?> { 2L, null, 20L };
        finalContext.SaveChanges();
    }

    /// <summary>
    ///     测试值类型元素集合与引用类型元素集合在同一个对象上共存
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ValueCollectionCoexistsWithOthers(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        var bean = context.CreateSet<JavaBean>().First(p => p.IntNumber == 7);

        //引用类型元素集合(List<string>写法)仍然正常
        Assert.That(bean.Strings, Is.EqualTo(new[] { "6", "7", "8" }));
        //值类型元素集合正常
        Assert.That(bean.Numbers, Is.EqualTo(new List<long> { 7L, 70L, 700L }));
        //可空值类型元素集合正常
        Assert.That(bean.NullableNumbers, Is.EqualTo(new List<long?> { 7L, null, 70L }));
        //同行的其它标量属性不受影响
        Assert.That(bean.String, Is.EqualTo("7号字符串"));
        Assert.That(bean.LongNumber, Is.EqualTo(700L));
    }

    /// <summary>
    ///     测试条件查询后读取 List&lt;long&gt; 属性
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ReadValueCollectionByCondition(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        var bean = context.CreateSet<JavaBean>().FirstOrDefault(p => p.IntNumber == 5);
        Assert.That(bean, Is.Not.Null);
        Assert.That(bean.Numbers, Is.EqualTo(new List<long> { 5L, 50L, 500L }));

        //条件查询返回的多个对象
        var list = context.CreateSet<JavaBean>().Where(p => p.IntNumber > 15).OrderBy(p => p.IntNumber).ToList();
        Assert.That(list.Count, Is.EqualTo(5));
        Assert.That(list.All(p => p.Numbers != null && p.Numbers.Count == 3), Is.True);
    }

    /// <summary>
    ///     测试修改 List&lt;long&gt; 属性后保存并重新读取
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void UpdateValueCollection(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        var bean = context.CreateSet<JavaBean>().First(p => p.IntNumber == 1);

        //整体替换
        bean.Numbers = new List<long> { 999L, 1000L };
        context.SaveChanges();

        var newContext = ContextUtils.CreateContext(dataSource);
        var reloaded = newContext.CreateSet<JavaBean>().First(p => p.IntNumber == 1);
        Assert.That(reloaded.Numbers, Is.EqualTo(new List<long> { 999L, 1000L }));

        //清空
        reloaded.Numbers = new List<long>();
        newContext.SaveChanges();

        var emptyContext = ContextUtils.CreateContext(dataSource);
        var emptyReloaded = emptyContext.CreateSet<JavaBean>().First(p => p.IntNumber == 1);
        Assert.That(emptyReloaded.Numbers, Is.Not.Null);
        Assert.That(emptyReloaded.Numbers.Count, Is.EqualTo(0));

        //还原 避免影响其它用例
        emptyReloaded.Numbers = new List<long> { 1L, 10L, 100L };
        emptyContext.SaveChanges();

        var finalContext = ContextUtils.CreateContext(dataSource);
        Assert.That(finalContext.CreateSet<JavaBean>().First(p => p.IntNumber == 1).Numbers,
            Is.EqualTo(new List<long> { 1L, 10L, 100L }));
    }

    /// <summary>
    ///     测试 List&lt;long&gt; / List&lt;long?&gt; 属性以Json文本形式参与序列化(取值器输出的即入库内容)
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ValueCollectionIsStoredAsJson(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        var structuralType = context.Model.GetStructuralType(typeof(JavaBean));
        var attribute = structuralType.GetAttribute("Numbers");
        var nullableAttribute = structuralType.GetAttribute("NullableNumbers");

        Assert.That(attribute, Is.Not.Null, "Numbers应当被建模为属性");
        Assert.That(nullableAttribute, Is.Not.Null, "NullableNumbers应当被建模为属性");
        //AttributeTypeConvert把List<long>和List<long?>折叠为string存储
        Assert.That(attribute.DataType, Is.EqualTo(typeof(string)));
        Assert.That(nullableAttribute.DataType, Is.EqualTo(typeof(string)));

        var bean = context.CreateSet<JavaBean>().First(p => p.IntNumber == 3);

        Assert.That(attribute.ValueGetter.GetValue(bean), Is.EqualTo("[3,30,300]"));
        //可空值类型元素中的null在Json里表现为null字面量
        Assert.That(nullableAttribute.ValueGetter.GetValue(bean), Is.EqualTo("[3,null,30]"));
    }
}
