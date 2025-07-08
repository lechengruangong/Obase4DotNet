using System;
using System.Linq;
using Obase.AddonTest.Domain.Annotation;
using Obase.AddonTest.Domain.MultiTenant;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Service;

namespace Obase.Test.AddonTest.MultiTenant;

/// <summary>
///     用代码配置的 未定义了多租户字段的测试
/// </summary>
[TestFixture]
public class MultiTenantNoDefTest
{
    /// <summary>
    ///     构造实例 为上下文赋值
    /// </summary>
    [OneTimeSetUp]
    public void SetUp()
    {
        //设置当前用户为第0个用户
        TenantIdCenter.CurrentUserIndex = 0;

        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateAddonContext(dataSource);

            //销毁所有可能的冗余对象
            context.CreateSet<MultiTenantSchoolNoDef>().Delete(p => p.SchoolId > 0);
            context.CreateSet<MultiTenantTeacherNoDef>().Delete(p => p.TeacherId > 0);
            //加入新对象 保存的对象都是第0个用户的
            var school = new MultiTenantSchoolNoDef
            {
                Createtime = DateTime.Now,
                EstablishmentTime = DateTime.Parse("1999-12-31 23:59:59"),
                IsPrime = false,
                Name = "第X某某学校",
                SchoolType = (ESchoolType)new Random((int)DateTime.Now.Ticks).Next(3)
            };
            var teacher = new MultiTenantTeacherNoDef { School = school, Name = "某老师" };
            context.Attach(school);
            context.Attach(teacher);

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
            //销毁所有可能的冗余对象
            context.CreateSet<MultiTenantSchoolNoDef>().Delete(p => p.SchoolId > 0);
            context.CreateSet<MultiTenantTeacherNoDef>().Delete(p => p.TeacherId > 0);
        }
    }

    /// <summary>
    ///     简单查询
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void QueryTest(EDataSource dataSource)
    {
        //设置当前用户为第0个用户
        TenantIdCenter.CurrentUserIndex = 0;
        var context = ContextUtils.CreateAddonContext(dataSource);
        //查询时会将读取器的返回值作为附加条件 即此时查询的是第0个用户的
        var teacher = context.CreateSet<MultiTenantTeacherNoDef>().Include(p => p.School).FirstOrDefault();
        //校验取出的值 这里没有定义此属性 
        Assert.That(teacher, Is.Not.Null);
        Assert.That(teacher.School, Is.Not.Null);

        //设置当前用户为第1个用户
        TenantIdCenter.CurrentUserIndex = 1;
        context = ContextUtils.CreateAddonContext(dataSource);

        //查询时会将读取器的返回值作为附加条件 此处应无法查询出对象
        teacher = context.CreateSet<MultiTenantTeacherNoDef>().Include(p => p.School).FirstOrDefault();
        //校验取出的值
        Assert.That(teacher, Is.Null);

        //设置当前租户ID为全局ID
        TenantIdCenter.CurrentUserIndex = 2;

        var gTeacher = new MultiTenantTeacherNoDef { Name = "某老师G" };
        context.Attach(gTeacher);

        context.SaveChanges();

        //设置当前用户为第0个用户
        TenantIdCenter.CurrentUserIndex = 0;
        //MultiTenantTeacher启用了全局ID 会查出刚才保存的新的教师
        var list = context.CreateSet<MultiTenantTeacherNoDef>().ToList();
        //校验取出的值
        Assert.That(list, Is.Not.Null);
        Assert.That(list.Count, Is.EqualTo(2));
    }
}