/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：服务定义,存储服务的类型等信息.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:09:23
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.DependencyInjection
{
    /// <summary>
    ///     服务定义
    /// </summary>
    public class ServiceDefinition
    {
        /// <summary>
        ///     初始化服务定义
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="implementType">实现类型</param>
        /// <param name="factory">构造方法委托</param>
        /// <param name="serviceLifetime">服务的生命周期</param>
        internal ServiceDefinition(Type serviceType, Type implementType = null,
            Func<ServiceContainer, object> factory = null,
            EServiceLifetime serviceLifetime = EServiceLifetime.Singleton)
        {
            ServiceType = serviceType;
            ImplementType = implementType ?? serviceType;
            ServiceLifetime = serviceLifetime;
            ImplementationFactory = factory;
        }

        /// <summary>
        ///     生命周期
        /// </summary>
        public EServiceLifetime ServiceLifetime { get; }

        /// <summary>
        ///     实现类型
        /// </summary>
        public Type ImplementType { get; }

        /// <summary>
        ///     服务类型
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        ///     构造方法委托
        /// </summary>
        public Func<ServiceContainer, object> ImplementationFactory { get; }

        /// <summary>
        ///     获取实现类型
        /// </summary>
        /// <returns></returns>
        public Type GetImplementType()
        {
            if (ImplementType != null)
                return ImplementType;

            if (ImplementationFactory != null)
                return ImplementationFactory.Method.ReturnType;

            return ServiceType;
        }


        /// <summary>
        ///     构造一个单例的服务定义
        ///     此服务的创建方式为根据反射获取到的第一个公开或非公开构造函数创建
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <typeparam name="TServiceImplement">实现类类型</typeparam>
        /// <returns></returns>
        public static ServiceDefinition Singleton<TService, TServiceImplement>() where TServiceImplement : TService
        {
            return new ServiceDefinition(typeof(TService), typeof(TServiceImplement));
        }

        /// <summary>
        ///     构造一个单例的服务定义
        ///     此服务的创建方式为根据反射获取到的第一个公开或非公开构造函数创建
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <returns></returns>
        public static ServiceDefinition Singleton<TService>()
        {
            return new ServiceDefinition(typeof(TService));
        }

        /// <summary>
        ///     构造一个单例的服务定义
        ///     此服务的创建方式为方法委托创建
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <param name="factory">构造服务的方法委托</param>
        /// <returns></returns>
        public static ServiceDefinition Singleton<TService>(Func<ServiceContainer, object> factory)
        {
            return new ServiceDefinition(typeof(TService), null, factory);
        }

        /// <summary>
        ///     构造一个单例的服务定义
        ///     此服务的创建方式为方法委托创建
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <typeparam name="TServiceImplement">实现类类型</typeparam>
        /// <param name="factory">构造服务的方法委托</param>
        /// <returns></returns>
        public static ServiceDefinition Singleton<TService, TServiceImplement>(Func<ServiceContainer, object> factory)
        {
            return new ServiceDefinition(typeof(TService), typeof(TServiceImplement), factory);
        }


        /// <summary>
        ///     构造一个多例的服务定义
        ///     此服务的创建方式为根据反射获取到的第一个公开或非公开构造函数创建
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <typeparam name="TServiceImplement">实现类类型</typeparam>
        /// <returns></returns>
        public static ServiceDefinition Transient<TService, TServiceImplement>() where TServiceImplement : TService
        {
            return new ServiceDefinition(typeof(TService), typeof(TServiceImplement), null, EServiceLifetime.Transient);
        }

        /// <summary>
        ///     构造一个多例的服务定义
        ///     此服务的创建方式为根据反射获取到的第一个公开或非公开构造函数创建
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <returns></returns>
        public static ServiceDefinition Transient<TService>()
        {
            return new ServiceDefinition(typeof(TService), null, null, EServiceLifetime.Transient);
        }

        /// <summary>
        ///     构造一个多例的服务定义
        ///     此服务的创建方式为方法委托创建
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <param name="factory">构造服务的方法委托</param>
        /// <returns></returns>
        public static ServiceDefinition Transient<TService>(Func<ServiceContainer, object> factory)
        {
            return new ServiceDefinition(typeof(TService), null, factory, EServiceLifetime.Transient);
        }

        /// <summary>
        ///     构造一个多例的服务定义
        ///     此服务的创建方式为方法委托创建
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <typeparam name="TServiceImplement">实现类类型</typeparam>
        /// <param name="factory">构造服务的方法委托</param>
        /// <returns></returns>
        public static ServiceDefinition Transient<TService, TServiceImplement>(Func<ServiceContainer, object> factory)
        {
            return new ServiceDefinition(typeof(TService), typeof(TServiceImplement), factory,
                EServiceLifetime.Transient);
        }
    }
}