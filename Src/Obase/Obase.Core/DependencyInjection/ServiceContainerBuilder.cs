/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：服务容器建造器,提供建造服务容器方法.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:04:42
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.DependencyInjection
{
    /// <summary>
    ///     服务容器建造器
    /// </summary>
    public sealed class ServiceContainerBuilder : IEnumerable<ServiceDefinition>
    {
        /// <summary>
        ///     所属的上下文类型
        /// </summary>
        private readonly Type _contextType;

        /// <summary>
        ///     注入的服务定义集合
        /// </summary>
        private readonly List<ServiceDefinition> _services = new List<ServiceDefinition>();

        /// <summary>
        ///     初始化服务容器建造器
        /// </summary>
        /// <param name="contextType">上下文类型</param>
        public ServiceContainerBuilder(Type contextType)
        {
            // 检查上下文类型是否为ObjectContext的实现类
            if (!typeof(ObjectContext).IsAssignableFrom(contextType))
                throw new ArgumentException($"{contextType.FullName}必须是ObjectContext的实现类.");

            _contextType = contextType;
        }

        /// <summary>
        ///     实现集合的GetEnumerator方法
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ServiceDefinition> GetEnumerator()
        {
            return _services.GetEnumerator();
        }

        /// <summary>
        ///     实现接口的GetEnumerator方法
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     添加一个服务定义
        ///     如果存在相同服务类型和实现类型的服务定义则返回原有定义
        /// </summary>
        /// <param name="item">服务定义</param>
        /// <returns></returns>
        private ServiceContainerBuilder Add(ServiceDefinition item)
        {
            if (item.GetImplementType().IsInterface || item.GetImplementType().IsAbstract)
                throw new InvalidOperationException(
                    $"实现类型不能是接口或者抽象类,服务类型:{item.ServiceType.FullName},实现类型:{item.GetImplementType().FullName}.");
            // 检查是否已存在相同的服务定义
            if (_services.Any(s =>
                    s.ServiceType == item.ServiceType && s.GetImplementType() == item.GetImplementType())) return this;

            _services.Add(item);
            return this;
        }

        /// <summary>
        ///     添加一个单例的服务定义
        ///     此服务的创建方式为根据反射获取到的第一个公开或非公开构造函数创建
        /// </summary>
        /// <param name="serviceType">服务的类型</param>
        /// <returns></returns>
        public ServiceContainerBuilder AddSingleton(Type serviceType)
        {
            return Add(new ServiceDefinition(serviceType));
        }

        /// <summary>
        ///     添加一个单例的服务定义
        ///     此服务的创建方式为根据反射获取到的第一个公开或非公开构造函数创建
        /// </summary>
        /// <param name="serviceType">服务的类型</param>
        /// <param name="implementType">实现类型</param>
        /// <returns></returns>
        public ServiceContainerBuilder AddSingleton(Type serviceType, Type implementType)
        {
            return Add(new ServiceDefinition(serviceType, implementType));
        }

        /// <summary>
        ///     添加一个单例的服务定义
        ///     此服务的创建方式为根据反射获取到的第一个公开或非公开构造函数创建
        /// </summary>
        /// <typeparam name="TService">服务的类型</typeparam>
        /// <returns></returns>
        public ServiceContainerBuilder AddSingleton<TService>()
        {
            return Add(ServiceDefinition.Singleton<TService>());
        }

        /// <summary>
        ///     添加一个单例的服务定义
        ///     此服务的创建方式为根据反射获取到的第一个公开或非公开构造函数创建
        /// </summary>
        /// <typeparam name="TService">服务的类型</typeparam>
        /// <typeparam name="TServiceImplement">实现类型</typeparam>
        /// <returns></returns>
        public ServiceContainerBuilder AddSingleton<TService, TServiceImplement>() where TServiceImplement : TService
        {
            return Add(ServiceDefinition.Singleton<TService, TServiceImplement>());
        }

        /// <summary>
        ///     添加一个单例的服务定义
        ///     此服务的创建方式为方法委托创建
        /// </summary>
        /// <typeparam name="TService">服务的类型</typeparam>
        /// <param name="factory">构造对象委托</param>
        /// <returns></returns>
        public ServiceContainerBuilder AddSingleton<TService>(Func<ServiceContainer, object> factory)
        {
            return Add(ServiceDefinition.Singleton<TService>(factory));
        }

        /// <summary>
        ///     添加一个单例的服务定义
        ///     此服务的创建方式为方法委托创建
        /// </summary>
        /// <typeparam name="TService">服务的类型</typeparam>
        /// <typeparam name="TServiceImplement">实现类型</typeparam>
        /// <param name="factory">构造对象委托</param>
        /// <returns></returns>
        public ServiceContainerBuilder AddSingleton<TService, TServiceImplement>(Func<ServiceContainer, object> factory)
        {
            return Add(ServiceDefinition.Singleton<TService, TServiceImplement>(factory));
        }

        /// <summary>
        ///     添加一个多例的服务定义
        ///     此服务的创建方式为根据反射获取到的第一个公开或非公开构造函数创建
        /// </summary>
        /// <param name="serviceType">服务的类型</param>
        /// <returns></returns>
        public ServiceContainerBuilder AddTransient(Type serviceType)
        {
            return Add(new ServiceDefinition(serviceType, null, null, EServiceLifetime.Transient));
        }

        /// <summary>
        ///     添加一个多例的服务定义
        ///     此服务的创建方式为根据反射获取到的第一个公开或非公开构造函数创建
        /// </summary>
        /// <param name="serviceType">服务的类型</param>
        /// <param name="implementType">实现类型</param>
        /// <returns></returns>
        public ServiceContainerBuilder AddTransient(Type serviceType, Type implementType)
        {
            return Add(new ServiceDefinition(serviceType, implementType, null, EServiceLifetime.Transient));
        }

        /// <summary>
        ///     添加一个多例的服务定义
        ///     此服务的创建方式为根据反射获取到的第一个公开或非公开构造函数创建
        /// </summary>
        /// <typeparam name="TService">服务的类型</typeparam>
        /// <returns></returns>
        public ServiceContainerBuilder AddTransient<TService>()
        {
            return Add(ServiceDefinition.Transient<TService>());
        }

        /// <summary>
        ///     添加一个多例的服务定义
        ///     此服务的创建方式为根据反射获取到的第一个公开或非公开构造函数创建
        /// </summary>
        /// <typeparam name="TService">服务的类型</typeparam>
        /// <typeparam name="TServiceImplement">实现类型</typeparam>
        /// <returns></returns>
        public ServiceContainerBuilder AddTransient<TService, TServiceImplement>() where TServiceImplement : TService
        {
            return Add(ServiceDefinition.Transient<TService, TServiceImplement>());
        }

        /// <summary>
        ///     添加一个多例的服务定义
        ///     此服务的创建方式为方法委托创建
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <param name="factory">构造服务的方法委托</param>
        /// <returns></returns>
        public ServiceContainerBuilder AddTransient<TService>(Func<ServiceContainer, object> factory)
        {
            return Add(ServiceDefinition.Transient<TService>(factory));
        }

        /// <summary>
        ///     添加一个多例的服务定义
        ///     此服务的创建方式为方法委托创建
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <typeparam name="TServiceImplement">实现类型</typeparam>
        /// <param name="factory">构造服务的方法委托</param>
        /// <returns></returns>
        public ServiceContainerBuilder AddTransient<TService, TServiceImplement>(Func<ServiceContainer, object> factory)
        {
            return Add(ServiceDefinition.Transient<TService, TServiceImplement>(factory));
        }

        /// <summary>
        ///     建造服务容器
        ///     会同时将服务容器放置于单例中
        /// </summary>
        /// <returns></returns>
        public ServiceContainer Build()
        {
            // 新建一个服务容器 不可重复创建
            var container = new ServiceContainer(_services);
            if (ServiceContainerInstance.Current.GetServiceContainer(_contextType) != null)
                throw new InvalidOperationException($"上下文{_contextType.FullName}已创建服务容器,不能重复创建.");
            // 将服务容器放置于单例中
            ServiceContainerInstance.Current.SetServiceContainer(_contextType, container);
            return container;
        }
    }
}