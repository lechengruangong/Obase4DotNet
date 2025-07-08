using System.Collections.Generic;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Association;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     使用ObjectSet创建对象测试
/// </summary>
[TestFixture]
public class ObjectSetCreateTest
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
            //清理可能的冗余数据
            context.CreateSet<Class>().Delete(p => p.ClassId > 0);
            context.CreateSet<ClassTeacher>().Delete(p => p.ClassId > 0 || p.TeacherId > 0);
            context.CreateSet<Teacher>().Delete(p => p.TeacherId > 0);
        }
    }

    /// <summary>
    ///     销毁对象
    /// </summary>
    [OneTimeTearDown]
    public void Dispose()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //清理可能的冗余数据
            context.CreateSet<Class>().Delete(p => p.ClassId > 0);
            context.CreateSet<ClassTeacher>().Delete(p => p.ClassId > 0 || p.TeacherId > 0);
            context.CreateSet<Teacher>().Delete(p => p.TeacherId > 0);
        }
    }

    /// <summary>
    ///     测试使用ObjectSet创建对象
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void CreateTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //创建一个班级和一个教师
        var clazz = new Class
        {
            Name = "C班级"
        };
        var teacher = new Teacher
        {
            Name = "C教师"
        };
        context.Attach(clazz);
        context.Attach(teacher);
        context.SaveChanges();
        //创建一个显式关联对象 此种方式New的对象是域类对象 无法触发延迟加载
        var classTeacher =
            new ClassTeacher(clazz.ClassId, teacher.TeacherId, true, true, ["C课程"]);
        //关联端对象均为null
        Assert.That(classTeacher.Class, Is.Null);
        Assert.That(classTeacher.Teacher, Is.Null);

        //使用Create方法创建 会根据定义的端冗余主键进行加载 并将创建的对象附加到上下文
        classTeacher =
            context.Create<ClassTeacher>(clazz.ClassId, teacher.TeacherId, true, true, new List<string> { "C课程" });
        //关联端对象均不为null
        Assert.That(classTeacher.Class, Is.Not.Null);
        Assert.That(classTeacher.Teacher, Is.Not.Null);

        //此处已经将classTeacher附加 直接保存即可
        context.SaveChanges();
    }
}