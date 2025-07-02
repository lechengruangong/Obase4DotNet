using System;
using System.Collections.Generic;
using Obase.Core.DependencyInjection;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.DependencyInjection;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     依赖注入测试
/// </summary>
[TestFixture]
public class DependencyInjectionTest
{
    /// <summary>
    ///     测试单例的依赖注入
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void TestSingleton(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        var builder = ObaseDependencyInjection.CreateBuilder(context.GetType());
        builder.AddSingleton(typeof(ServiceSa));
        //重复创建会报不能重复创建的InvalidOperationException
        Assert.Throws<InvalidOperationException>(() => builder.Build());
        //获取容器
        var container = ServiceContainerInstance.Current.GetServiceContainer(context.GetType());

        var sA = container.GetService(typeof(ServiceSa));
        //sA 可以取出
        Assert.That(sA.GetType(), Is.EqualTo(typeof(ServiceSa)));

        var sB = container.GetService<ServiceSb>();
        //记录一下sB的时间
        var dateTime = sB.CreateTime;
        //是一样的
        sB = container.GetService<ServiceSb>();
        Assert.That(sB.CreateTime, Is.EqualTo(dateTime));


        var sC = container.GetService<ServiceSc>();
        //创建时间是固定值
        Assert.That(sC.CreateTime, Is.EqualTo(new DateTime(1999, 12, 31)));

        //IService同时注册了B和D 此时获取到的只有D
        var sD = container.GetService<IServiceS>();

        Assert.That(sD.GetType(), Is.EqualTo(typeof(ServiceSd)));

        //可以使用List装载所有的IService
        var iS = container.GetService<List<IServiceS>>();

        //2个 按照注册顺序 第一个是ServiceSB 第二个是ServiceSD
        Assert.That(iS.Count, Is.EqualTo(2));
        Assert.That(iS[0].GetType(), Is.EqualTo(typeof(ServiceSb)));
        Assert.That(iS[1].GetType(), Is.EqualTo(typeof(ServiceSd)));

        //E创建需要依赖D
        var sE = container.GetService<ServiceSe>();
        //D已经注册了 可以创建出来
        Assert.That(sE,Is.Not.Null);

        //F 使用了DateTime作为参数 会报ArgumentException错误 
        Assert.Throws<ArgumentException>(() => container.GetService<ServiceSf>());

        //G 使用了委托构造
        var sG = container.GetService<ServiceSg>();

        Assert.That(sG.CreateTime, Is.EqualTo(new DateTime(2000, 1, 1)));

        //H 自定义的创建方法 且注册在接口下
        var sOh = container.GetService<IServiceSo>();
        Assert.That(sOh.GetType(), Is.EqualTo(typeof(ServiceSh)));
        Assert.That(sOh.CreateTime, Is.EqualTo(new DateTime(1999, 1, 1)));
    }

    /// <summary>
    ///     测试多例的依赖注入
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void TestTransient(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        var builder = ObaseDependencyInjection.CreateBuilder(context.GetType());
        builder.AddSingleton(typeof(ServiceTa));
        //重复创建会报不能重复创建的InvalidOperationException
        Assert.Throws<InvalidOperationException>(() => builder.Build());
        //获取容器
        var container = ServiceContainerInstance.Current.GetServiceContainer(context.GetType());

        var tA = container.GetService(typeof(ServiceTa));
        //tA 可以取出
        Assert.That(tA.GetType(), Is.EqualTo(typeof(ServiceTa)));

        var tB = container.GetService<ServiceTb>();
        //记录一下tB的时间
        var dateTime = tB.CreateTime;
        //是不一样的
        tB = container.GetService<ServiceTb>();
        Assert.That(tB.CreateTime, Is.Not.EqualTo(dateTime));


        var tC = container.GetService<ServiceTc>();
        //创建时间是固定值
        Assert.That(tC.CreateTime, Is.EqualTo(new DateTime(1999, 12, 31)));

        //IService同时注册了B和D 此时获取到的只有D
        var tD = container.GetService<IServiceT>();

        Assert.That(tD.GetType(), Is.EqualTo(typeof(ServiceTd)));

        //可以使用List装载所有的IService
        var iT = container.GetService<List<IServiceT>>();

        //2个 按照注册顺序 第一个是ServiceTB 第二个是ServiceTD
        Assert.That(iT.Count, Is.EqualTo(2));
        Assert.That(iT[0].GetType(), Is.EqualTo(typeof(ServiceTb)));
        Assert.That(iT[1].GetType(), Is.EqualTo(typeof(ServiceTd)));

        //E创建需要依赖D
        var tE = container.GetService<ServiceTe>();
        //D已经注册了 可以创建出来
        Assert.That(tE,Is.Not.Null);

        //F 使用了DateTime作为参数 会报ArgumentException错误 
        Assert.Throws<ArgumentException>(() => container.GetService<ServiceTf>());

        //G 使用了委托构造
        var tG = container.GetService<ServiceSg>();

        Assert.That(tG.CreateTime, Is.EqualTo(new DateTime(2000, 1, 1)));

        //H 自定义的创建方法 且注册在接口下
        var sTh = container.GetService<IServiceTo>();
        Assert.That(sTh.GetType(), Is.EqualTo(typeof(ServiceTh)));
        Assert.That(sTh.CreateTime, Is.EqualTo(new DateTime(1999, 1, 1)));
    }
}