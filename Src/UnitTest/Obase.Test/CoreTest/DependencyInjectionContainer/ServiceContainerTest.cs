using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Obase.Core.DependencyInjection;
using Obase.Providers.Sql;
using Obase.Test.Configuration;

namespace Obase.Test.CoreTest.DependencyInjectionContainer;

/// <summary>
///     依赖注入容器、作用域与作用域内单例的测试
///     测试用的服务在ConfigSetUp中注册,本测试只从已建好的容器中解析,按数据源逐个验证
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

    #region 容器基本行为

    /// <summary>
    ///     单例:同一实例,且同一服务类型多次注册时以最后一次注册为准
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Singleton_ReturnsSameInstanceAndLastRegistrationWins(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        var first = container.GetService<ContainerTestServices.IService>();
        var second = container.GetService<ContainerTestServices.IService>();

        Assert.That(first, Is.Not.Null);
        Assert.That(first, Is.SameAs(second));
        Assert.That(first.Code, Is.EqualTo("B"));
    }

    /// <summary>
    ///     用List&lt;T&gt;获取某服务类型的全部实现 顺序为注册顺序
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void GetServices_ReturnsAllImplementationsInRegistrationOrder(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        var services = container.GetService<List<ContainerTestServices.IService>>();

        Assert.That(services, Is.Not.Null);
        Assert.That(services.Count, Is.EqualTo(2));
        Assert.That(string.Join(",", services.Select(p => p.Code)), Is.EqualTo("A,B"));
    }

    /// <summary>
    ///     构造函数注入:构造函数参数从容器中解析
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ConstructorInjection_ResolvesRegisteredDependency(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        var service = container.GetService<ContainerTestServices.NeedsDependency>();
        var dependency = container.GetService<ContainerTestServices.Dependency>();

        Assert.That(service, Is.Not.Null);
        Assert.That(service.Dependency, Is.SameAs(dependency));
    }

    /// <summary>
    ///     构造函数参数无法解析时抛出ArgumentException
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ConstructorInjection_UnresolvableParameterThrows(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        Assert.Throws<ArgumentException>(() => container.GetService<ContainerTestServices.NeedsUnregistered>());
        Assert.Throws<ArgumentException>(() => container.GetService(typeof(ContainerTestServices.NeedsUnregistered)));
    }

    /// <summary>
    ///     由委托创建服务实例
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ImplementationFactory_CreatesInstance(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        var first = container.GetService<ContainerTestServices.IFactoryOnly>();
        var second = container.GetService<ContainerTestServices.IFactoryOnly>();

        Assert.That(first, Is.Not.Null);
        Assert.That(first, Is.InstanceOf<ContainerTestServices.FactoryOnly>());
        Assert.That(first, Is.SameAs(second));
    }

    /// <summary>
    ///     同一上下文的容器已建造时 再次建造会抛出InvalidOperationException
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void DuplicateBuildThrows(EDataSource dataSource)
    {
        var builder = ObaseDependencyInjection.CreateBuilder(
            TestCaseSourceConfigurationManager.GetDataSourceContextType(dataSource));
        builder.AddSingleton<ContainerTestServices.IService, ContainerTestServices.ServiceA>();

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    /// <summary>
    ///     同一服务类型与实现类型重复注册时只保留一个定义
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void DuplicateRegistration_KeepsSingleEntry(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        var services = container.GetService<List<ContainerTestServices.Dependency>>();

        Assert.That(services, Is.Not.Null);
        Assert.That(services.Count, Is.EqualTo(1));
        Assert.That(container.GetService<ContainerTestServices.Dependency>(), Is.SameAs(services[0]));
    }

    /// <summary>
    ///     开放泛型按请求的闭合类型解析
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void OpenGeneric_ResolvedPerClosedType(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        var stringRepository = container.GetService<ContainerTestServices.IRepository<string>>();
        var intRepository = container.GetService<ContainerTestServices.IRepository<int>>();

        Assert.That(stringRepository, Is.Not.Null);
        Assert.That(intRepository, Is.Not.Null);
        Assert.That(stringRepository, Is.InstanceOf<ContainerTestServices.Repository<string>>());
        Assert.That(intRepository, Is.InstanceOf<ContainerTestServices.Repository<int>>());
        Assert.That(stringRepository, Is.Not.SameAs(intRepository));
    }

    /// <summary>
    ///     用List&lt;T&gt;获取开放泛型的全部实现
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ListOfOpenGeneric_Resolved(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        var repositories = container.GetService<List<ContainerTestServices.IRepository<string>>>();

        Assert.That(repositories, Is.Not.Null);
        Assert.That(repositories.Count, Is.EqualTo(1));
        Assert.That(repositories[0], Is.InstanceOf<ContainerTestServices.Repository<string>>());
    }

    #endregion

    #region 健壮性

    /// <summary>
    ///     获取未注册的值类型返回默认值而不是抛出异常
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void GetServiceOfValueType_ReturnsDefault(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        Assert.That(container.GetService<int>(), Is.EqualTo(0));
        Assert.That(container.GetService<DateTime>(), Is.EqualTo(default(DateTime)));
        Assert.That(container.GetService<ContainerTestServices.IService>(), Is.Not.Null);
    }

    /// <summary>
    ///     以接口作为服务类型、用委托注册时可以正常解析
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void FactoryRegistrationWithInterfaceServiceType_Succeeds(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        var service = container.GetService<ContainerTestServices.IFactoryOnly>();

        Assert.That(service, Is.Not.Null);
        Assert.That(service, Is.InstanceOf<ContainerTestServices.FactoryOnly>());
    }

    /// <summary>
    ///     循环依赖抛出异常 且异常信息包含解析链
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void CircularDependencyThrowsWithResolutionChain(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            container.GetService<ContainerTestServices.CyclicA>());

        Assert.That(exception.Message, Does.Contain("循环依赖"));
        Assert.That(exception.Message, Does.Contain("CyclicA"));
        Assert.That(exception.Message, Does.Contain("CyclicB"));
    }

    /// <summary>
    ///     并发解析同一单例只创建一次
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ConcurrentSingletonResolution_CreatesInstanceOnce(EDataSource dataSource)
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
    ///     单例在容器与作用域之间共享同一实例
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scope_SingletonSharedWithContainer(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        var fromContainer = container.GetService<ContainerTestServices.IService>();
        using (var scope = container.CreateScope())
        {
            Assert.That(scope.GetService<ContainerTestServices.IService>(), Is.SameAs(fromContainer));
        }

        //作用域释放不影响单例
        Assert.That(container.GetService<ContainerTestServices.IService>(), Is.SameAs(fromContainer));
    }

    /// <summary>
    ///     作用域内解析的多例归作用域 随作用域释放
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scope_TransientOwnedAndDisposedByScope(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);
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
    }

    /// <summary>
    ///     从容器解析的多例不受作用域释放影响
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scope_RootTransientNotAffectedByScopeDispose(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);
        ContainerTestServices.Tracked.Reset();

        var fromContainer = container.GetService<ContainerTestServices.Tracked>();
        using (var scope = container.CreateScope())
        {
            scope.GetService<ContainerTestServices.Tracked>();
        }

        //只有作用域里的那一个被释放 容器里的仍归容器
        Assert.That(fromContainer, Is.Not.Null);
        Assert.That(ContainerTestServices.Tracked.Created, Is.EqualTo(2));
        Assert.That(ContainerTestServices.Tracked.Disposed, Is.EqualTo(1));
    }

    /// <summary>
    ///     作用域内按创建顺序的逆序释放
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scope_DisposesInReverseCreationOrder(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);
        ContainerTestServices.DisposalLog.Reset();

        using (var scope = container.CreateScope())
        {
            //解析B时会先创建它的依赖A
            scope.GetService<ContainerTestServices.LoggedB>();
        }

        Assert.That(ContainerTestServices.DisposalLog.Joined, Is.EqualTo("B,A"));
    }

    /// <summary>
    ///     嵌套作用域:释放父作用域时先释放仍然存活的子作用域
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scope_NestedScopeDisposedFirst(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);
        ContainerTestServices.DisposalLog.Reset();

        var outer = container.CreateScope();
        outer.GetService<ContainerTestServices.OuterItem>();
        var inner = outer.CreateScope();
        inner.GetService<ContainerTestServices.InnerItem>();

        //只释放父作用域 子作用域应当先被释放
        outer.Dispose();

        Assert.That(inner.IsDisposed, Is.True);
        Assert.That(outer.IsDisposed, Is.True);
        Assert.That(ContainerTestServices.DisposalLog.Joined, Is.EqualTo("inner,outer"));
    }

    /// <summary>
    ///     多层嵌套:由深到浅依次释放
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scope_GrandchildScopesDisposedNewestFirst(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);
        ContainerTestServices.DisposalLog.Reset();

        var outer = container.CreateScope();
        outer.GetService<ContainerTestServices.OuterItem>();
        var child = outer.CreateScope();
        child.GetService<ContainerTestServices.InnerItem>();
        var grandchild = child.CreateScope();
        grandchild.GetService<ContainerTestServices.GrandchildItem>();

        outer.Dispose();

        Assert.That(grandchild.IsDisposed, Is.True);
        Assert.That(child.IsDisposed, Is.True);
        //由深到浅 每一层先释放自己的实例
        Assert.That(ContainerTestServices.DisposalLog.Joined, Is.EqualTo("grandchild,inner,outer"));
    }

    /// <summary>
    ///     兄弟作用域互不影响:释放其中一个 另一个仍然可用
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scope_SiblingScopesAreIndependent(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);
        ContainerTestServices.Tracked.Reset();

        var first = container.CreateScope();
        var second = container.CreateScope();
        first.GetService<ContainerTestServices.Tracked>();
        second.GetService<ContainerTestServices.Tracked>();

        first.Dispose();

        Assert.That(first.IsDisposed, Is.True);
        Assert.That(second.IsDisposed, Is.False);
        //只释放第一个作用域里的实例
        Assert.That(ContainerTestServices.Tracked.Disposed, Is.EqualTo(1));

        //第二个作用域仍然可以解析 关闭它时释放自己的实例
        Assert.That(second.GetService<ContainerTestServices.Tracked>(), Is.Not.Null);
        second.Dispose();
        //第二个作用域里一共创建了两个实例(之前的一个与刚才的一个) 一并释放
        Assert.That(ContainerTestServices.Tracked.Disposed, Is.EqualTo(3));
    }

    /// <summary>
    ///     释放父作用域时 从容器直接创建的兄弟作用域不受影响
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scope_DisposingParentKeepsSiblingUsable(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);
        ContainerTestServices.Tracked.Reset();

        var parent = container.CreateScope();
        var child = parent.CreateScope();
        var sibling = container.CreateScope();
        child.GetService<ContainerTestServices.Tracked>();
        sibling.GetService<ContainerTestServices.Tracked>();

        parent.Dispose();

        Assert.That(child.IsDisposed, Is.True);
        Assert.That(sibling.IsDisposed, Is.False);
        //只有子作用域里的实例被释放
        Assert.That(ContainerTestServices.Tracked.Disposed, Is.EqualTo(1));

        sibling.Dispose();
        Assert.That(ContainerTestServices.Tracked.Disposed, Is.EqualTo(2));
    }

    /// <summary>
    ///     已释放的作用域再解析抛异常 重复释放不抛异常
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scope_ClosedScopeThrowsAndDisposeIsIdempotent(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        var scope = container.CreateScope();
        scope.GetService<ContainerTestServices.LoggedA>();
        scope.Dispose();
        scope.Dispose();

        Assert.That(scope.IsDisposed, Is.True);
        Assert.Throws<ObjectDisposedException>(() => scope.GetService<ContainerTestServices.LoggedA>());
        Assert.Throws<ObjectDisposedException>(() => scope.GetService(typeof(ContainerTestServices.LoggedA)));
    }

    /// <summary>
    ///     父作用域释放后 已释放的子作用域不能再解析
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scope_ResolvingInClosedChildScopeThrows(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        var parent = container.CreateScope();
        var child = parent.CreateScope();
        child.GetService<ContainerTestServices.LoggedA>();

        parent.Dispose();

        Assert.That(child.IsDisposed, Is.True);
        Assert.Throws<ObjectDisposedException>(() => child.GetService<ContainerTestServices.LoggedA>());
    }

    /// <summary>
    ///     作用域内大量解析多例不会在容器中累积
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scope_ResolvingManyTransientsDoesNotAccumulate(EDataSource dataSource)
    {
        const int count = 20000;
        var container = GetContainer(dataSource);
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

    #endregion

    #region 作用域内单例

    /// <summary>
    ///     作用域内单例:同一作用域内是同一个实例且只创建一次
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scoped_SameInstanceWithinScope(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);
        ContainerTestServices.ScopedOnly.Created = 0;

        using (var scope = container.CreateScope())
        {
            var first = scope.GetService<ContainerTestServices.ScopedOnly>();
            var second = scope.GetService<ContainerTestServices.ScopedOnly>();

            Assert.That(first, Is.SameAs(second));
            Assert.That(ContainerTestServices.ScopedOnly.Created, Is.EqualTo(1));
        }
    }

    /// <summary>
    ///     作用域内单例:不同作用域各自持有自己的实例
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scoped_DifferentScopesGetDifferentInstances(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);
        ContainerTestServices.ScopedOnly.Created = 0;

        using (var first = container.CreateScope())
        using (var second = container.CreateScope())
        {
            Assert.That(first.GetService<ContainerTestServices.ScopedOnly>(),
                Is.Not.SameAs(second.GetService<ContainerTestServices.ScopedOnly>()));
            Assert.That(ContainerTestServices.ScopedOnly.Created, Is.EqualTo(2));
        }
    }

    /// <summary>
    ///     作用域内单例:从容器获取时归容器所有 作用域释放不影响它
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scoped_FromContainerOwnedByContainer(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);
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
    }

    /// <summary>
    ///     作用域内单例:单例依赖作用域内单例时解析失败 异常信息包含解析链
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scoped_SingletonDependingOnScopedThrows(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            container.GetService<ContainerTestServices.SingletonNeedsScoped>());

        Assert.That(exception.Message, Does.Contain("单例服务不能依赖作用域内单例的服务"));
        Assert.That(exception.Message, Does.Contain("SingletonNeedsScoped"));
        Assert.That(exception.Message, Does.Contain("ScopedOnly"));
    }

    /// <summary>
    ///     作用域内单例:随作用域释放 且先于它的多例依赖释放
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scoped_DisposedAfterItsTransientDependency(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);
        ContainerTestServices.DisposalLog.Reset();

        using (var scope = container.CreateScope())
        {
            var service = scope.GetService<ContainerTestServices.ScopedWithTransient>();
            Assert.That(service.Dependency, Is.Not.Null);
        }

        //多例依赖先创建 后释放;作用域内单例后创建 先释放
        Assert.That(ContainerTestServices.DisposalLog.Joined, Is.EqualTo("scoped,A"));
    }

    /// <summary>
    ///     作用域内单例:用List&lt;T&gt;获取全部实现时在同一作用域内复用同一批实例
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Scoped_GetServicesReusesInstancesWithinScope(EDataSource dataSource)
    {
        var container = GetContainer(dataSource);
        ContainerTestServices.ScopedOnly.Created = 0;

        using (var scope = container.CreateScope())
        {
            var single = scope.GetService<ContainerTestServices.ScopedOnly>();
            var all = scope.GetService<List<ContainerTestServices.ScopedOnly>>();

            Assert.That(all.Count, Is.EqualTo(1));
            Assert.That(all[0], Is.SameAs(single));
            Assert.That(scope.GetService<ContainerTestServices.ScopedOnly>(), Is.SameAs(single));
            Assert.That(ContainerTestServices.ScopedOnly.Created, Is.EqualTo(1));
        }
    }

    #endregion
}
