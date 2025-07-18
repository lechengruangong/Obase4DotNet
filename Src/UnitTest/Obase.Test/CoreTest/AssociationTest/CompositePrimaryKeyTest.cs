using System.Linq;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Association;

namespace Obase.Test.CoreTest.AssociationTest;

/// <summary>
///     联合主键关联测试
/// </summary>
[TestFixture]
public class CompositePrimaryKeyTest
{
    /// <summary>
    ///     构造实例 装载初始数据
    /// </summary>
    [OneTimeSetUp]
    public void SetUp()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //清理可能的旧数据
            context.CreateSet<Teacher>().Delete(p => p.TeacherId > 0);
            context.CreateSet<PassPaper>().Delete(p => p.TeacherId > 0);

            //加入测试的联合主键老师
            var teacher = new Teacher { Name = "联合主键老师" };
            context.Attach(teacher);
            context.SaveChanges();
            //加入教师的通行证
            var passPaper1 = new PassPaper(teacher.TeacherId, EPassPaperType.A) { Memo = "备注1" };
            var passPaper2 = new PassPaper(teacher.TeacherId, EPassPaperType.B) { Memo = "备注2" };

            context.Attach(passPaper1);
            context.Attach(passPaper2);
            //保存
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
            //清理可能的旧数据
            context.CreateSet<Teacher>().Delete(p => p.TeacherId > 0);
            context.CreateSet<PassPaper>().Delete(p => p.TeacherId > 0);
        }
    }

    /// <summary>
    ///     联合主键测试
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void CurdTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //查询教师
        var qTeacher = context.CreateSet<Teacher>().FirstOrDefault(p => p.TeacherId > 0);
        //验证教师和通行证 此处通行证是延迟加载的
        Assert.That(qTeacher, Is.Not.Null);
        Assert.That(qTeacher.PassPaperList, Is.Not.Null);
        Assert.That(qTeacher.PassPaperList.Count, Is.EqualTo(2));

        //修改通行证
        qTeacher.PassPaperList[0].Memo = "修改后的备注1";
        qTeacher.PassPaperList[1].Memo = "修改后的备注2";
        //保存
        context.SaveChanges();

        //查出来 验证修改和包含加载
        context = ContextUtils.CreateContext(dataSource);
        qTeacher = context.CreateSet<Teacher>().Include(p => p.PassPaperList)
            .FirstOrDefault(p => p.TeacherId > 0);
        //验证
        Assert.That(qTeacher, Is.Not.Null);
        Assert.That(qTeacher.PassPaperList, Is.Not.Null);
        Assert.That(qTeacher.PassPaperList.Count, Is.EqualTo(2));
        Assert.That(qTeacher.PassPaperList[0].Memo, Is.EqualTo("修改后的备注1"));
        Assert.That(qTeacher.PassPaperList[1].Memo, Is.EqualTo("修改后的备注2"));

        //查询通行证
        context = ContextUtils.CreateContext(dataSource);
        var qPassPaper = context.CreateSet<PassPaper>().ToList();
        //验证
        Assert.That(qPassPaper, Is.Not.Null);
        Assert.That(qPassPaper.Count, Is.EqualTo(2));
        Assert.That(qPassPaper[1].Teacher.TeacherId, Is.EqualTo(qPassPaper[0].Teacher.TeacherId));

        //查询通行证并且使用Include加载教师
        context = ContextUtils.CreateContext(dataSource);
        qTeacher = context.CreateSet<Teacher>().Include(p => p.PassPaperList)
            .FirstOrDefault(p => p.TeacherId > 0);
        //验证
        Assert.That(qTeacher, Is.Not.Null);
        Assert.That(qTeacher.PassPaperList, Is.Not.Null);
        Assert.That(qTeacher.PassPaperList.Count, Is.EqualTo(2));

        //删除通行证和教师
        context.CreateSet<Teacher>().Remove(qTeacher);
        context.CreateSet<PassPaper>().Remove(qTeacher.PassPaperList[0]);
        context.CreateSet<PassPaper>().Remove(qTeacher.PassPaperList[1]);

        context.SaveChanges();
    }
}