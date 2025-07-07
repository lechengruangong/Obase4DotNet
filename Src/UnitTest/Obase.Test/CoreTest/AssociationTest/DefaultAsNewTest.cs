using System.Linq;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Association.DefaultAsNew;

namespace Obase.Test.CoreTest.AssociationTest;

/// <summary>
///     测试关联端是否默认附加
/// </summary>
[TestFixture]
public class DefaultAsNewTest
{
    /// <summary>
    ///     构造测试 清理数据
    /// </summary>
    [OneTimeSetUp]
    public void SetUp()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //清理可能的冗余数据
            context.CreateSet<DefaultNewClass>().Delete(p => p.ClassId > 0);
            context.CreateSet<DefaultClass>().Delete(p => p.ClassId > 0);
            context.CreateSet<DefaultStudent>().Delete(p => p.StudentId > 0);
            context.CreateSet<DefaultSchool>().Delete(p => p.SchoolId > 0);
            context.CreateSet<DefaultClassTeacher>().Delete(p => p.TeacherId > 0 || p.ClassId > 0);
            context.CreateSet<DefaultTeacher>().Delete(p => p.TeacherId > 0);
        }
    }

    /// <summary>
    ///     销毁方法
    /// </summary>
    [OneTimeTearDown]
    public void Dispose()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //清理可能的冗余数据
            context.CreateSet<DefaultNewClass>().Delete(p => p.ClassId > 0);
            context.CreateSet<DefaultClass>().Delete(p => p.ClassId > 0);
            context.CreateSet<DefaultStudent>().Delete(p => p.StudentId > 0);
            context.CreateSet<DefaultSchool>().Delete(p => p.SchoolId > 0);
            context.CreateSet<DefaultClassTeacher>().Delete(p => p.TeacherId > 0 || p.ClassId > 0);
            context.CreateSet<DefaultTeacher>().Delete(p => p.TeacherId > 0);
        }
    }

    /// <summary>
    ///     新建关联端 且 关联表在左端 测试方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void DefaultAsNewImpLeftCreateAndQueryTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //新建班级
        var defaultNewClass = new DefaultNewClass
        {
            Name = "默认创建新关联端班级"
        };
        //保存
        context.CreateSet<DefaultNewClass>().Attach(defaultNewClass);
        context.SaveChanges();
        //新建学校
        defaultNewClass.School = new DefaultSchool { Name = "新学校1" };
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);
        //会作为新对象被保存
        var clazz = context.CreateSet<DefaultNewClass>().Include(p => p.School)
            .FirstOrDefault(p => p.ClassId == defaultNewClass.ClassId);
        //验证查询结果
        Assert.That(clazz, Is.Not.Null);
        Assert.That(clazz.School, Is.Not.Null);
        Assert.That(clazz.School.Name, Is.EqualTo("新学校1"));
    }

    /// <summary>
    ///     新建关联端 且 关联表在右端 测试方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void DefaultAsNewImpRightCreateAndQueryTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //新建班级
        var defaultNewClass = new DefaultNewClass
        {
            Name = "默认创建新关联端班级"
        };
        //保存
        context.CreateSet<DefaultNewClass>().Attach(defaultNewClass);
        context.SaveChanges();
        //新建学生
        defaultNewClass.Students = [new DefaultStudent { Name = "新学生1" }];
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);
        //会作为新对象被保存
        var clazz = context.CreateSet<DefaultNewClass>().Include(p => p.Students)
            .FirstOrDefault(p => p.ClassId == defaultNewClass.ClassId);
        //验证查询结果
        Assert.That(clazz, Is.Not.Null);
        Assert.That(clazz.Students, Is.Not.Null);
        Assert.That(clazz.Students[0], Is.Not.Null);
        Assert.That(clazz.Students[0].Name, Is.EqualTo("新学生1"));
    }

    /// <summary>
    ///     新建关联端 且 独立关联表 测试方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void DefaultAsNewIndependentCreateAndQueryTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //新建班级
        var defaultNewClass = new DefaultNewClass
        {
            Name = "默认创建新关联端班级"
        };
        //保存
        context.CreateSet<DefaultNewClass>().Attach(defaultNewClass);
        context.SaveChanges();
        //新建任课教师和教师
        defaultNewClass.ClassTeachers =
        [
            new DefaultNewClassTeacher
            {
                Class = defaultNewClass, ClassId = defaultNewClass.ClassId, IsManage = true,
                Teacher = new DefaultTeacher { Name = "新教师1" }
            }
        ];
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);
        //会作为新对象被保存
        var clazz = context.CreateSet<DefaultNewClass>().Include(p => p.ClassTeachers.Select(q => q.Teacher))
            .FirstOrDefault(p => p.ClassId == defaultNewClass.ClassId);
        //验证查询结果
        Assert.That(clazz, Is.Not.Null);
        Assert.That(clazz.ClassTeachers, Is.Not.Null);
        Assert.That(clazz.ClassTeachers[0], Is.Not.Null);
        Assert.That(clazz.ClassTeachers[0].Teacher.Name, Is.EqualTo("新教师1"));
    }

    /// <summary>
    ///     不新建关联端 且 关联表在左端 测试方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void DefaultImpLeftCreateAndQueryTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //新建班级
        var defaultClass = new DefaultClass
        {
            Name = "默认不创建新关联端班级"
        };
        context.CreateSet<DefaultClass>().Attach(defaultClass);

        //新建一个学校
        var school = new DefaultSchool { Name = "新学校1" };
        context.CreateSet<DefaultSchool>().Attach(school);
        context.SaveChanges();

        //换一个上下文
        context = ContextUtils.CreateContext(dataSource);
        var clazz = context.CreateSet<DefaultClass>().Include(p => p.School)
            .FirstOrDefault(p => p.ClassId == defaultClass.ClassId);
        Assert.That(clazz, Is.Not.Null);
        //再新建学校 复制之前的值
        clazz.School = new DefaultSchool { Name = school.Name, SchoolId = school.SchoolId };
        context.SaveChanges();

        //会建立关联
        clazz = context.CreateSet<DefaultClass>().Include(p => p.School)
            .FirstOrDefault(p => p.ClassId == defaultClass.ClassId);
        //验证查询结果    
        Assert.That(clazz, Is.Not.Null);
        Assert.That(clazz.School, Is.Not.Null);
        Assert.That(clazz.School.Name, Is.EqualTo("新学校1"));
        Assert.That(clazz.School.SchoolId, Is.EqualTo(school.SchoolId));
    }

    /// <summary>
    ///     不新建关联端 且 关联表在右端 测试方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void DefaultImpRightCreateAndQueryTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //新建班级
        var defaultClass = new DefaultClass
        {
            Name = "默认不创建新关联端班级"
        };

        context.CreateSet<DefaultClass>().Attach(defaultClass);
        //新建一个学生
        var student = new DefaultStudent { Name = "新学生1" };
        context.CreateSet<DefaultStudent>().Attach(student);
        context.SaveChanges();

        //换一个上下文
        context = ContextUtils.CreateContext(dataSource);
        var clazz = context.CreateSet<DefaultClass>().Include(p => p.Students)
            .FirstOrDefault(p => p.ClassId == defaultClass.ClassId);

        Assert.That(clazz, Is.Not.Null);

        //再新建学生 复制之前的值
        clazz.Students = [new DefaultStudent { Name = student.Name, StudentId = student.StudentId }];
        context.SaveChanges();

        //会建立关联
        clazz = context.CreateSet<DefaultClass>().Include(p => p.Students)
            .FirstOrDefault(p => p.ClassId == defaultClass.ClassId);
        //验证查询结果
        Assert.That(clazz, Is.Not.Null);
        Assert.That(clazz.Students, Is.Not.Null);
        Assert.That(clazz.Students[0], Is.Not.Null);
        Assert.That(clazz.Students[0].Name, Is.EqualTo("新学生1"));
    }

    /// <summary>
    ///     不新建关联端 且 独立关联表 测试方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void DefaultIndependentCreateAndQueryTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //新建班级
        var defaultClass = new DefaultClass
        {
            Name = "默认不创建新关联端班级"
        };
        //保存
        context.CreateSet<DefaultClass>().Attach(defaultClass);
        //新建教师
        var teacher = new DefaultTeacher { Name = "新教师1" };
        context.CreateSet<DefaultTeacher>().Attach(teacher);
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);
        //会作为新对象被保存
        var clazz = context.CreateSet<DefaultClass>().Include(p => p.ClassTeachers)
            .FirstOrDefault(p => p.ClassId == defaultClass.ClassId);
        Assert.That(clazz, Is.Not.Null);

        //新建任课教师和复制之前值的教师
        clazz.ClassTeachers =
        [
            new DefaultClassTeacher
            {
                Class = defaultClass, ClassId = defaultClass.ClassId, IsManage = true,
                Teacher = new DefaultTeacher { Name = teacher.Name, TeacherId = teacher.TeacherId }
            }
        ];
        context.SaveChanges();
        //验证结果
        Assert.That(clazz, Is.Not.Null);
        Assert.That(clazz.ClassTeachers, Is.Not.Null);
        Assert.That(clazz.ClassTeachers[0], Is.Not.Null);
        Assert.That(clazz.ClassTeachers[0].Teacher.Name, Is.EqualTo("新教师1"));
    }
}