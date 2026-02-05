using System;
using System.Linq;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Association;
using Obase.Test.Domain.Association.NoAssociationExtAttr;

namespace Obase.Test.CoreTest.AssociationTest;

/// <summary>
///     不定义关联对象冗余属性测试
/// </summary>
[TestFixture]
public class NoAssocationAttrTest
{
    /// <summary>
    ///     初始化测试
    /// </summary>
    [OneTimeSetUp]
    public void SetUp()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //清理可能的冗余数据
            context.CreateSet<NoAssociationExtAttrStudent>().Delete(p => p.StudentId > 0);
            context.CreateSet<NoAssociationExtAttrSchool>().Delete(p => p.SchoolId > 0);
            context.CreateSet<NoAssociationExtAttrClass>().Delete(p => p.ClassId > 0);
            context.CreateSet<NoAssociationExtAttrTeacher>().Delete(p => p.TeacherId > 0);
            context.CreateSet<ClassTeacher>().Delete(p => p.ClassId > 0 || p.TeacherId > 0);

            //加入测试学校
            var newschool = new NoAssociationExtAttrSchool
            {
                Createtime = DateTime.Now,
                EstablishmentTime = DateTime.Parse("1999-12-31 23:59:59"),
                IsPrime = false,
                Name = "不定义关联对象冗余属性的第X某某学校",
                SchoolType = (ESchoolType)new Random((int)DateTime.Now.Ticks).Next(3)
            };
            //学校的班级
            var newclass = new NoAssociationExtAttrClass
            {
                Name = "不定义关联对象冗余属性的某某班",
                School = newschool
            };

            context.CreateSet<NoAssociationExtAttrSchool>().Attach(newschool);
            context.CreateSet<NoAssociationExtAttrClass>().Attach(newclass);

            //加入学生
            for (var i = 1; i < 3; i++)
            {
                var student = new NoAssociationExtAttrStudent
                {
                    Class = newclass,
                    Name = $"不定义关联对象冗余属性的小{i}"
                };
                context.CreateSet<NoAssociationExtAttrStudent>().Attach(student);
            }

            //加入教师和班级任课教师
            var teacher = new NoAssociationExtAttrTeacher { Name = "不定义关联对象冗余属性的某老师" };
            var classTeacher = new NoAssociationExtAttrClassTeacher
            {
                Class = newclass,
                Teacher = teacher,
                IsManage = true,
                IsSubstitute = false,
                Subject = ["语文", "数学", "化学"]
            };
            //设置班级任课教师
            newclass.SetTeacher(classTeacher);
            context.CreateSet<NoAssociationExtAttrTeacher>().Attach(teacher);

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
            //清理可能的冗余数据
            context.CreateSet<NoAssociationExtAttrStudent>().Delete(p => p.StudentId > 0);
            context.CreateSet<NoAssociationExtAttrSchool>().Delete(p => p.SchoolId > 0);
            context.CreateSet<NoAssociationExtAttrClass>().Delete(p => p.ClassId > 0);
            context.CreateSet<NoAssociationExtAttrTeacher>().Delete(p => p.TeacherId > 0);
            context.CreateSet<ClassTeacher>().Delete(p => p.ClassId > 0 || p.TeacherId > 0);
        }
    }

    /// <summary>
    ///     测试方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void CurdTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //查询学校
        var school = context.CreateSet<NoAssociationExtAttrSchool>().FirstOrDefault(p => p.SchoolId > 0);
        Assert.That(school, Is.Not.Null);
        //查询班级 延迟加载学校 学生 任课教师
        var clazz = context.CreateSet<NoAssociationExtAttrClass>().FirstOrDefault(p => p.ClassId > 0);
        //验证数据
        Assert.That(clazz, Is.Not.Null);
        Assert.That(clazz.School, Is.Not.Null);
        Assert.That(clazz.Students, Is.Not.Null);
        Assert.That(clazz.Students.Count, Is.EqualTo(2));
        Assert.That(clazz.ClassTeachers, Is.Not.Null);
        Assert.That(clazz.ClassTeachers.Count, Is.EqualTo(1));

        //查询班级 Include学校 学生 任课教师
        context = ContextUtils.CreateContext(dataSource);
        clazz = context.CreateSet<NoAssociationExtAttrClass>().Include(p => p.School).Include(p => p.Students)
            .Include(p => p.ClassTeachers).FirstOrDefault(p => p.ClassId > 0);
        //验证数据
        Assert.That(clazz, Is.Not.Null);
        Assert.That(clazz.School, Is.Not.Null);
        Assert.That(clazz.Students, Is.Not.Null);
        Assert.That(clazz.Students.Count, Is.EqualTo(2));
        Assert.That(clazz.ClassTeachers, Is.Not.Null);
        Assert.That(clazz.ClassTeachers.Count, Is.EqualTo(1));


        context = ContextUtils.CreateContext(dataSource);
        //查询班级 Include加载任课教师.教师
        clazz = context.CreateSet<NoAssociationExtAttrClass>().Include(p => p.ClassTeachers.Select(q => q.Teacher))
            .FirstOrDefault(p => p.ClassId > 0);
        //验证数据
        Assert.That(clazz, Is.Not.Null);
        Assert.That(clazz.Students, Is.Not.Null);
        Assert.That(clazz.Students.Count, Is.EqualTo(2));
        Assert.That(clazz.ClassTeachers, Is.Not.Null);
        Assert.That(clazz.ClassTeachers.Count, Is.EqualTo(1));
        Assert.That(clazz.ClassTeachers[0].Teacher, Is.Not.Null);

        //移除测试
        context = ContextUtils.CreateContext(dataSource);
        //删除数据
        school = context.CreateSet<NoAssociationExtAttrSchool>().FirstOrDefault(p => p.SchoolId > 0);
        context.CreateSet<NoAssociationExtAttrSchool>().Remove(school);
        clazz = context.CreateSet<NoAssociationExtAttrClass>().FirstOrDefault(p => p.ClassId > 0);
        context.CreateSet<NoAssociationExtAttrClass>().Remove(clazz);

        context.SaveChanges();
    }
}