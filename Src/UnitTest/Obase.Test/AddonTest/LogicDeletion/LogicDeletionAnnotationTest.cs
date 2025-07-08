using System;
using System.Linq;
using Obase.AddonTest.Domain.LogicDeletion;
using Obase.LogicDeletion;
using Obase.Providers.Sql;
using Obase.Test.Configuration;

namespace Obase.Test.AddonTest.LogicDeletion;

/// <summary>
///     标注配置的逻辑删除(有定义的字段)测试
/// </summary>
[TestFixture]
public class LogicDeletionAnnotationTest
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

            //销毁可能的冗余数据
            context.CreateSet<LogicDeletionAnnotation>().Delete(p => p.IntNumber > 0);

            //添加新对象 一半删除一半没删除
            for (var i = 1; i < 21; i++)
                context.Attach(new LogicDeletionAnnotation
                {
                    Bool = i % 2 == 0,
                    DateTime = DateTime.Now,
                    DecimalNumber = Math.Pow(Math.PI, i),
                    IntNumber = i,
                    String = $"{i}号字符串"
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
            var context = ContextUtils.CreateAddonContext(dataSource);
            //销毁可能的冗余数据
            context.CreateSet<LogicDeletionAnnotation>().Delete(p => p.IntNumber > 0);
        }
    }

    /// <summary>
    ///     简单的增删改查测试
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void QueryTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateAddonContext(dataSource);

        //无条件查询
        var list = context.CreateSet<LogicDeletionAnnotation>().ToList();

        //有10个
        Assert.That(list.Count, Is.EqualTo(10));

        //逻辑删除其中部分
        foreach (var logicDeletion in list)
            //逻辑删除所有带1的字符串值 包含1,10,11,13,15,17,19
            if (logicDeletion.String.Contains("1"))
                context.CreateSet<LogicDeletionAnnotation>().RemoveLogically(logicDeletion);
        context.SaveChanges();

        //查询
        list = context.CreateSet<LogicDeletionAnnotation>().ToList();

        //有4个
        Assert.That(list.Count, Is.EqualTo(4));

        //测试直接逻辑删除
        context.CreateSet<LogicDeletionAnnotation>().DeleteLogically(p => p.IntNumber <= 5);

        //查询
        list = context.CreateSet<LogicDeletionAnnotation>().ToList();

        //有2个
        Assert.That(list.Count, Is.EqualTo(2));

        //测试恢复
        context.CreateSet<LogicDeletionAnnotation>().RecoveryLogically(p => p.IntNumber >= 0);

        //查询
        list = context.CreateSet<LogicDeletionAnnotation>().ToList();

        //有20个
        Assert.That(list.Count, Is.EqualTo(20));
    }
}