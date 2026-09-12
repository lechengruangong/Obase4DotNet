/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：服务的构造函数缓存,缓存注册的服务构造函数.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:00:12
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Concurrent;
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
        ///     单例对象
        /// </summary>
        private static readonly Lazy<ServiceConstructorInstance> LazyCurrent =
            new Lazy<ServiceConstructorInstance>(() => new ServiceConstructorInstance(),
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        ///     对象上下文类服务容器缓存
        /// </summary>
        private readonly ConcurrentDictionary<Type, ConstructorInfo> _serviceConstructors =
            new ConcurrentDictionary<Type, ConstructorInfo>();

        /// <summary>
        ///     私有构造
        /// </summary>
        private ServiceConstructorInstance()
        {
        }

        /// <summary>
        ///     获取服务容器实例
        /// </summary>
        public static ServiceConstructorInstance Current => LazyCurrent.Value;

        /// <summary>
        ///     获取某个服务类型的构造函数
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        internal ConstructorInfo GetConstructor(Type serviceType)
        {
            //使用并发字典保证线程安全 如果不存在则返回null
            return _serviceConstructors.TryGetValue(serviceType, out var constructor) ? constructor : null;
        }

        /// <summary>
        ///     设置某个服务类型的构造函数
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="constructor">服务容器</param>
        internal void SetConstructor(Type serviceType, ConstructorInfo constructor)
        {
            //使用并发字典保证线程安全 已存在时不覆盖
            _serviceConstructors.TryAdd(serviceType, constructor);
        }

        /// <summary>
        ///     获取某个类的构造函数是否已经缓存
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        internal bool Exist(Type serviceType)
        {
            //使用并发字典保证线程安全
            return _serviceConstructors.ContainsKey(serviceType);
        }
    }
}