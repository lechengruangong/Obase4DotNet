using System.Linq;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Functional.DataError;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     数据错误(关联引用是一对一 但数据是一对多)的关联测试
/// </summary>
[TestFixture]
public class DataErrorTest
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
            context.CreateSet<DataErrorStudent>().Delete(p => p.StudentId >= 0);
            context.CreateSet<DataErrorStudentInfo>().Delete(p => p.StudentInfoId >= 0);

            //加入测试数据
            var stu = new DataErrorStudent
            {
                StudentId = 1,
                Name = "小1"
            };
            //添加3个学生信息
            for (var i = 1; i < 3; i++)
            {
                context.Attach(stu);
                var studentInfo = new DataErrorStudentInfo
                {
                    Background = $"普通{i}",
                    Description = $"普普通通{i}",
                    StudentId = 1,
                    StudentInfoId = i
                };
                context.Attach(studentInfo);
            }

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
            context.CreateSet<DataErrorStudent>().Delete(p => p.StudentId >= 0);
            context.CreateSet<DataErrorStudentInfo>().Delete(p => p.StudentInfoId >= 0);
        }
    }

    /// <summary>
    ///     测试错误数据
    ///     关联引用是一对一 但数据是一对多
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Test(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //加载一对一关联
        var student = context.CreateSet<DataErrorStudent>().Include(p => p.StudentInfo).FirstOrDefault();
        //此时军不为空
        Assert.That(student, Is.Not.Null);
        Assert.That(student.StudentInfo, Is.Not.Null);
        //随意修改一个属性
        student.Name = "小X";
        //保存
        context.SaveChanges();

        //此时 DataErrorStudentInfo中StudentId为1的 仍然有多个 没有被关联解除
        var count = context.CreateSet<DataErrorStudentInfo>().Count(p => p.StudentId == 1);
        //仍然有两个
        Assert.That(count, Is.EqualTo(2));
    }
}