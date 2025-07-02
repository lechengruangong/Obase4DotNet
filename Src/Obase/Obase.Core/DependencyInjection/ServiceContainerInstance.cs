/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：服务容器实例,提供服务容器实例的单例.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:06:11
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Threading;

namespace Obase.Core.DependencyInjection
{
    /// <summary>
    ///     服务容器实例
    /// </summary>
    public class ServiceContainerInstance : IDisposable
    {
        /// <summary>
        ///     锁对象
        /// </summary>
        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        ///     单例对象
        /// </summary>
        private static volatile ServiceContainerInstance _current;

        /// <summary>
        ///     对象上下文类服务容器缓存
        /// </summary>
        private readonly Dictionary<Type, ServiceContainer> _serviceContainers =
            new Dictionary<Type, ServiceContainer>();

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
        public static ServiceContainerInstance Current
        {
            get
            {
                // 双重锁定单例模式
                if (_current == null)
                    lock (typeof(ServiceContainerInstance))
                    {
                        if (_current == null) _current = new ServiceContainerInstance();
                    }

                return _current;
            }
        }

        /// <summary>
        ///     释放资源方法
        /// </summary>
        public void Dispose()
        {
            //释放所有的服务容器
            foreach (var container in _serviceContainers) container.Value.Dispose();
        }

        /// <summary>
        ///     获取某个上下文类型的服务容器
        /// </summary>
        /// <param name="contextType">上下文类型</param>
        /// <returns></returns>
        public ServiceContainer GetServiceContainer(Type contextType)
        {
            //使用锁对象保证线程安全 读取不到则返回null
            ReaderWriterLock.EnterReadLock();
            var result = _serviceContainers.TryGetValue(contextType, out var container) ? container : null;
            ReaderWriterLock.ExitReadLock();
            return result;
        }

        /// <summary>
        ///     设置某个上下文类型的服务容器
        /// </summary>
        /// <param name="contextType">上下文类型</param>
        /// <param name="container">服务容器</param>
        public void SetServiceContainer(Type contextType, ServiceContainer container)
        {
            if (!_serviceContainers.ContainsKey(contextType))
            {
                //使用锁对象保证线程安全 不存在则添加
                ReaderWriterLock.EnterWriteLock();
                _serviceContainers[contextType] = container;
                ReaderWriterLock.ExitWriteLock();
            }
        }
    }
}