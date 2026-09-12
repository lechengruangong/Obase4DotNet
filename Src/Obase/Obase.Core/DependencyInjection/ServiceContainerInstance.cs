/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：服务容器实例,提供服务容器实例的单例.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:06:11
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Obase.Core.DependencyInjection
{
    /// <summary>
    ///     服务容器实例
    /// </summary>
    public class ServiceContainerInstance : IDisposable
    {
        /// <summary>
        ///     单例对象
        /// </summary>
        private static readonly Lazy<ServiceContainerInstance> LazyCurrent =
            new Lazy<ServiceContainerInstance>(() => new ServiceContainerInstance(),
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        ///     对象上下文类服务容器缓存
        /// </summary>
        private readonly ConcurrentDictionary<Type, ServiceContainer> _serviceContainers =
            new ConcurrentDictionary<Type, ServiceContainer>();

        /// <summary>
        ///     私有构造
        /// </summary>
        private ServiceContainerInstance()
        {
            //注册退出事件
            AppDomain.CurrentDomain.ProcessExit += (s1, e1) => { Dispose(); };
            Console.CancelKeyPress += (s1, e1) => { Dispose(); };
        }


        /// <summary>
        ///     获取服务容器实例
        /// </summary>
        public static ServiceContainerInstance Current => LazyCurrent.Value;

        /// <summary>
        ///     释放资源方法
        /// </summary>
        public void Dispose()
        {
            //释放所有的服务容器
            foreach (var container in _serviceContainers) container.Value?.Dispose();
        }

        /// <summary>
        ///     获取某个上下文类型的服务容器
        /// </summary>
        /// <param name="contextType">上下文类型</param>
        /// <returns></returns>
        public ServiceContainer GetServiceContainer(Type contextType)
        {
            //使用并发字典保证线程安全 读取不到则返回null
            return _serviceContainers.TryGetValue(contextType, out var container) ? container : null;
        }

        /// <summary>
        ///     设置某个上下文类型的服务容器
        /// </summary>
        /// <param name="contextType">上下文类型</param>
        /// <param name="container">服务容器</param>
        public void SetServiceContainer(Type contextType, ServiceContainer container)
        {
            //使用并发字典保证线程安全 已存在时不覆盖
            _serviceContainers.TryAdd(contextType, container);
        }
    }
}