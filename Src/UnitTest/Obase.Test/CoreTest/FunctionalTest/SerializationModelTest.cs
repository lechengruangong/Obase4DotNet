using System;
using System.Linq;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Functional.Serialization;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     有序列化模型的序列划测试
/// </summary>
[TestFixture]
public class SerializationModelTest
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

            //销毁所有旧对象
            context.CreateSet<Domain.Functional.Serialization.Service>().Delete(p => p.Code != "");

            //添加一个服务 Code为Simple
            var serviceSimple = new Domain.Functional.Serialization.Service
            {
                Code = "Simple",
                Route = new Route("*/Get", EAction.Pass),
                SubRoute = [new Route("*/Delete", EAction.Reject), new Route("*/Patch", EAction.Drop)],
                Identity = new Identity(Guid.NewGuid(), DateTime.Now, "Admin")
            };
            //附加
            context.Attach(serviceSimple);

            //添加一个服务 Code为Nan
            var serviceNull = new Domain.Functional.Serialization.Service
            {
                Code = "Nan"
            };
            //附加
            context.Attach(serviceNull);

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

            //销毁所有旧对象
            context.CreateSet<Domain.Functional.Serialization.Service>().Delete(p => p.Code != "");
        }
    }

    /// <summary>
    ///     测试方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void QueryTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //查找对象Simple
        var service = context.CreateSet<Domain.Functional.Serialization.Service>()
            .FirstOrDefault(p => p.Code == "Simple");
        //可以查询到
        Assert.That(service, Is.Not.Null);
        //检查Route
        Assert.That(service.Route, Is.Not.Null);
        Assert.That(service.Route.Action, Is.EqualTo(EAction.Pass));
        Assert.That(service.Route.Rule, Is.EqualTo("*/Get"));
        Assert.That(service.Route.PalceHolder, Is.Null);
        //检查SubRoute
        Assert.That(service.SubRoute, Is.Not.Null);
        Assert.That(service.SubRoute.Length, Is.EqualTo(2));
        Assert.That(service.SubRoute[0].Action, Is.EqualTo(EAction.Reject));
        Assert.That(service.SubRoute[0].Rule, Is.EqualTo("*/Delete"));
        Assert.That(service.SubRoute[0].PalceHolder, Is.Null);
        Assert.That(service.SubRoute[1].Action, Is.EqualTo(EAction.Drop));
        Assert.That(service.SubRoute[1].Rule, Is.EqualTo("*/Patch"));
        Assert.That(service.SubRoute[1].PalceHolder, Is.Null);
        //检查Identity
        Assert.That(service.Identity, Is.Not.Null);
        Assert.That(service.Identity.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(service.Identity.CreateTime, Is.LessThanOrEqualTo(DateTime.Now));
        Assert.That(service.Identity.Role, Is.EqualTo("Admin"));
        Assert.That(service.Identity.QueryTime, Is.GreaterThanOrEqualTo(service.Identity.CreateTime));

        //查找对象Nan
        service = context.CreateSet<Domain.Functional.Serialization.Service>().FirstOrDefault(p => p.Code == "Nan");
        //可以查询到
        Assert.That(service, Is.Not.Null);
        //检查Route
        Assert.That(service.Route, Is.Null);
        //检查SubRoute 会是一个空集合 表示设置过了
        Assert.That(service.SubRoute, Is.Empty);
        //检查Identity
        Assert.That(service.Identity, Is.Null);
    }
}