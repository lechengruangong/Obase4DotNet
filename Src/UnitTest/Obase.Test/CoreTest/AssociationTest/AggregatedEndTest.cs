using System.Linq;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Association;

namespace Obase.Test.CoreTest.AssociationTest;

/// <summary>
///     聚合端测试
/// </summary>
[TestFixture]
public class AggregatedEndTest
{
    /// <summary>
    ///     构造实例 装载初始对象
    /// </summary>
    [OneTimeSetUp]
    public void SetUp()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //清理可能的冗余数据
            context.CreateSet<Student>().Delete(p => p.StudentId > 0);
            context.CreateSet<StudentInfo>().Delete(p => p.StudentInfoId > 0);

            context = ContextUtils.CreateContext(dataSource);

            //加入测试数据
            //加入学生
            for (var i = 1; i < 6; i++)
            {
                var student = new Student
                {
                    Name = $"小{i}"
                };
                context.CreateSet<Student>().Attach(student);
            }

            context.SaveChanges();

            //为学生加入学生信息
            context = ContextUtils.CreateContext(dataSource);
            var studentList = context.CreateSet<Student>().ToList();
            foreach (var student in studentList)
                context.CreateSet<StudentInfo>().Attach(new StudentInfo
                {
                    StudentId = student.StudentId,
                    Background = "普通",
                    Description = "普普通通"
                });

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
            var context = ContextUtils.CreateContext(dataSource);

            context.CreateSet<Student>().Delete(p => p.StudentId > 0);
            context.CreateSet<StudentInfo>().Delete(p => p.StudentInfoId > 0);
        }
    }

    /// <summary>
    ///     测试关联端聚合
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Test(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //查询学生
        var student = context.CreateSet<Student>().Include(p => p.StudentInfo).FirstOrDefault();
        //学生和学生信息都不是空
        Assert.That(student, Is.Not.Null);
        Assert.That(student.StudentInfo, Is.Not.Null);


        //移除了学生
        context.Remove(student);
        context.SaveChanges();

        var studentId = student.StudentId;
        //学生和学生信息都是被删除
        student = context.CreateSet<Student>().FirstOrDefault(p => p.StudentId == studentId);
        var studentInfo = context.CreateSet<StudentInfo>().FirstOrDefault(p => p.StudentId == studentId);
        //都是空
        Assert.That(student, Is.Null);
        Assert.That(studentInfo, Is.Null);

        context = ContextUtils.CreateContext(dataSource);
        //另外一个学生
        student = context.CreateSet<Student>().Include(p => p.StudentInfo).FirstOrDefault();

        //学生和学生信息都不是空
        Assert.That(student, Is.Not.Null);
        Assert.That(student.StudentInfo, Is.Not.Null);

        //新建
        var newStudentInfo = new StudentInfo
        {
            Background = "新普通",
            Description = "新普普通通",
            StudentId = student.StudentId
        };
        context.Attach(newStudentInfo);

        //替换学生信息
        student.StudentInfo = newStudentInfo;
        context.SaveChanges();

        student = context.CreateSet<Student>().Include(p => p.StudentInfo).FirstOrDefault();

        //学生和学生信息都不是空
        Assert.That(student, Is.Not.Null);
        Assert.That(student.StudentInfo, Is.Not.Null);
        Assert.That(student.StudentInfo.Background, Is.EqualTo("新普通"));
        Assert.That(student.StudentInfo.Description, Is.EqualTo("新普普通通"));

        //查询此时的学生信息
        var infos = context.CreateSet<StudentInfo>().ToList();
        //应只有一条
        long count = infos.Count(p => p.StudentId == student.StudentId);
        Assert.That(count, Is.EqualTo(1));
        //并且没有其他被解除关系的
        count = infos.Count(p => p.StudentId == 0);
        //是0条
        Assert.That(count, Is.EqualTo(0));
    }
}