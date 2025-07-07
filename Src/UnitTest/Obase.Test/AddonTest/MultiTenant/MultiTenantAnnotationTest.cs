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
///     用标注配置的 定义了多租户字段的测试
/// </summary>
[TestFixture]
public class MultiTenantAnnotationTest
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
            context.CreateSet<MultiTenantSchoolAnnotation>().Delete(p => p.SchoolId > 0);
            context.CreateSet<MultiTenantTeacherAnnotation>().Delete(p => p.TeacherId > 0);
            //加入新对象 保存的对象都是第0个用户的
            var school = new MultiTenantSchoolAnnotation
            {
                Createtime = DateTime.Now,
                EstablishmentTime = DateTime.Parse("1999-12-31 23:59:59"),
                IsPrime = false,
                Name = "第X某某学校",
                SchoolType = (ESchoolType)new Random((int)DateTime.Now.Ticks).Next(3),
                //此处赋一个错误值 会被ITenantIdReader或者委托的返回值覆盖
                MultiTenantId = Guid.Empty
            };
            var teacher = new MultiTenantTeacherAnnotation { School = school, Name = "某老师" };
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
            context.CreateSet<MultiTenantSchoolAnnotation>().Delete(p => p.SchoolId > 0);
            context.CreateSet<MultiTenantTeacherAnnotation>().Delete(p => p.TeacherId > 0);
        }
    }

    /// <summary>
    ///     简单查询测试
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void QueryTest(EDataSource dataSource)
    {
        //设置当前用户为第0个用户
        TenantIdCenter.CurrentUserIndex = 0;
        var context = ContextUtils.CreateAddonContext(dataSource);
        //查询时会将读取器的返回值作为附加条件 即此时查询的是第0个用户的
        var teacher = context.CreateSet<MultiTenantTeacherAnnotation>().Include(p => p.School).FirstOrDefault();
        //校验取出的值
        Assert.That(teacher, Is.Not.Null);
        Assert.That(TenantIdCenter.TenantIds[0], Is.EqualTo(teacher.MultiTenantId));
        Assert.That(teacher.School, Is.Not.Null);
        Assert.That(TenantIdCenter.TenantIds[0], Is.EqualTo(teacher.School.MultiTenantId));

        //设置当前用户为第1个用户
        TenantIdCenter.CurrentUserIndex = 1;
        //此时查询的是第1个用户的值
        context = ContextUtils.CreateAddonContext(dataSource);
        //此处应无法查询出对象
        teacher = context.CreateSet<MultiTenantTeacherAnnotation>().Include(p => p.School).FirstOrDefault();
        //无法查出
        Assert.That(teacher, Is.Null);
    }
}