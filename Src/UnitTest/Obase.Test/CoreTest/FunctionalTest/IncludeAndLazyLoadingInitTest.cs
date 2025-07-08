using System.Linq;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Association;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     包含和延迟加载初始化容器测试
/// </summary>
[TestFixture]
public class IncludeAndLazyLoadingInitTest
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
            context.CreateSet<Student>().Delete(p => p.StudentId > 0);
            context.CreateSet<Class>().Delete(p => p.ClassId > 0);
            context.CreateSet<Teacher>().Delete(p => p.TeacherId > 0);

            //添加一个教师
            var teacher = new Teacher
            {
                Name = "延迟加载无通行证教师"
            };

            context.Attach(teacher);
            context.SaveChanges();

            //添加班级和学生
            //学校的班级
            var newclass = new Class
            {
                Name = "初始化容器某某班"
            };

            context.Attach(newclass);

            //加入学生
            for (var i = 1; i < 6; i++)
            {
                var student = new Student
                {
                    Class = newclass,
                    Name = $"小{i}"
                };
                context.Attach(student);
            }

            //保存
            context.SaveChanges();
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
            context.CreateSet<Student>().Delete(p => p.StudentId > 0);
            context.CreateSet<Class>().Delete(p => p.ClassId > 0);
            context.CreateSet<Teacher>().Delete(p => p.TeacherId > 0);
        }
    }

    /// <summary>
    ///     测试包含和延迟加载初始化容器
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Test(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //此教师无通行证
        var queryTeacher = context.CreateSet<Teacher>().FirstOrDefault(p => p.Name == "延迟加载无通行证教师");
        //使用延迟加载进行加载 获得一个空容器
        Assert.That(queryTeacher, Is.Not.Null);
        Assert.That(queryTeacher.PassPaperList, Is.Not.Null);
        Assert.That(queryTeacher.PassPaperList.Count, Is.EqualTo(0));

        context = ContextUtils.CreateContext(dataSource);
        //使用Include进行加载 获得一个空容器
        queryTeacher = context.CreateSet<Teacher>().Include(p => p.PassPaperList)
            .FirstOrDefault(p => p.Name == "延迟加载无通行证教师");

        Assert.That(queryTeacher, Is.Not.Null);
        Assert.That(queryTeacher.PassPaperList, Is.Not.Null);
        Assert.That(queryTeacher.PassPaperList.Count, Is.EqualTo(0));

        //删除
        context.Remove(queryTeacher);
        context.SaveChanges();

        //测试无延迟加载的关联
        context = ContextUtils.CreateContext(dataSource);

        var clazz = context.CreateSet<Class>().FirstOrDefault();
        //没有包含Student 是空值
        Assert.That(clazz, Is.Not.Null);
        Assert.That(clazz.Students, Is.Null);
        //没有包含ClassTeachers 是空值
        Assert.That(clazz, Is.Not.Null);
        Assert.That(clazz.ClassTeachers, Is.Null);

        clazz = context.CreateSet<Class>().Include(p => p.Students).Include(p => p.ClassTeachers)
            .FirstOrDefault(p => p.Name == "初始化容器某某班");
        Assert.That(clazz, Is.Not.Null);
        //加载了Student 有值
        Assert.That(clazz.Students, Is.Not.Null);
        Assert.That(clazz.Students.Count, Is.EqualTo(5));
        //加载了ClassTeacher 没值
        Assert.That(clazz.ClassTeachers, Is.Not.Null);
        Assert.That(clazz.ClassTeachers.Count, Is.EqualTo(0));
    }
}