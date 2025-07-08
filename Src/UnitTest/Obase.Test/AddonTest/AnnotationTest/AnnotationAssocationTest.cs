using System;
using System.Linq;
using Obase.AddonTest.Domain.Annotation;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;

namespace Obase.Test.AddonTest.AnnotationTest;

/// <summary>
///     标注建模基础关联测试
/// </summary>
[TestFixture]
public class AnnotationAssocationTest
{
    /// <summary>
    ///     构造实例 装载初始对象
    /// </summary>
    [OneTimeSetUp]
    public void SetUp()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateAddonContext(dataSource);

            //清理可能的冗余数据
            context.CreateSet<AnnotationStudent>().Delete(p => p.StudentId > 0);
            context.CreateSet<AnnotationSchool>().Delete(p => p.SchoolId > 0);
            context.CreateSet<AnnotationClass>().Delete(p => p.ClassId > 0);
            context.CreateSet<AnnotationClassTeacher>().Delete(p => p.ClassId > 0 || p.TeacherId > 0);
            context.CreateSet<AnnotationTeacher>().Delete(p => p.TeacherId > 0);

            //加入测试学校
            var newSchool = new AnnotationSchool
            {
                Createtime = DateTime.Now,
                EstablishmentTime = DateTime.Parse("1999-12-31 23:59:59"),
                IsPrime = false,
                Name = "标注第X某某学校",
                SchoolType = ESchoolType.Junior
            };

            //学校的班级
            var newClass = new AnnotationClass
            {
                Name = "标注某某班",
                School = newSchool
            };
            //附加对象
            context.Attach(newSchool);
            context.Attach(newClass);

            //加入学生
            for (var i = 1; i < 6; i++)
            {
                var student = new AnnotationStudent
                {
                    Class = newClass,
                    Name = $"标注小{i}",
                    School = newSchool
                };
                //附加对象
                context.Attach(student);
            }

            //加入教师
            var teacher = new AnnotationTeacher { Name = "标注某老师", School = newSchool };
            var clasTeacher = new AnnotationClassTeacher
            {
                Class = newClass,
                Teacher = teacher,
                IsManage = true,
                IsSubstitute = false,
                Subject = "标注语文"
            };
            //附加对象
            context.Attach(teacher);
            context.Attach(clasTeacher);
            //保存所有的对象
            context.SaveChanges();
        }
    }

    /// <summary>
    ///     销毁
    /// </summary>
    [OneTimeTearDown]
    public void TearDown()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateAddonContext(dataSource);
            //测试完成后清理数据
            context.CreateSet<AnnotationStudent>().Delete(p => p.StudentId > 0);
            context.CreateSet<AnnotationSchool>().Delete(p => p.SchoolId > 0);
            context.CreateSet<AnnotationClass>().Delete(p => p.ClassId > 0);
            context.CreateSet<AnnotationClassTeacher>().Delete(p => p.ClassId > 0 || p.TeacherId > 0);
            context.CreateSet<AnnotationTeacher>().Delete(p => p.TeacherId > 0);
        }
    }

    /// <summary>
    ///     简单的增删改查测试
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void SimpleCurdTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateAddonContext(dataSource);
        //查询所有的学生
        var students = context.CreateSet<AnnotationStudent>().Include(p => p.Class).Include(p => p.School).ToList();
        //有5个
        Assert.That(students, Is.Not.Null);
        Assert.That(students.Count, Is.EqualTo(5));
        //每一个都不是空 且有班级和学校
        foreach (var student in students)
        {
            Assert.That(student.Class, Is.Not.Null);
            Assert.That(student.School, Is.Not.Null);
        }

        //查询班级
        var classes = context.CreateSet<AnnotationClass>().Include(p => p.School)
            .Include(p => p.ClassTeachers.Select(q => q.Teacher.School)).ToList();
        //有1个
        Assert.That(classes, Is.Not.Null);
        Assert.That(classes.Count, Is.EqualTo(1));

        //班级的学生是延迟加载的 可以在访问后获取到
        var classStudents = classes[0].Students;
        //有5个学生
        Assert.That(classStudents, Is.Not.Null);
        Assert.That(classStudents.Count, Is.EqualTo(5));

        //班级的学校是Include的 可以直接获取
        var school = classes[0].School;
        //学校不为空
        Assert.That(school, Is.Not.Null);

        //班级的任课教师是Include的 且加载到了教师->学校
        var classTeachers = classes[0].ClassTeachers;
        //不为空
        Assert.That(classTeachers, Is.Not.Null);
        Assert.That(classTeachers.Count, Is.EqualTo(1));
        //任课教师有教师和学校
        Assert.That(classTeachers[0].Teacher, Is.Not.Null);
        Assert.That(classTeachers[0].Teacher.School, Is.Not.Null);
    }
}