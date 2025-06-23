/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：服务容器,提供注册服务的容器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:03:22
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Obase.Core.DependencyInjection
{
    /// <summary>
    ///     服务容器
    /// </summary>
    public class ServiceContainer : IDisposable
    {
        /// <summary>
        ///     服务定义集合
        /// </summary>
        private readonly IReadOnlyList<ServiceDefinition> _services;

        /// <summary>
        ///     单例服务集合
        /// </summary>
        private readonly ConcurrentDictionary<ServiceKey, object> _singletonInstances =
            new ConcurrentDictionary<ServiceKey, object>();

        /// <summary>
        ///     多例的服务集合
        /// </summary>
        private readonly ConcurrentBag<object> _transientDisposables = new ConcurrentBag<object>();

        /// <summary>
        ///     初始化服务容器
        /// </summary>
        /// <param name="serviceDefinitions">服务定义集合</param>
        internal ServiceContainer(IReadOnlyList<ServiceDefinition> serviceDefinitions)
        {
            _services = serviceDefinitions;
            //注册退出事件
            AppDomain.CurrentDomain.ProcessExit += (s1, e1) => { Dispose(); };
            Console.CancelKeyPress += (s1, e1) => { Dispose(); };
        }

        /// <summary>
        ///     释放方法
        /// </summary>
        public void Dispose()
        {
            lock (_singletonInstances)
            {
                //单例的释放掉
                foreach (var instance in _singletonInstances.Values) (instance as IDisposable)?.Dispose();
                //多例的释放掉
                foreach (var o in _transientDisposables) (o as IDisposable)?.Dispose();

                _singletonInstances.Clear();
            }
        }

        /// <summary>
        ///     获取服务
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            //在服务定义中查找
            var serviceDefinition = _services.LastOrDefault(s => s.ServiceType == serviceType);
            object svc;
            if (serviceDefinition == null)
            {
                //如果是泛型的
                if (serviceType.IsGenericType)
                {
                    //取泛型中最后一个与服务类型相同的
                    var genericType = serviceType.GetGenericTypeDefinition();
                    serviceDefinition = _services.LastOrDefault(s => s.ServiceType == genericType);
                    //没有找到 则此处尝试获取的可能是所有此服务类型的服务
                    //根据约定 此时形式必须为 List<Service>
                    if (serviceDefinition == null)
                    {
                        //做一个List<Service>
                        var innerServiceType = serviceType.GetGenericArguments().First();
                        //如果要的不是 就返回null
                        if (!typeof(List<>).MakeGenericType(innerServiceType)
                                .IsAssignableFrom(serviceType)) return null;
                        //获取具体的类型
                        var innerRegType = innerServiceType;
                        if (innerServiceType.IsGenericType) innerRegType = innerServiceType.GetGenericTypeDefinition();
                        //组装结果 
                        var list = new List<object>();
                        foreach (var def in _services.Where(s => s.ServiceType == innerRegType))
                        {
                            //单例 存放于字典
                            if (def.ServiceLifetime == EServiceLifetime.Singleton)
                            {
                                svc = _singletonInstances.GetOrAdd(new ServiceKey(innerServiceType, def),
                                    _ => GetServiceInstance(innerServiceType, def));
                            }
                            else
                            {
                                //多例 每次创建
                                svc = GetServiceInstance(innerServiceType, def);
                                //可释放的放置于释放区
                                if (svc is IDisposable) _transientDisposables.Add(svc);
                            }

                            if (svc != null) list.Add(svc);
                        }

                        //内部转换
                        var realResult = Activator.CreateInstance(typeof(List<>).MakeGenericType(innerRegType));
                        foreach (var r in list)
                        {
                            var addMethod = realResult.GetType()
                                .GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                            addMethod?.Invoke(realResult, new[] { r });
                        }

                        return realResult;
                    }
                }
                else
                {
                    return null;
                }
            }

            //单例的 放入单例集合
            if (serviceDefinition.ServiceLifetime == EServiceLifetime.Singleton)
                return _singletonInstances.GetOrAdd(new ServiceKey(serviceType, serviceDefinition),
                    key => GetServiceInstance(key.ServiceType, serviceDefinition));
            //多例的 直接创建
            svc = GetServiceInstance(serviceType, serviceDefinition);
            //实现了IDisposable的放入释放集合
            if (svc is IDisposable) _transientDisposables.Add(svc);
            return svc;
        }

        /// <summary>
        ///     获取服务
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <returns></returns>
        public TService GetService<TService>()
        {
            return (TService)GetService(typeof(TService));
        }

        /// <summary>
        ///     获取服务对象方法
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="serviceDefinition">服务定义</param>
        /// <returns></returns>
        private object GetServiceInstance(Type serviceType, ServiceDefinition serviceDefinition)
        {
            //定义了自己的方法 由定义方处理
            if (serviceDefinition.ImplementationFactory != null)
                return serviceDefinition.ImplementationFactory.Invoke(this);
            //没有定义 反射处理
            var implementType = serviceDefinition.ImplementType ?? serviceType;

            if (implementType.IsInterface || implementType.IsAbstract)
                throw new InvalidOperationException(
                    $"实现类型不能是接口或者抽象类,服务类型:{serviceType.FullName},实现类型:{serviceDefinition.ImplementType}.");

            //从缓存里面取
            var constructor = ServiceConstructorInstance.Current.GetConstructor(implementType);
            //没有 反射取
            if (constructor == null)
            {
                //获取所有公开的
                var constructors = implementType.GetConstructors();
                if (constructors.Length == 0)
                    //没有就获取非公开的    
                    constructors = implementType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

                constructor = constructors.FirstOrDefault();
            }

            if (constructor == null)
                throw new ArgumentException($"无法获取到服务{implementType.FullName}可用的构造函数.");

            //不存在的放入缓存
            if (!ServiceConstructorInstance.Current.Exist(implementType))
                ServiceConstructorInstance.Current.SetConstructor(implementType, constructor);

            var parameters = constructor.GetParameters();

            //没有参数
            if (parameters.Length == 0) return constructor.Invoke(null);

            var parameterList = new List<object>();
            //有参数 从当前容器里取符合的参数
            foreach (var parameter in parameters)
            {
                var parameterType = parameter.ParameterType;
                var parameterValue = GetService(parameterType);
                if (parameterValue == null)
                    throw new ArgumentException(
                        $"处理服务{implementType.FullName}的构造函数参数{parameter.Name}出错,无法从已注册的服务中获取类型为{parameterType.FullName}的对象.");
                parameterList.Add(parameterValue);
            }

            return constructor.Invoke(parameterList.ToArray());
        }

        /// <summary>
        ///     服务类型键
        ///     表示某个服务类型的具体定义
        /// </summary>
        private sealed class ServiceKey : IEquatable<ServiceKey>
        {
            /// <summary>
            ///     实现类类型
            /// </summary>
            private readonly Type _implementType;

            /// <summary>
            ///     服务类型
            /// </summary>
            private readonly Type _serviceType;

            /// <summary>
            ///     初始化服务类型键
            /// </summary>
            /// <param name="serviceType">服务类型</param>
            /// <param name="definition">服务定义</param>
            public ServiceKey(Type serviceType, ServiceDefinition definition)
            {
                _serviceType = serviceType;
                _implementType = definition.GetImplementType();
            }

            /// <summary>
            ///     服务类型
            /// </summary>
            public Type ServiceType => _serviceType;

            /// <summary>
            ///     实现类类型
            /// </summary>
            public Type ImplementType => _implementType;

            /// <summary>
            ///     重写equal方法
            /// </summary>
            /// <param name="other">要比较的对象</param>
            /// <returns></returns>
            public bool Equals(ServiceKey other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                return _serviceType == other._serviceType && _implementType == other._implementType;
            }

            /// <summary>
            ///     重写equal方法
            /// </summary>
            /// <param name="obj">要比较的对象</param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                return ReferenceEquals(this, obj) || (obj is ServiceKey other && Equals(other));
            }

            /// <summary>
            ///     重写HashCode以保证在集合中的计算
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                var key = $"{ServiceType.FullName}_{ImplementType.FullName}";
                return key.GetHashCode();
            }
        }
    }
}