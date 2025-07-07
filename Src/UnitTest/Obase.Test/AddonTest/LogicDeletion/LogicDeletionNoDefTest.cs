using System;
using System.Linq;
using Obase.AddonTest.Domain.LogicDeletion;
using Obase.LogicDeletion;
using Obase.Providers.Sql;
using Obase.Test.Configuration;

namespace Obase.Test.AddonTest.LogicDeletion;

/// <summary>
///     代码配置的逻辑删除(无定义的字段)测试
/// </summary>
[TestFixture]
public class LogicDeletionNoDefTest
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
            context.CreateSet<LogicDeletionNoDef>().Delete(p => p.IntNumber > 0);

            //添加新对象 全部没删除
            for (var i = 1; i < 21; i++)
                context.Attach(new LogicDeletionNoDef
                {
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
            context.CreateSet<LogicDeletionNoDef>().Delete(p => p.IntNumber > 0);
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
        var list = context.CreateSet<LogicDeletionNoDef>().ToList();

        //有20个
        Assert.That(list.Count, Is.EqualTo(20));

        //逻辑删除其中部分
        foreach (var logicDeletion in list)
            //逻辑删除所有带1的字符串值 包含1,10,11,12,13,14,15,16,17,18,19
            if (logicDeletion.String.Contains("1"))
                context.CreateSet<LogicDeletionNoDef>().RemoveLogically(logicDeletion);
        context.SaveChanges();

        //查询
        list = context.CreateSet<LogicDeletionNoDef>().ToList();

        //有9个
        Assert.That(list.Count, Is.EqualTo(9));

        //测试直接逻辑删除
        context.CreateSet<LogicDeletionNoDef>().DeleteLogically(p => p.IntNumber <= 5);

        //查询
        list = context.CreateSet<LogicDeletionNoDef>().ToList();

        //有5个
        Assert.That(list.Count, Is.EqualTo(5));

        //测试恢复
        context.CreateSet<LogicDeletionNoDef>().RecoveryLogically(p => p.IntNumber >= 0);

        //查询
        list = context.CreateSet<LogicDeletionNoDef>().ToList();

        //有20个
        Assert.That(list.Count, Is.EqualTo(20));
    }
}