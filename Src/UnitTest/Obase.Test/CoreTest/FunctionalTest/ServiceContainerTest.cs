using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Obase.Core.DependencyInjection;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Functional.DependencyInjection;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     依赖注入容器、作用域与作用域内单例的测试
///     测试用的服务在ConfigSetUp中注册,本测试只从已建好的容器中解析,按数据源逐个验证
///     同一主题的场景合并在一个用例内断言,避免用例过于零碎
///     与Java版ServiceContainerTest的用例一一对应,两版用例数必须保持一致
/// </summary>
[TestFixture]
public class ServiceContainerTest
{
    /// <summary>
    ///     获取当前数据源对应的服务容器
    /// </summary>
    private static ServiceContainer GetContainer(EDataSource dataSource)
    {
        return ServiceContainerInstance.Current.GetServiceContainer(
            TestCaseSourceConfigurationManager.GetDataSourceContextType(dataSource));
    }

    #region 容器解析

    /// <summary>
    ///     解析已注册的服务:单例同一实例、末次注册胜出、重复注册去重、List&lt;T&gt;按注册顺序返回、构造函数注入、未注册值类型返回默认值
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Container_ResolvesRegisteredServices(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        //单例:同一实例,且同一服务类型多次注册时以最后一次注册为准
        var first = container.GetService<ContainerTestServices.IService>();
        var second = container.GetService<ContainerTestServices.IService>();
        Assert.That(first, Is.Not.Null);
        Assert.That(first, Is.SameAs(second));
        Assert.That(first.Code, Is.EqualTo("B"));

        //用List<T>获取某服务类型的全部实现 顺序为注册顺序
        var services = container.GetService<List<ContainerTestServices.IService>>();
        Assert.That(services, Is.Not.Null);
        Assert.That(services.Count, Is.EqualTo(2));
        Assert.That(string.Join(",", services.Select(p => p.Code)), Is.EqualTo("A,B"));

        //同一服务类型与实现类型重复注册时只保留一个定义
        var dependencies = container.GetService<List<ContainerTestServices.Dependency>>();
        Assert.That(dependencies, Is.Not.Null);
        Assert.That(dependencies.Count, Is.EqualTo(1));
        Assert.That(container.GetService<ContainerTestServices.Dependency>(), Is.SameAs(dependencies[0]));

        //构造函数注入:构造函数参数从容器中解析
        var needsDependency = container.GetService<ContainerTestServices.NeedsDependency>();
        Assert.That(needsDependency, Is.Not.Null);
        Assert.That(needsDependency.Dependency, Is.SameAs(container.GetService<ContainerTestServices.Dependency>()));

        //获取未注册的值类型返回默认值而不是抛出异常
        Assert.That(container.GetService<int>(), Is.EqualTo(0));
        Assert.That(container.GetService<DateTime>(), Is.EqualTo(default(DateTime)));
    }

    /// <summary>
    ///     由委托创建服务实例,以接口作为服务类型时同样可以解析,且单例复用同一实例
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Container_CreatesInstancesFromFactories(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        var first = container.GetService<ContainerTestServices.IFactoryOnly>();
        var second = container.GetService<ContainerTestServices.IFactoryOnly>();

        Assert.That(first, Is.Not.Null);
        Assert.That(first, Is.InstanceOf<ContainerTestServices.FactoryOnly>());
        Assert.That(first, Is.SameAs(second));
    }

    /// <summary>
    ///     开放泛型按请求的闭合类型解析,用List&lt;T&gt;获取开放泛型的全部实现
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Container_ResolvesGenericServices(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        var stringRepository = container.GetService<ContainerTestServices.IRepository<string>>();
        var intRepository = container.GetService<ContainerTestServices.IRepository<int>>();

        Assert.That(stringRepository, Is.Not.Null);
        Assert.That(intRepository, Is.Not.Null);
        Assert.That(stringRepository, Is.InstanceOf<ContainerTestServices.Repository<string>>());
        Assert.That(intRepository, Is.InstanceOf<ContainerTestServices.Repository<int>>());
        Assert.That(stringRepository, Is.Not.SameAs(intRepository));

        var repositories = container.GetService<List<ContainerTestServices.IRepository<string>>>();
        Assert.That(repositories, Is.Not.Null);
        Assert.That(repositories.Count, Is.EqualTo(1));
        Assert.That(repositories[0], Is.InstanceOf<ContainerTestServices.Repository<string>>());
    }

    /// <summary>
    ///     解析失败的各种情形:构造函数参数无法解析、循环依赖、单例依赖作用域内单例、重复建造
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Container_ReportsResolutionErrors(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        //构造函数参数无法解析时抛出ArgumentException
        Assert.Throws<ArgumentException>(() => container.GetService<ContainerTestServices.NeedsUnregistered>());
        Assert.Throws<ArgumentException>(() => container.GetService(typeof(ContainerTestServices.NeedsUnregistered)));

        //循环依赖抛出异常 且异常信息包含解析链
        var circular = Assert.Throws<InvalidOperationException>(() =>
            container.GetService<ContainerTestServices.CyclicA>());
        Assert.That(circular.Message, Does.Contain("循环依赖"));
        Assert.That(circular.Message, Does.Contain("CyclicA"));
        Assert.That(circular.Message, Does.Contain("CyclicB"));

        //单例依赖作用域内单例时解析失败 异常信息包含解析链
        var scoped = Assert.Throws<InvalidOperationException>(() =>
            container.GetService<ContainerTestServices.SingletonNeedsScoped>());
        Assert.That(scoped.Message, Does.Contain("单例服务不能依赖作用域内单例的服务"));
        Assert.That(scoped.Message, Does.Contain("SingletonNeedsScoped"));
        Assert.That(scoped.Message, Does.Contain("ScopedOnly"));

        //同一上下文的容器已建造时 再次建造会抛出InvalidOperationException
        var builder = ObaseDependencyInjection.CreateBuilder(
            TestCaseSourceConfigurationManager.GetDataSourceContextType(dataSource));
        builder.AddSingleton<ContainerTestServices.IService, ContainerTestServices.ServiceA>();
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    /// <summary>
    ///     并发解析同一单例只创建一次
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Container_ResolvesSingletonConcurrently(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);
        //单例可能在别的用例里已经创建过 这里只验证并发解析不会重复创建
        container.GetService<ContainerTestServices.SlowSingleton>();
        var createdBefore = ContainerTestServices.SlowSingleton.Created;

        var instances = new ConcurrentBag<ContainerTestServices.SlowSingleton>();
        var threads = Enumerable.Range(0, 16)
            .Select(_ => new Thread(() =>
                instances.Add(container.GetService<ContainerTestServices.SlowSingleton>()))).ToArray();
        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();

        Assert.That(instances.Count, Is.EqualTo(16));
        Assert.That(instances.Distinct().Count(), Is.EqualTo(1));
        Assert.That(ContainerTestServices.SlowSingleton.Created, Is.EqualTo(createdBefore));
    }

    #endregion

    #region 作用域

    /// <summary>
    ///     作用域与单例共享实例;作用域内解析的多例归作用域并随作用域释放,容器解析的多例不受影响;兄弟作用域互不影响
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scope_OwnsTransientsAndIsolatesSiblingScopes(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        //单例在容器与作用域之间共享同一实例 作用域释放不影响单例
        var singleton = container.GetService<ContainerTestServices.IService>();
        using (var scope = container.CreateScope())
        {
            Assert.That(scope.GetService<ContainerTestServices.IService>(), Is.SameAs(singleton));
        }

        Assert.That(container.GetService<ContainerTestServices.IService>(), Is.SameAs(singleton));

        //作用域内解析的多例归作用域 随作用域释放
        ContainerTestServices.Tracked.Reset();
        using (var scope = container.CreateScope())
        {
            var first = scope.GetService<ContainerTestServices.Tracked>();
            var second = scope.GetService<ContainerTestServices.Tracked>();

            Assert.That(first, Is.Not.SameAs(second));
            Assert.That(ContainerTestServices.Tracked.Created, Is.EqualTo(2));
            Assert.That(ContainerTestServices.Tracked.Disposed, Is.EqualTo(0));
        }

        Assert.That(ContainerTestServices.Tracked.Disposed, Is.EqualTo(2));

        //从容器解析的多例不受作用域释放影响
        ContainerTestServices.Tracked.Reset();
        var fromContainer = container.GetService<ContainerTestServices.Tracked>();
        using (var scope = container.CreateScope())
        {
            scope.GetService<ContainerTestServices.Tracked>();
        }

        Assert.That(fromContainer, Is.Not.Null);
        Assert.That(ContainerTestServices.Tracked.Created, Is.EqualTo(2));
        Assert.That(ContainerTestServices.Tracked.Disposed, Is.EqualTo(1));

        //兄弟作用域互不影响:释放其中一个 另一个仍然可用
        ContainerTestServices.Tracked.Reset();
        var firstScope = container.CreateScope();
        var secondScope = container.CreateScope();
        firstScope.GetService<ContainerTestServices.Tracked>();
        secondScope.GetService<ContainerTestServices.Tracked>();

        firstScope.Dispose();

        Assert.That(firstScope.IsDisposed, Is.True);
        Assert.That(secondScope.IsDisposed, Is.False);
        Assert.That(ContainerTestServices.Tracked.Disposed, Is.EqualTo(1));

        Assert.That(secondScope.GetService<ContainerTestServices.Tracked>(), Is.Not.Null);
        secondScope.Dispose();
        //第二个作用域里一共创建了两个实例(之前的一个与刚才的一个) 一并释放
        Assert.That(ContainerTestServices.Tracked.Disposed, Is.EqualTo(3));

        //释放父作用域时 从容器直接创建的兄弟作用域不受影响
        ContainerTestServices.Tracked.Reset();
        var parent = container.CreateScope();
        var child = parent.CreateScope();
        var sibling = container.CreateScope();
        child.GetService<ContainerTestServices.Tracked>();
        sibling.GetService<ContainerTestServices.Tracked>();

        parent.Dispose();

        Assert.That(child.IsDisposed, Is.True);
        Assert.That(sibling.IsDisposed, Is.False);
        Assert.That(ContainerTestServices.Tracked.Disposed, Is.EqualTo(1));

        sibling.Dispose();
        Assert.That(ContainerTestServices.Tracked.Disposed, Is.EqualTo(2));

        //作用域内大量解析多例不会在容器中累积
        const int count = 20000;
        ContainerTestServices.Tracked.Reset();
        using (var scope = container.CreateScope())
        {
            for (var i = 0; i < count; i++) scope.GetService<ContainerTestServices.Tracked>();

            Assert.That(ContainerTestServices.Tracked.Created, Is.EqualTo(count));
            Assert.That(ContainerTestServices.Tracked.Disposed, Is.EqualTo(0));
        }

        //作用域释放后全部释放 容器不持有这些实例
        Assert.That(ContainerTestServices.Tracked.Disposed, Is.EqualTo(count));
    }

    /// <summary>
    ///     作用域按创建顺序的逆序释放;嵌套作用域由深到浅释放;已释放的作用域不能再解析且重复释放不抛异常
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scope_DisposesInNestingOrderAndRejectsClosedScope(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        //作用域内按创建顺序的逆序释放(解析B时会先创建它的依赖A)
        ContainerTestServices.DisposalLog.Reset();
        using (var scope = container.CreateScope())
        {
            scope.GetService<ContainerTestServices.LoggedB>();
        }

        Assert.That(ContainerTestServices.DisposalLog.Joined, Is.EqualTo("B,A"));

        //嵌套作用域:释放父作用域时先释放仍然存活的子作用域
        ContainerTestServices.DisposalLog.Reset();
        var outer = container.CreateScope();
        outer.GetService<ContainerTestServices.OuterItem>();
        var inner = outer.CreateScope();
        inner.GetService<ContainerTestServices.InnerItem>();

        outer.Dispose();

        Assert.That(inner.IsDisposed, Is.True);
        Assert.That(outer.IsDisposed, Is.True);
        Assert.That(ContainerTestServices.DisposalLog.Joined, Is.EqualTo("inner,outer"));

        //多层嵌套:由深到浅依次释放
        ContainerTestServices.DisposalLog.Reset();
        var grandOuter = container.CreateScope();
        grandOuter.GetService<ContainerTestServices.OuterItem>();
        var grandChild = grandOuter.CreateScope();
        grandChild.GetService<ContainerTestServices.InnerItem>();
        var grandGrandchild = grandChild.CreateScope();
        grandGrandchild.GetService<ContainerTestServices.GrandchildItem>();

        grandOuter.Dispose();

        Assert.That(grandGrandchild.IsDisposed, Is.True);
        Assert.That(grandChild.IsDisposed, Is.True);
        //由深到浅 每一层先释放自己的实例
        Assert.That(ContainerTestServices.DisposalLog.Joined, Is.EqualTo("grandchild,inner,outer"));

        //已释放的作用域再解析抛异常 重复释放不抛异常
        var closed = container.CreateScope();
        closed.GetService<ContainerTestServices.LoggedA>();
        closed.Dispose();
        closed.Dispose();

        Assert.That(closed.IsDisposed, Is.True);
        Assert.Throws<ObjectDisposedException>(() => closed.GetService<ContainerTestServices.LoggedA>());
        Assert.Throws<ObjectDisposedException>(() => closed.GetService(typeof(ContainerTestServices.LoggedA)));

        //父作用域释放后 已释放的子作用域不能再解析
        var parentScope = container.CreateScope();
        var childScope = parentScope.CreateScope();
        childScope.GetService<ContainerTestServices.LoggedA>();

        parentScope.Dispose();

        Assert.That(childScope.IsDisposed, Is.True);
        Assert.Throws<ObjectDisposedException>(() => childScope.GetService<ContainerTestServices.LoggedA>());
    }

    #endregion

    #region 作用域内单例

    /// <summary>
    ///     作用域内单例:同一作用域内是同一实例、不同作用域各自持有、从容器获取时归容器所有、List&lt;T&gt;在同一作用域内复用,且先于它的多例依赖释放
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scoped_SingletonLifecycle(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        //同一作用域内是同一个实例且只创建一次,List<T>复用同一批实例
        ContainerTestServices.ScopedOnly.Created = 0;
        using (var scope = container.CreateScope())
        {
            var first = scope.GetService<ContainerTestServices.ScopedOnly>();
            var second = scope.GetService<ContainerTestServices.ScopedOnly>();
            var all = scope.GetService<List<ContainerTestServices.ScopedOnly>>();

            Assert.That(first, Is.SameAs(second));
            Assert.That(all.Count, Is.EqualTo(1));
            Assert.That(all[0], Is.SameAs(first));
            Assert.That(ContainerTestServices.ScopedOnly.Created, Is.EqualTo(1));
        }

        //不同作用域各自持有自己的实例
        ContainerTestServices.ScopedOnly.Created = 0;
        using (var firstScope = container.CreateScope())
        using (var secondScope = container.CreateScope())
        {
            Assert.That(firstScope.GetService<ContainerTestServices.ScopedOnly>(),
                Is.Not.SameAs(secondScope.GetService<ContainerTestServices.ScopedOnly>()));
            Assert.That(ContainerTestServices.ScopedOnly.Created, Is.EqualTo(2));
        }

        //从容器获取时归容器所有 作用域释放不影响它
        ContainerTestServices.ScopedDisposable.Disposed = 0;
        var fromContainer = container.GetService<ContainerTestServices.ScopedDisposable>();
        Assert.That(container.GetService<ContainerTestServices.ScopedDisposable>(), Is.SameAs(fromContainer));

        using (var scope = container.CreateScope())
        {
            Assert.That(scope.GetService<ContainerTestServices.ScopedDisposable>(), Is.Not.SameAs(fromContainer));
        }

        //只释放了作用域里的那一个 容器的那个实例仍然存活
        Assert.That(ContainerTestServices.ScopedDisposable.Disposed, Is.EqualTo(1));
        Assert.That(container.GetService<ContainerTestServices.ScopedDisposable>(), Is.SameAs(fromContainer));

        //随作用域释放 且先于它的多例依赖释放
        ContainerTestServices.DisposalLog.Reset();
        using (var scope = container.CreateScope())
        {
            var service = scope.GetService<ContainerTestServices.ScopedWithTransient>();
            Assert.That(service.Dependency, Is.Not.Null);
        }

        //多例依赖先创建 后释放;作用域内单例后创建 先释放
        Assert.That(ContainerTestServices.DisposalLog.Joined, Is.EqualTo("scoped,A"));
    }

    #endregion
}
