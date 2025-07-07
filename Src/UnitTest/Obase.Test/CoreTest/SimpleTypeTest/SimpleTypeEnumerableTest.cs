using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Obase.Core;
using Obase.Core.Common;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Association;
using Obase.Test.Domain.SimpleType;

namespace Obase.Test.CoreTest.SimpleTypeTest;

/// <summary>
///     测试简单类型的Enumerable扩展方法
/// </summary>
[TestFixture]
public class SimpleTypeEnumerableTest
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

            //销毁所有对象
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
                    Guid = Guid.NewGuid(),
                    CharNumber = '\u006A',
                    FloatNumber = (float)Math.Pow(Math.PI, i),
                    DoubleNumber = (float)Math.Pow(Math.PI, i),
                    Time = new TimeSpan(0, 8, 40, 0),
                    Date = DateTime.Now,
                    String = $"{i}号字符串",
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
    ///     测试Aggregate方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void AggregateTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //累加成字符串: 开头是Start 而后是每个对象的string属性+,
        var aggregateString = context.CreateSet<JavaBean>().Aggregate("Start!", (s, p) => $"{s} {p.String},");

        Assert.That(aggregateString,
            Is.EqualTo(
                "Start! 1号字符串, 2号字符串, 3号字符串, 4号字符串, 5号字符串, 6号字符串, 7号字符串, 8号字符串, 9号字符串, 10号字符串, 11号字符串, 12号字符串, 13号字符串, 14号字符串, 15号字符串, 16号字符串, 17号字符串, 18号字符串, 19号字符串, 20号字符串,"));
    }

    /// <summary>
    ///     测试All方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void AllTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //是否都满足
        var allResult = context.CreateSet<JavaBean>().All(p => p.Bool);
        //有一半的对象Bool为true 所以不满足
        Assert.That(allResult, Is.False);

        //是否都满足
        var allResult1 = context.CreateSet<JavaBean>().All(p => p.DecimalNumber > 0);
        //所有的对象DecimalNumber都大于0 所以满足
        Assert.That(allResult1, Is.True);
    }

    /// <summary>
    ///     测试Any方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void AnyTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //是否任意满足
        var anyResult = context.CreateSet<JavaBean>().Any(p => p.Bool);
        //有一半的对象Bool为true 所以满足
        Assert.That(anyResult, Is.True);

        anyResult = context.CreateSet<JavaBean>().Any();
        //有20个对象 所以满足
        Assert.That(anyResult, Is.True);
    }

    /// <summary>
    ///     测试Avg方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void AvgTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //求平均数
        var avgResult = context.CreateSet<JavaBean>().Average(p => p.IntNumber);

        //本地算一遍
        var localResult = 0d;
        for (var i = 1; i < 21; i++) localResult += i;

        localResult /= 20;

        //SqlServer与其他数据精度略有不同
        Assert.That(avgResult, dataSource != EDataSource.SqlServer ? Is.EqualTo(localResult) : Is.EqualTo(10.0D));
    }

    /// <summary>
    ///     测试Cast方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void CastTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //进行转换 都转换为IModel类型
        var castResult = context.CreateSet<JavaBean>().Where(p => p.Bool).Cast<IModel>().ToList();

        var result = castResult.All(p => p != null);
        //每一个都可以转换为IModel类型
        Assert.That(result, Is.True);
    }

    /// <summary>
    ///     测试Contains方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ContainsTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //查找包含
        var containsResult = context.CreateSet<JavaBean>().Where(p => p.Strings.Contains("2")).ToList();
        //有8个
        Assert.That(containsResult.Count, Is.EqualTo(8));

        //定义一个本地的int列表
        var intList = new List<int> { 1, 2, 3, 4 };
        //使用这个列表进行Contains查询
        containsResult = context.CreateSet<JavaBean>().Where(p => intList.Contains(p.IntNumber)).ToList();
        //有4个
        Assert.That(containsResult.Count, Is.EqualTo(4));

        //用一个对象集的投影结果进行Contains查询
        containsResult = context.CreateSet<JavaBean>().Where(p =>
            context.CreateSet<JavaBean>().Where(q => q.Bool).Select(q => q.IntNumber).Contains(p.IntNumber)).ToList();
        //有10个
        Assert.That(containsResult.Count, Is.EqualTo(10));

        //定义一个本地的学生列表
        var studentList = new List<Student>
        {
            new() { StudentId = 5 },
            new() { StudentId = 6 },
            new() { StudentId = 7 },
            new() { StudentId = 8 }
        };
        //直接将本地IQueryable作为Contains的调用方传入 如果是可以支持的 就会正常执行 否则会抛出异常
        containsResult = context.CreateSet<JavaBean>()
            .Where(p => studentList.Where(q => q.ClassId >= 0).Select(q => q.StudentId).Contains(p.IntNumber)).ToList();
        //有4个
        Assert.That(containsResult.Count, Is.EqualTo(4));
    }

    /// <summary>
    ///     测试StartsWith 和EndsWith 方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void StartWithAndEndsWithTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //查找包含
        var containsResult = context.CreateSet<JavaBean>().Where(p => p.String.StartsWith("2号字")).ToList();
        //只有一个
        Assert.That(containsResult.Count, Is.EqualTo(1));
        //查找包含
        var containsResult1 = context.CreateSet<JavaBean>().Where(p => p.String.EndsWith("号字符串")).ToList();
        //有20个
        Assert.That(containsResult1.Count, Is.EqualTo(20));
        //反过来
        var containsResult2 = context.CreateSet<JavaBean>().Where(p => "1号字符串啊".StartsWith(p.String)).ToList();
        //Sqlite不会解析列名的%通配 所以是0个
        Assert.That(containsResult2.Count, dataSource == EDataSource.Sqlite ? Is.EqualTo(0) : Is.EqualTo(1));
    }

    /// <summary>
    ///     测试Count方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void CountTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //查找个数
        var countResult = context.CreateSet<JavaBean>().Count(p => p.DecimalNumber > 50);
        //有17个
        Assert.That(countResult, Is.EqualTo(17));
        //查找所有个数
        countResult = context.CreateSet<JavaBean>().Count();
        //有20个
        Assert.That(countResult, Is.EqualTo(20));
    }

    /// <summary>
    ///     测试DefaultIfEmpty方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void DefaultIfEmptyTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //查找默认
        var defaultIfEmptyResult =
            context.CreateSet<JavaBean>().Where(p => p.IntNumber < 0).DefaultIfEmpty().ToList();

        //没有满足条件的对象 所以返回一个默认值
        Assert.That(defaultIfEmptyResult.Count, Is.EqualTo(1));
        Assert.That(defaultIfEmptyResult[0], Is.Null);
    }

    /// <summary>
    ///     测试Distinct方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void DistinctTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //去重结果
        var distinctResult = context.CreateSet<JavaBean>().Distinct().ToList();
        //有20个
        Assert.That(distinctResult.Count, Is.EqualTo(20));
    }

    /// <summary>
    ///     测试ElementAT方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ElementAtTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //获取第2个元素
        var elementAtResult = context.CreateSet<JavaBean>().ElementAt(1);
        //第2个元素的IntNumber为2
        Assert.That(elementAtResult, Is.Not.Null);
        Assert.That(elementAtResult.IntNumber, Is.EqualTo(2));
    }

    /// <summary>
    ///     测试Except方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ExceptTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //两个集合
        var list = context.CreateSet<JavaBean>().Where(p => p.Bool).ToArray();
        var exceptResult = context.CreateSet<JavaBean>().Except(list).ToList();
        //取差集 有10个对象
        Assert.That(exceptResult.Count, Is.EqualTo(10));
    }

    /// <summary>
    ///     测试First方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void FirstTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //找出符合条件的第一个
        var firstResult = context.CreateSet<JavaBean>().FirstOrDefault(p => p.DecimalNumber > 90);
        //第一个DecimalNumber大于90的对象的IntNumber为4
        Assert.That(firstResult?.IntNumber, Is.EqualTo(4));
        //找出第一个
        firstResult = context.CreateSet<JavaBean>().FirstOrDefault();
        //第一个对象的IntNumber为1
        Assert.That(firstResult?.IntNumber, Is.EqualTo(1));
        //找出不存在的第一个
        firstResult = context.CreateSet<JavaBean>().FirstOrDefault(p => p.IntNumber == 0);
        //不存在的第一个对象为null
        Assert.That(firstResult, Is.Null);
    }

    /// <summary>
    ///     测试GroupJoin方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void GroupJoinTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //两个集合
        var list = context.CreateSet<JavaBean>().Where(p => p.Bool).ToList();
        //分组连接
        var groupJResult = context.CreateSet<JavaBean>().GroupJoin(list, p => p.Bool, p => p.Bool,
            (p, l) => new SmallJavaBeanLikeModel
                { Bool = p.Bool, DecimalNumber = p.DecimalNumber }).ToList();

        //有20个
        Assert.That(groupJResult.Count, Is.EqualTo(20));
    }

    /// <summary>
    ///     测试GroupBy方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void GroupTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //测试直接ToDictionary
        var dictResult = context.CreateSet<JavaBean>().ToDictionary(p => p.IntNumber, p => p);
        //有20个
        Assert.That(dictResult.Count, Is.EqualTo(20));

        //有条件和元素选择器的分组
        var groupResult = context.CreateSet<JavaBean>().Where(p => p.Bool).GroupBy(p => p.IntNumber, p => p.DateTime)
            .ToDictionary(p => p.Key, p => p.ToList());

        //有10个
        Assert.That(groupResult.Count, Is.EqualTo(10));
        //有条件的分组
        var groupNonElementSelecterResult = context.CreateSet<JavaBean>().Where(p => p.Bool).GroupBy(p => p.IntNumber)
            .ToDictionary(p => p.Key, p => p.ToList());
        //有10个
        Assert.That(groupNonElementSelecterResult.Count, Is.EqualTo(10));

        //分组中使用函数
        var functionGroup = context.CreateSet<JavaBean>().Where(p => p.Bool)
            .GroupBy(p => p.String.Substring(1, 1), p => p.DateTime)
            .ToDictionary(p => p.Key, p => p.ToList());

        //分组中使用函数 有5个
        Assert.That(functionGroup.Count, Is.EqualTo(5));

        //分组 投影 只有Key选择器
        var groupSelectWithoutElementSelectorResult = context.CreateSet<JavaBean>()
            .GroupBy(p => p.Bool, (p, t) => new { Bool = p, Objs = t.ToList() })
            .ToDictionary(p => p.Bool, p => p);
        //有2个
        Assert.That(groupSelectWithoutElementSelectorResult.Count, Is.EqualTo(2));

        //分组 投影
        var groupSelectResult = context.CreateSet<JavaBean>()
            .GroupBy(p => p.Bool, p => p.IntNumber, (p, t) => new { Bool = p, Sum = t.Sum() })
            .ToDictionary(p => p.Bool, p => p.Sum);
        //有2个
        Assert.That(groupSelectResult.Count, Is.EqualTo(2));

        //分组 投影 聚合函数无法翻译
        var groupListSelectResult = context.CreateSet<JavaBean>()
            .GroupBy(p => p.Bool, p => p.IntNumber, (p, t) => new { Bool = p, List = t.ToList() })
            .ToDictionary(p => p.Bool, p => p.List);
        //有2个
        Assert.That(groupListSelectResult.Count, Is.EqualTo(2));

        //分组 投影 聚合函数做参数
        var groupAggResult1 = context.CreateSet<JavaBean>()
            .GroupBy(p => p.Bool, p => p.IntNumber,
                (p, t) => new SimpleJavaBeanAvgGroup(p, t.Count()) { Avg = t.Average() })
            .ToDictionary(p => p.Bool, p => p);
        //有2个
        Assert.That(groupAggResult1.Count, Is.EqualTo(2));

        //分组 投影 无法翻译的函数做参数
        var groupAggResult2 = context.CreateSet<JavaBean>()
            .GroupBy(p => p.Bool, p => p.IntNumber,
                (p, t) => new SimpleJavaBeanListGroup(p, t.ToList()) { Avg = t.Average() })
            .ToDictionary(p => p.Bool, p => p);
        //有2个
        Assert.That(groupAggResult2.Count, Is.EqualTo(2));
    }

    /// <summary>
    ///     测试Intersect方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void IntersectTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //两个集合
        var list = context.CreateSet<JavaBean>().Where(p => p.Bool).ToArray();
        var intersectResult = context.CreateSet<JavaBean>().Intersect(list).ToList();
        //取交集 有10个对象
        Assert.That(intersectResult.Count, Is.EqualTo(10));
    }

    /// <summary>
    ///     测试Join方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void JoinTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //两个集合
        var list = context.CreateSet<JavaBean>().Where(p => p.Bool);
        var joinResult = context.CreateSet<JavaBean>().Join(list, p => p.Bool, p => p.Bool,
            (p, l) => new SmallJavaBeanLikeModel
                { Bool = p.Bool, DecimalNumber = p.DecimalNumber }).ToList();
        //连接后 有100个对象
        Assert.That(joinResult.Count, Is.EqualTo(100));
    }

    /// <summary>
    ///     测试last方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void LastTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //有条件的找出最后一个
        var lastResult = context.CreateSet<JavaBean>().LastOrDefault(p => p.DecimalNumber > 90);
        //最后一个DecimalNumber大于90的对象的IntNumber为20
        Assert.That(lastResult?.IntNumber, Is.EqualTo(20));

        //找出最后一个
        lastResult = context.CreateSet<JavaBean>().Last();
        //最后一个对象的IntNumber为20
        Assert.That(lastResult?.IntNumber, Is.EqualTo(20));

        //找出不存在的最后一个
        lastResult = context.CreateSet<JavaBean>().LastOrDefault(p => p.IntNumber == 0);
        //是null
        Assert.That(lastResult, Is.Null);
    }

    /// <summary>
    ///     测试Max方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void MaxTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //求最大
        var maxResult = context.CreateSet<JavaBean>().Max(p => p.IntNumber);
        //最大值为20
        Assert.That(maxResult, Is.EqualTo(20));
    }

    /// <summary>
    ///     测试min方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void MinTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //求最小
        var minResult = context.CreateSet<JavaBean>().Min(p => p.IntNumber);
        //最小值为1
        Assert.That(minResult, Is.EqualTo(1));
    }

    /// <summary>
    ///     测试无条件查询
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void NullQueryTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //无条件查询
        var list = context.CreateSet<JavaBean>().ToList();
        //有20个
        Assert.That(list.Count, Is.EqualTo(20));
    }

    /// <summary>
    ///     测试OfType方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void OfTypeTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //进行转换
        var ofTypeResult = context.CreateSet<JavaBean>().Where(p => p.Bool).OfType<IModel>().ToList();

        var result = ofTypeResult.All(p => p != null);
        //每一个都可以转换为IModel类型
        Assert.That(result, Is.True);
    }

    /// <summary>
    ///     测试Order方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void OrderTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //覆盖式排序 实际生效的是后一个
        var orderResult = context.CreateSet<JavaBean>().OrderByDescending(p => p.DecimalNumber)
            .OrderByDescending(p => p.IntNumber).ToList();
        //有20个对象 最后一个是1
        Assert.That(orderResult.Count, Is.EqualTo(20));
        Assert.That(orderResult.Last().IntNumber, Is.EqualTo(1));
        //非覆盖式排序 两个生效
        orderResult = context.CreateSet<JavaBean>().OrderBy(p => p.Time).ThenByDescending(p => p.IntNumber).ToList();
        //有20个对象 最后一个是1
        Assert.That(orderResult.Count, Is.EqualTo(20));
        Assert.That(orderResult.Last().IntNumber, Is.EqualTo(1));
    }

    /// <summary>
    ///     测试Reverse方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ReverseTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //排序后倒置
        var orderResult = context.CreateSet<JavaBean>().OrderBy(p => p.DecimalNumber).Reverse().ToList();
        //有20个对象 最后一个是1
        Assert.That(orderResult.Count, Is.EqualTo(20));
        Assert.That(orderResult.Last().IntNumber, Is.EqualTo(1));
    }

    /// <summary>
    ///     测试Select
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void SelectTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //投影到简单属性
        var selectResult = context.CreateSet<JavaBean>().Select(p => p.Bool).ToList();
        //有20个
        Assert.That(selectResult.Count, Is.EqualTo(20));
        //投影到匿名类型
        var selectToEntity =
            context.CreateSet<JavaBean>().Select(p => new SimpleJavaBeanSelect(p.Bool, p.IntNumber)
                { DecimalNumber = p.DecimalNumber }).ToList();
        //有20个
        Assert.That(selectToEntity.Count, Is.EqualTo(20));
    }

    /// <summary>
    ///     测试SequenceEqual方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void SequenceEqualTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //两个集合 测试相等
        var list = context.CreateSet<JavaBean>().Where(p => p.Bool).ToList();
        var sequenceEqualResult = context.CreateSet<JavaBean>().SequenceEqual(list);
        //并不相等
        Assert.That(sequenceEqualResult, Is.False);
    }

    /// <summary>
    ///     测试Single方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void SingleTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //单值 无默认值
        var singelResult = context.CreateSet<JavaBean>().Single(p => p.IntNumber == 1);
        //存在 不为空
        Assert.That(singelResult, Is.Not.Null);
        //单值 有默认值
        singelResult = context.CreateSet<JavaBean>().SingleOrDefault(p => p.IntNumber == 2);
        //存在 不为空
        Assert.That(singelResult, Is.Not.Null);
        //单值 有默认值 但有多个满足条件的 //会直接报错
        Assert.Throws<InvalidOperationException>(() => context.CreateSet<JavaBean>().SingleOrDefault());
    }

    /// <summary>
    ///     测试Skip Take
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void SkipTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //跳过10个取10个
        var skipResult = context.CreateSet<JavaBean>().Where(p => p.IntNumber > 0).Skip(10).Take(10).ToList();
        //有10个 开头的是11
        Assert.That(skipResult.Count, Is.EqualTo(10));
        Assert.That(skipResult.First().IntNumber, Is.EqualTo(11));
    }

    /// <summary>
    ///     测试Sum方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void SumTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //求和
        var sumResult = context.CreateSet<JavaBean>().Sum(p => p.IntNumber);
        //本地算一遍
        var localResult = 0d;
        for (var i = 1; i < 21; i++) localResult += i;
        //结果和本地的相等
        Assert.That(sumResult, Is.EqualTo(localResult));
    }

    /// <summary>
    ///     测试Union方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void UnionTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //两个集合
        var list = context.CreateSet<JavaBean>().Where(p => p.Bool).ToList();
        var unionResult = context.CreateSet<JavaBean>().Where(p => p.Bool == false).Union(list).ToList();
        //取并集 有20个对象
        Assert.That(unionResult.Count, Is.EqualTo(20));
    }

    /// <summary>
    ///     测试Where
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void WhereTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //复杂条件
        var whereResult = context.CreateSet<JavaBean>()
            .Where(p => p.IntNumber > 0 && p.DecimalNumber > 987 && p.Strings.Contains("2") == false).ToList();
        //有9个对象满足条件
        Assert.That(whereResult.Count, Is.EqualTo(9));
        //几种布尔值的查询
        whereResult = context.CreateSet<JavaBean>().Where(p => true).ToList();
        //有20个对象满足条件
        Assert.That(whereResult.Count, Is.EqualTo(20));

        whereResult = context.CreateSet<JavaBean>().Where(p => p.Bool != true).ToList();
        //有10个对象满足条件
        Assert.That(whereResult.Count, Is.EqualTo(10));

        whereResult = context.CreateSet<JavaBean>().Where(p => p.Bool == false).ToList();
        //有10个对象满足条件
        Assert.That(whereResult.Count, Is.EqualTo(10));

        //常数表达式置于比较前方
        whereResult = context.CreateSet<JavaBean>()
            .Where(p => 0 < p.IntNumber && 987 < p.DecimalNumber && p.Strings.Contains("2") == false).ToList();
        //有9个对象满足条件
        Assert.That(whereResult.Count, Is.EqualTo(9));
        //几种其他类型的前置
        whereResult = context.CreateSet<JavaBean>().Where(p => null != p.String).ToList();
        //有20个对象满足条件
        Assert.That(whereResult.Count, Is.EqualTo(20));

        whereResult = context.CreateSet<JavaBean>().Where(p => "" != p.String).ToList();
        //有20个对象满足条件
        Assert.That(whereResult.Count, Is.EqualTo(20));

        whereResult = context.CreateSet<JavaBean>().Where(p => true != !p.Bool).ToList();
        //有20个对象满足条件
        Assert.That(whereResult.Count, Is.EqualTo(10));

        //测试谓词条件组合器
        Expression<Func<JavaBean, bool>> expression = p => p.IntNumber > 0;
        expression = expression.And(p => p.DecimalNumber > 987);
        expression = expression.Or(p => p.IntNumber == 0);
        expression = expression.And(p => p.Strings.Contains("2") == false);
        whereResult = context.CreateSet<JavaBean>()
            .Where(expression).ToList();
        //有9个对象满足条件
        Assert.That(whereResult.Count, Is.EqualTo(9));
    }

    /// <summary>
    ///     测试WhereIf
    /// </summary>
    /// <param name="dataSource"></param>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void WhereIfTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //传入的参数肯定不是OLEDB 所以拼接了p => p.Bool
        var result = context.CreateSet<JavaBean>().Where(p => p.ByteNumber > 0).Where(p => p.IntNumber > 0)
            .WhereIf(dataSource != EDataSource.Oledb, p => p.Bool).ToList();
        //有10个对象满足条件
        Assert.That(result.Count, Is.EqualTo(10));
    }

    /// <summary>
    ///     测试Zip
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ZipTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //Zip方法参数
        var zipParameter = new List<int> { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

        var zipResult1 = context.CreateSet<JavaBean>().Where(p => p.IntNumber <= 10).Zip(zipParameter,
            (p, s) => p.IntNumber + s).ToList();

        Assert.That(zipResult1.Count, Is.EqualTo(10));
        //结果为查出的IntNumber和之前的值相加
        var seq = new[] { 12, 14, 16, 18, 20, 22, 24, 26, 28, 30 };
        //序列相等
        Assert.That(seq.SequenceEqual(zipResult1), Is.True);
    }
}

/// <summary>
///     简单JAVABEAN类 符合Json命名标准
/// </summary>
public class SimpleJavaBeanAvgGroup
{
    /// <summary>
    ///     构造简单JAVABEAN类
    /// </summary>
    /// <param name="b">布尔值</param>
    /// <param name="count">个数聚合</param>
    public SimpleJavaBeanAvgGroup(bool b, long count)
    {
        Bool = b;
        Count = count;
    }

    /// <summary>
    ///     布尔值
    /// </summary>
    public bool Bool { get; }

    /// <summary>
    ///     个数聚合
    /// </summary>
    public long Count { get; }

    /// <summary>
    ///     Int值Avg
    /// </summary>
    public double Avg { get; set; }
}

/// <summary>
///     简单JAVABEAN类 符合Json命名标准
/// </summary>
public class SimpleJavaBeanListGroup
{
    /// <summary>
    ///     构造简单JAVABEAN类
    /// </summary>
    /// <param name="b">布尔值</param>
    /// <param name="ints">Int值列表</param>
    public SimpleJavaBeanListGroup(bool b, List<int> ints)
    {
        Bool = b;
        Ints = ints;
    }

    /// <summary>
    ///     布尔值
    /// </summary>
    public bool Bool { get; }

    /// <summary>
    ///     Int值列表
    /// </summary>
    public List<int> Ints { get; }


    /// <summary>
    ///     Int值Avg
    /// </summary>
    public double Avg { get; set; }
}

/// <summary>
///     简单JAVABEAN类 符合Json命名标准
/// </summary>
public class SimpleJavaBeanSelect
{
    /// <summary>
    ///     构造简单JAVABEAN类
    /// </summary>
    /// <param name="b">布尔值</param>
    /// <param name="intNumber">Int</param>
    public SimpleJavaBeanSelect(bool b, long intNumber)
    {
        Bool = b;
        IntNumber = intNumber;
    }

    /// <summary>
    ///     布尔值
    /// </summary>
    public bool Bool { get; }

    /// <summary>
    ///     Int
    /// </summary>
    public long IntNumber { get; }

    /// <summary>
    ///     Decimal值
    /// </summary>
    public decimal DecimalNumber { get; set; }
}