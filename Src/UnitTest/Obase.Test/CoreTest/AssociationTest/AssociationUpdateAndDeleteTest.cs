using System;
using System.Collections.Generic;
using System.Linq;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Association;

namespace Obase.Test.CoreTest.AssociationTest;

/// <summary>
///     关联的更新和删除测试
/// </summary>
[TestFixture]
public class AssociationUpdateAndDeleteTest
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
            context.CreateSet<School>().Delete(p => p.SchoolId > 0);
            context.CreateSet<Class>().Delete(p => p.ClassId > 0);

            //添加新对象
            for (var i = 1; i < 5; i++)
            {
                var school = new School
                {
                    Createtime = DateTime.Now,
                    EstablishmentTime = DateTime.Parse("1999-12-31 23:59:59"),
                    IsPrime = i % 2 == 0,
                    Name = $"第{i}某某学校",
                    SchoolType = (ESchoolType)new Random((int)DateTime.Now.Ticks).Next(3)
                };
                context.Attach(school);

                if (i == 1)
                {
                    //学校的班级 只有一个
                    var newclass = new Class
                    {
                        Name = "某某班",
                        School = school
                    };
                    context.Attach(newclass);
                }
            }

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
            context.CreateSet<School>().Delete(p => p.SchoolId > 0);
            context.CreateSet<Class>().Delete(p => p.ClassId > 0);
        }
    }

    /// <summary>
    ///     测试修改和标记删除方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void UpdateAndRemove(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //查找一组对象
        var list = context.CreateSet<Class>().Where(p => p.ClassId > 0).Include(p => p.School).ToList();
        //1个班级 一个学校
        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(list[0].School, Is.Not.Null);

        //同时修改学校的名称和班级的名称
        list[0].Name = "新某某班";
        list[0].School.Name = "新第X某某学校";
        //保存
        context.SaveChanges();

        //查出来
        context = ContextUtils.CreateContext(dataSource);
        //查询修改的学校
        var cla = context.CreateSet<Class>().Include(p => p.School).FirstOrDefault(p => p.ClassId == list[0].ClassId);

        //是修改后的值
        Assert.That(cla, Is.Not.Null);
        Assert.That(cla.Name, Is.EqualTo("新某某班"));
        Assert.That(cla.School, Is.Not.Null);
        Assert.That(cla.School.Name, Is.EqualTo("新第X某某学校"));

        //标记删除
        context.Remove(cla);
        context.SaveChanges();

        var exist = context.CreateSet<Class>().Any(p => p.ClassId == list[0].ClassId);
        //不存在此对象
        Assert.That(exist, Is.False);
    }


    /// <summary>
    ///     测试直接移除和直接修改方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void DeleteAndDirectChangeTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //添加一个对象
        var school = new School
        {
            Createtime = DateTime.Now,
            EstablishmentTime = DateTime.Parse("1999-12-31 23:59:59"),
            IsPrime = false,
            Name = "第X某某学校",
            SchoolType = (ESchoolType)new Random((int)DateTime.Now.Ticks).Next(3)
        };
        context.Attach(school);
        //保存
        context.SaveChanges();

        //查出来
        var schoolId = school.SchoolId;

        school = context.CreateSet<School>().FirstOrDefault(p => p.SchoolId == schoolId);
        //存在此对象
        Assert.That(school, Is.Not.Null);

        //就地修改
        var result = context.CreateSet<School>().SetAttributes(
            new[] { new KeyValuePair<string, object>("Name", "新第X某某学校") },
            p => p.SchoolId == schoolId);

        //查出来
        context = ContextUtils.CreateContext(dataSource);
        school = context.CreateSet<School>().FirstOrDefault(p => p.SchoolId == schoolId);
        //是修改后的值
        Assert.That(school, Is.Not.Null);
        Assert.That(result, Is.EqualTo(1));
        Assert.That(school.Name, Is.EqualTo("新第X某某学校"));

        //就地删除
        result = context.CreateSet<School>().Delete(p => p.SchoolId == schoolId);
        //受影响的行数为1
        Assert.That(result, Is.EqualTo(1));

        //查出来
        context = ContextUtils.CreateContext(dataSource);
        school = context.CreateSet<School>().FirstOrDefault(p => p.SchoolId == schoolId);
        //没有此对象
        Assert.That(school, Is.Null);
    }

    /// <summary>
    ///     测试显式关联修改
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ExplicitAssociationModifyTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //新增一个学校的班级
        var newclass = new Class
        {
            Name = "显示关联班"
        };

        context.Attach(newclass);
        //新增一个教师和任课教师
        var teacher = new Teacher { Name = "显示关联老师" };
        var clasTeacher = new ClassTeacher(newclass, teacher)
        {
            IsManage = true,
            IsSubstitute = false,
            Subject = ["语文", "数学", "化学"]
        };

        newclass.SetTeacher(clasTeacher);
        context.Attach(teacher);
        //保存数据
        context.SaveChanges();

        //查出来 修改属性
        context = ContextUtils.CreateContext(dataSource);
        var clazz = context.CreateSet<Class>().Include(p => p.ClassTeachers.Select(q => q.Teacher))
            .FirstOrDefault(p => p.ClassId == newclass.ClassId);

        teacher = context.CreateSet<Teacher>().FirstOrDefault(p => p.TeacherId == teacher.TeacherId);
        //都有值
        Assert.That(clazz, Is.Not.Null);
        Assert.That(clazz.ClassTeachers, Is.Not.Null);
        Assert.That(teacher, Is.Not.Null);
        //将任课教师的属性修改
        clazz.ClassTeachers[0].Subject = ["显示关联语文", "显示关联数学", "显示关联化学"];
        clazz.ClassTeachers[0].IsManage = false;
        clazz.ClassTeachers[0].IsSubstitute = true;
        //保存
        context.SaveChanges();

        //查出来
        context = ContextUtils.CreateContext(dataSource);
        clazz = context.CreateSet<Class>().Include(p => p.ClassTeachers.Select(q => q.Teacher))
            .FirstOrDefault(p => p.ClassId == newclass.ClassId);

        teacher = context.CreateSet<Teacher>().FirstOrDefault(p => p.TeacherId == teacher.TeacherId);
        //验证属性
        Assert.That(clazz, Is.Not.Null);
        Assert.That(clazz.ClassTeachers, Is.Not.Null);
        Assert.That(teacher, Is.Not.Null);
        //验证任课教师的属性
        Assert.That(
            new List<string> { "显示关联语文", "显示关联数学", "显示关联化学" }.SequenceEqual(clazz.ClassTeachers[0].Subject), Is.True);
        Assert.That(clazz.ClassTeachers[0].IsManage, Is.False);
        Assert.That(clazz.ClassTeachers[0].IsSubstitute, Is.True);
        //移除后重建相同关联端的对象并修改属性
        clazz.ClassTeachers.Clear();

        clasTeacher = new ClassTeacher(clazz, teacher)
        {
            IsManage = false,
            IsSubstitute = false,
            Subject = ["显示关联语文1", "显示关联数学2", "显示关联化学3"]
        };
        //重新设置任课教师
        clazz.SetTeacher(clasTeacher);
        //保存
        context.SaveChanges();

        //查出来
        context = ContextUtils.CreateContext(dataSource);
        clazz = context.CreateSet<Class>().Include(p => p.ClassTeachers.Select(q => q.Teacher))
            .FirstOrDefault(p => p.ClassId == newclass.ClassId);

        //验证属性
        Assert.That(clazz, Is.Not.Null);
        Assert.That(clazz.ClassTeachers, Is.Not.Null);
        Assert.That(clazz.ClassTeachers[0].Teacher, Is.Not.Null);
        Assert.That(clazz.ClassTeachers[0].Teacher.TeacherId, Is.EqualTo(teacher.TeacherId));
        //验证任课教师的属性
        Assert.That(
            new List<string> { "显示关联语文1", "显示关联数学2", "显示关联化学3" }.SequenceEqual(clazz.ClassTeachers[0].Subject),
            Is.True);
        Assert.That(clazz.ClassTeachers[0].IsManage, Is.False);
        Assert.That(clazz.ClassTeachers[0].IsSubstitute, Is.False);

        //删除任课教师和班级
        context.CreateSet<Class>().Remove(clazz);
        context.CreateSet<Teacher>().Remove(teacher);
        context.SaveChanges();
    }
}