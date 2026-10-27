/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：依赖注入容器、作用域测试用的服务类型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Obase.Test.CoreTest.DependencyInjectionContainer;

/// <summary>
///     依赖注入容器与作用域测试用的服务类型
///     这些服务在ConfigSetUp中注册,测试用例只负责从已建好的容器中解析
///     放在本程序集而不是Obase.Test.Domain,是因为Obase.Test.Domain会被整体注册进对象数据模型
/// </summary>
public static class ContainerTestServices
{
    /// <summary>
    ///     释放顺序记录器
    /// </summary>
    public static class DisposalLog
    {
        /// <summary>
        ///     已记录的释放顺序
        /// </summary>
        public static readonly ConcurrentQueue<string> Entries = new();

        /// <summary>
        ///     记录顺序的拼接结果
        /// </summary>
        public static string Joined => string.Join(",", Entries);

        /// <summary>
        ///     清空记录
        /// </summary>
        public static void Reset()
        {
            while (Entries.TryDequeue(out _))
            {
            }
        }
    }

    /// <summary>
    ///     服务接口
    /// </summary>
    public interface IService
    {
        /// <summary>
        ///     标识
        /// </summary>
        string Code { get; }
    }

    /// <summary>
    ///     服务实现一
    /// </summary>
    public sealed class ServiceA : IService
    {
        /// <inheritdoc />
        public string Code => "A";
    }

    /// <summary>
    ///     服务实现二
    /// </summary>
    public sealed class ServiceB : IService
    {
        /// <inheritdoc />
        public string Code => "B";
    }

    /// <summary>
    ///     用于构造函数注入的依赖
    /// </summary>
    public sealed class Dependency
    {
    }

    /// <summary>
    ///     需要构造函数参数的服务
    /// </summary>
    public sealed class NeedsDependency
    {
        /// <summary>
        ///     初始化需要构造函数参数的服务
        /// </summary>
        /// <param name="dependency">依赖</param>
        public NeedsDependency(Dependency dependency)
        {
            Dependency = dependency;
        }

        /// <summary>
        ///     依赖
        /// </summary>
        public Dependency Dependency { get; }
    }

    /// <summary>
    ///     未注册的接口 用于验证构造函数参数无法解析时的行为
    /// </summary>
    public interface INotRegistered
    {
    }

    /// <summary>
    ///     构造函数依赖未注册接口的服务
    /// </summary>
    public sealed class NeedsUnregistered
    {
        /// <summary>
        ///     初始化构造函数依赖未注册接口的服务
        /// </summary>
        /// <param name="service">未注册的接口</param>
        public NeedsUnregistered(INotRegistered service)
        {
        }
    }

    /// <summary>
    ///     仅由委托注册的接口
    /// </summary>
    public interface IFactoryOnly
    {
    }

    /// <summary>
    ///     仅由委托注册的接口的实现
    /// </summary>
    public sealed class FactoryOnly : IFactoryOnly
    {
    }

    /// <summary>
    ///     循环依赖的一方
    /// </summary>
    public sealed class CyclicA
    {
        /// <summary>
        ///     初始化循环依赖的一方
        /// </summary>
        /// <param name="b">另一方</param>
        public CyclicA(CyclicB b)
        {
        }
    }

    /// <summary>
    ///     循环依赖的另一方
    /// </summary>
    public sealed class CyclicB
    {
        /// <summary>
        ///     初始化循环依赖的另一方
        /// </summary>
        /// <param name="a">一方</param>
        public CyclicB(CyclicA a)
        {
        }
    }

    /// <summary>
    ///     慢速构造的单例 用于并发测试
    /// </summary>
    public sealed class SlowSingleton
    {
        /// <summary>
        ///     创建次数
        /// </summary>
        public static int Created;

        /// <summary>
        ///     初始化慢速构造的单例
        /// </summary>
        public SlowSingleton()
        {
            Interlocked.Increment(ref Created);
            Thread.Sleep(50);
        }
    }

    /// <summary>
    ///     泛型服务接口 用于验证开放泛型注册
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public interface IRepository<T>
    {
    }

    /// <summary>
    ///     泛型服务接口的实现
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public sealed class Repository<T> : IRepository<T>
    {
    }

    /// <summary>
    ///     记录创建与释放次数的可释放服务
    /// </summary>
    public sealed class Tracked : IDisposable
    {
        /// <summary>
        ///     创建次数
        /// </summary>
        public static int Created;

        /// <summary>
        ///     释放次数
        /// </summary>
        public static int Disposed;

        /// <summary>
        ///     初始化记录创建与释放次数的可释放服务
        /// </summary>
        public Tracked()
        {
            Interlocked.Increment(ref Created);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Interlocked.Increment(ref Disposed);
        }

        /// <summary>
        ///     重置计数
        /// </summary>
        public static void Reset()
        {
            Created = 0;
            Disposed = 0;
        }
    }

    /// <summary>
    ///     记录释放顺序的服务 无依赖
    /// </summary>
    public sealed class LoggedA : IDisposable
    {
        /// <inheritdoc />
        public void Dispose()
        {
            DisposalLog.Entries.Enqueue("A");
        }
    }

    /// <summary>
    ///     记录释放顺序的服务 依赖LoggedA
    /// </summary>
    public sealed class LoggedB : IDisposable
    {
        /// <summary>
        ///     初始化记录释放顺序的服务
        /// </summary>
        /// <param name="a">依赖</param>
        public LoggedB(LoggedA a)
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            DisposalLog.Entries.Enqueue("B");
        }
    }

    /// <summary>
    ///     父作用域中的可释放服务
    /// </summary>
    public sealed class OuterItem : IDisposable
    {
        /// <inheritdoc />
        public void Dispose()
        {
            DisposalLog.Entries.Enqueue("outer");
        }
    }

    /// <summary>
    ///     子作用域中的可释放服务
    /// </summary>
    public sealed class InnerItem : IDisposable
    {
        /// <inheritdoc />
        public void Dispose()
        {
            DisposalLog.Entries.Enqueue("inner");
        }
    }

    /// <summary>
    ///     孙作用域中的可释放服务
    /// </summary>
    public sealed class GrandchildItem : IDisposable
    {
        /// <inheritdoc />
        public void Dispose()
        {
            DisposalLog.Entries.Enqueue("grandchild");
        }
    }

    /// <summary>
    ///     作用域内单例 记录创建次数
    /// </summary>
    public sealed class ScopedOnly : IDisposable
    {
        /// <summary>
        ///     创建次数
        /// </summary>
        public static int Created;

        /// <summary>
        ///     初始化作用域内单例
        /// </summary>
        public ScopedOnly()
        {
            Interlocked.Increment(ref Created);
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }

    /// <summary>
    ///     记录释放次数的作用域内单例
    /// </summary>
    public sealed class ScopedDisposable : IDisposable
    {
        /// <summary>
        ///     释放次数
        /// </summary>
        public static int Disposed;

        /// <inheritdoc />
        public void Dispose()
        {
            Interlocked.Increment(ref Disposed);
        }
    }

    /// <summary>
    ///     作用域内单例 依赖一个多例 用于验证释放顺序
    /// </summary>
    public sealed class ScopedWithTransient : IDisposable
    {
        /// <summary>
        ///     初始化作用域内单例
        /// </summary>
        /// <param name="dependency">多例依赖</param>
        public ScopedWithTransient(LoggedA dependency)
        {
            Dependency = dependency;
        }

        /// <summary>
        ///     多例依赖
        /// </summary>
        public LoggedA Dependency { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            DisposalLog.Entries.Enqueue("scoped");
        }
    }

    /// <summary>
    ///     单例 依赖作用域内单例 解析时应当失败
    /// </summary>
    public sealed class SingletonNeedsScoped
    {
        /// <summary>
        ///     初始化单例
        /// </summary>
        /// <param name="service">作用域内单例</param>
        public SingletonNeedsScoped(ScopedOnly service)
        {
        }
    }
}
