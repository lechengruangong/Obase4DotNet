/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：服务的构造函数缓存,缓存注册的服务构造函数.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:00:12
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Obase.Core.DependencyInjection
{
    /// <summary>
    ///     服务的构造函数缓存
    /// </summary>
    internal class ServiceConstructorInstance
    {
        /// <summary>
        ///     锁对象
        /// </summary>
        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        ///     单例对象
        /// </summary>
        private static volatile ServiceConstructorInstance _current;

        /// <summary>
        ///     对象上下文类服务容器缓存
        /// </summary>
        private readonly Dictionary<Type, ConstructorInfo> _serviceConstructors =
            new Dictionary<Type, ConstructorInfo>();

        /// <summary>
        ///     私有构造
        /// </summary>
        private ServiceConstructorInstance()
        {
        }

        /// <summary>
        ///     获取服务容器实例
        /// </summary>
        public static ServiceConstructorInstance Current
        {
            get
            {
                // 双重锁定单例模式
                if (_current == null)
                    lock (typeof(ServiceConstructorInstance))
                    {
                        if (_current == null) _current = new ServiceConstructorInstance();
                    }

                return _current;
            }
        }

        /// <summary>
        ///     获取某个服务类型的构造函数
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        internal ConstructorInfo GetConstructor(Type serviceType)
        {
            // 使用读写锁来保证线程安全 如果不存在则返回null
            ReaderWriterLock.EnterReadLock();
            var result = _serviceConstructors.TryGetValue(serviceType, out var constructor) ? constructor : null;
            ReaderWriterLock.ExitReadLock();
            return result;
        }

        /// <summary>
        ///     设置某个服务类型的构造函数
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="constructor">服务容器</param>
        internal void SetConstructor(Type serviceType, ConstructorInfo constructor)
        {
            if (!_serviceConstructors.ContainsKey(serviceType))
            {
                // 使用读写锁来保证线程安全 不存在则添加
                ReaderWriterLock.EnterWriteLock();
                _serviceConstructors[serviceType] = constructor;
                ReaderWriterLock.ExitWriteLock();
            }
        }

        /// <summary>
        ///     获取某个类的构造函数是否已经缓存
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        internal bool Exist(Type serviceType)
        {
            // 使用读写锁来保证线程安全 用类型的哈希值来判断是否存在
            ReaderWriterLock.EnterReadLock();
            var result = _serviceConstructors.ContainsKey(serviceType);
            ReaderWriterLock.ExitReadLock();
            return result;
        }
    }
}