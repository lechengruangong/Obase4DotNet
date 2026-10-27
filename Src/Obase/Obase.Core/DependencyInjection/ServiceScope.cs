/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：服务作用域,表示一批服务实例的归属与释放边界.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-10-27 11:03:22
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Obase.Core.DependencyInjection
{
    /// <summary>
    ///     服务作用域
    ///     作用域只是释放边界,不是可见性边界:作用域内能解析到的服务与容器完全一致.
    ///     从作用域解析出的多例服务与作用域内单例(EServiceLifetime.Scoped)归该作用域所有,
    ///     作用域释放时按创建顺序的逆序释放;单例始终归容器所有,不受作用域影响.
    ///     作用域可以再创建子作用域,释放父作用域时先释放仍然存活的子作用域.
    /// </summary>
    /// <example>
    ///     <code>
    /// using (var scope = container.CreateScope())
    /// {
    ///     var repository = scope.GetService&lt;IRepository&gt;();
    /// }
    /// </code>
    /// </example>
    public sealed class ServiceScope : IDisposable
    {
        /// <summary>
        ///     子作用域(按创建顺序记录)
        /// </summary>
        private readonly ConcurrentDictionary<ServiceScope, byte> _children =
            new ConcurrentDictionary<ServiceScope, byte>();

        /// <summary>
        ///     所属的服务容器
        /// </summary>
        private readonly ServiceContainer _container;

        /// <summary>
        ///     归属于本作用域的实例(按创建顺序记录)
        /// </summary>
        private readonly ConcurrentQueue<object> _disposables = new ConcurrentQueue<object>();

        /// <summary>
        ///     父作用域 为null表示是容器的直接子作用域
        /// </summary>
        private readonly ServiceScope _parent;

        /// <summary>
        ///     归属于本作用域的作用域内单例的实例
        /// </summary>
        private readonly ConcurrentDictionary<ServiceContainer.ServiceKey, Lazy<object>> _scopedInstances =
            new ConcurrentDictionary<ServiceContainer.ServiceKey, Lazy<object>>();

        /// <summary>
        ///     释放时使用的锁
        /// </summary>
        private readonly object _disposeLock = new object();

        /// <summary>
        ///     创建序号 用于释放时保证后创建的先释放
        /// </summary>
        private readonly long _sequence;

        /// <summary>
        ///     是否需要释放的标记 0-未释放 1-已释放
        /// </summary>
        private int _disposed;

        /// <summary>
        ///     初始化服务作用域
        ///     由ServiceContainer.CreateScope创建
        /// </summary>
        /// <param name="container">所属的服务容器</param>
        /// <param name="parent">父作用域 为null表示是容器的直接子作用域</param>
        /// <param name="sequence">创建序号</param>
        internal ServiceScope(ServiceContainer container, ServiceScope parent, long sequence)
        {
            _container = container;
            _parent = parent;
            _sequence = sequence;
        }

        /// <summary>
        ///     是否已释放
        /// </summary>
        public bool IsDisposed => Volatile.Read(ref _disposed) != 0;

        /// <summary>
        ///     创建序号 用于释放时保证后创建的先释放
        /// </summary>
        internal long Sequence => _sequence;

        /// <summary>
        ///     归属于本作用域的作用域内单例的实例
        /// </summary>
        internal ConcurrentDictionary<ServiceContainer.ServiceKey, Lazy<object>> ScopedInstances => _scopedInstances;

        /// <summary>
        ///     创建一个子作用域
        ///     子作用域内解析的多例服务与作用域内单例归子作用域所有,释放本作用域时子作用域会先被释放
        /// </summary>
        /// <returns>子作用域</returns>
        public ServiceScope CreateScope()
        {
            EnsureNotDisposed();

            return _container.CreateScopeCore(this);
        }

        /// <summary>
        ///     获取服务
        ///     多例服务与作用域内单例归本作用域所有,单例仍是容器中的同一个实例
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns>服务实例 未注册时返回null</returns>
        public object GetService(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            EnsureNotDisposed();

            return _container.GetServiceInScope(serviceType, this);
        }

        /// <summary>
        ///     获取服务
        ///     多例服务与作用域内单例归本作用域所有,单例仍是容器中的同一个实例
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <returns>服务实例 未注册时返回null(值类型为默认值)</returns>
        public TService GetService<TService>()
        {
            var service = GetService(typeof(TService));
            return service == null ? default(TService) : (TService)service;
        }

        /// <summary>
        ///     释放作用域 释放归属于本作用域的实例
        ///     先释放仍然存活的子作用域(后创建的先释放) 再释放本作用域自己的实例
        ///     重复释放不会产生任何影响
        /// </summary>
        public void Dispose()
        {
            //已经释放过 直接返回
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

            lock (_disposeLock)
            {
                //先释放子作用域 后创建的先释放
                if (!_children.IsEmpty)
                {
                    var children = new List<ServiceScope>(_children.Keys);
                    children.Sort((x, y) => y.Sequence.CompareTo(x.Sequence));
                    foreach (var child in children)
                        try
                        {
                            child.Dispose();
                        }
                        catch (Exception)
                        {
                            //单个子作用域释放失败 不影响其余子作用域的释放
                        }

                    _children.Clear();
                }

                //逆序释放 先创建的后释放 尽量让依赖方先于被依赖方释放
                var instances = _disposables.ToArray();
                for (var i = instances.Length - 1; i >= 0; i--)
                    try
                    {
                        (instances[i] as IDisposable)?.Dispose();
                    }
                    catch (Exception)
                    {
                        //单个实例释放失败 不影响其余实例的释放
                    }

                while (_disposables.TryDequeue(out _))
                {
                }

                _scopedInstances.Clear();
            }

            //从父作用域或容器中注销 释放之后不再负责这个作用域
            if (_parent != null)
                _parent.UnregisterChild(this);
            else
                _container.UnregisterScope(this);
        }

        /// <summary>
        ///     注销子作用域
        ///     供子作用域释放时调用
        /// </summary>
        /// <param name="child">子作用域</param>
        internal void UnregisterChild(ServiceScope child)
        {
            if (child == null) return;

            _children.TryRemove(child, out var _);
        }

        /// <summary>
        ///     登记子作用域
        ///     供容器创建子作用域时调用
        /// </summary>
        /// <param name="child">子作用域</param>
        internal void RegisterChild(ServiceScope child)
        {
            if (child == null) return;

            _children.TryAdd(child, 0);
        }

        /// <summary>
        ///     登记一个归属于本作用域的实例
        /// </summary>
        /// <param name="instance">服务实例</param>
        internal void Track(object instance)
        {
            if (instance is IDisposable) _disposables.Enqueue(instance);
        }

        /// <summary>
        ///     确认作用域未释放
        /// </summary>
        internal void EnsureNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ServiceScope), "服务作用域已被释放,不能再获取服务.");
        }
    }
}
