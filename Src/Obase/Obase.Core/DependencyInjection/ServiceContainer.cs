/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：服务容器,提供注册服务的容器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 11:03:22
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Obase.Core.DependencyInjection
{
    /// <summary>
    ///     服务容器
    /// </summary>
    public class ServiceContainer : IDisposable
    {
        /// <summary>
        ///     单次解析允许的最大嵌套层数
        ///     仅用于兜底 正常情况下应由解析链的循环依赖检测先发现
        /// </summary>
        private const int MaxResolutionDepth = 128;

        /// <summary>
        ///     当前线程的解析嵌套层数
        /// </summary>
        [ThreadStatic] private static int _resolutionDepth;

        /// <summary>
        ///     是否需要释放的标记 0-未释放 1-已释放
        /// </summary>
        private int _disposed;

        /// <summary>
        ///     释放时使用的锁
        /// </summary>
        private readonly object _disposeLock = new object();

        /// <summary>
        ///     待释放的实例集合（按创建顺序记录 释放时逆序）
        /// </summary>
        private readonly ConcurrentQueue<object> _disposables = new ConcurrentQueue<object>();

        /// <summary>
        ///     服务定义索引（服务类型 -> 该服务类型下的全部服务定义 按注册顺序排列）
        /// </summary>
        private readonly Dictionary<Type, List<ServiceDefinition>> _serviceIndex;

        /// <summary>
        ///     单例服务集合
        ///     值为Lazy对象 保证同一个服务定义只创建一个实例 也避免创建过程被并发执行多次
        /// </summary>
        private readonly ConcurrentDictionary<ServiceKey, Lazy<object>> _singletonInstances =
            new ConcurrentDictionary<ServiceKey, Lazy<object>>();

        /// <summary>
        ///     从容器解析的作用域内单例的实例集合
        ///     容器本身也是一个作用域 从容器解析的作用域内单例归容器所有
        /// </summary>
        private readonly ConcurrentDictionary<ServiceKey, Lazy<object>> _rootScopedInstances =
            new ConcurrentDictionary<ServiceKey, Lazy<object>>();

        /// <summary>
        ///     当前线程正在创建的单例层数
        ///     不为零时说明正在创建单例 此时不允许依赖作用域内单例
        /// </summary>
        [ThreadStatic] private static int _singletonCreationDepth;

        /// <summary>
        ///     所属的上下文类型
        /// </summary>
        private readonly Type _contextType;

        /// <summary>
        ///     当前仍然存活的作用域
        /// </summary>
        private readonly ConcurrentDictionary<ServiceScope, byte> _scopes =
            new ConcurrentDictionary<ServiceScope, byte>();

        /// <summary>
        ///     作用域的创建序号 用于保证释放时后创建的先释放
        /// </summary>
        private long _scopeSequence;

        /// <summary>
        ///     初始化服务容器
        /// </summary>
        /// <param name="contextType">所属的上下文类型</param>
        /// <param name="serviceDefinitions">服务定义集合</param>
        internal ServiceContainer(Type contextType, IReadOnlyList<ServiceDefinition> serviceDefinitions)
        {
            _contextType = contextType;
            _serviceIndex = BuildServiceIndex(serviceDefinitions);
        }

        /// <summary>
        ///     创建一个服务作用域
        ///     从作用域获取的多例服务归该作用域所有 作用域释放时释放
        /// </summary>
        /// <returns>服务作用域</returns>
        public ServiceScope CreateScope()
        {
            return CreateScopeCore(null);
        }

        /// <summary>
        ///     创建一个服务作用域
        /// </summary>
        /// <param name="parent">父作用域 为null表示是容器的直接子作用域</param>
        /// <returns>服务作用域</returns>
        internal ServiceScope CreateScopeCore(ServiceScope parent)
        {
            ThrowIfDisposed();

            var scope = new ServiceScope(this, parent, Interlocked.Increment(ref _scopeSequence));
            //只有容器的直接子作用域由容器保管 子作用域由父作用域保管
            if (parent == null)
                _scopes.TryAdd(scope, 0);
            else
                parent.RegisterChild(scope);
            return scope;
        }

        /// <summary>
        ///     注销作用域
        ///     供作用域释放时调用
        /// </summary>
        /// <param name="scope">作用域</param>
        internal void UnregisterScope(ServiceScope scope)
        {
            if (scope == null) return;

            _scopes.TryRemove(scope, out var _);
        }

        /// <summary>
        ///     在指定的作用域内获取服务
        ///     供作用域调用
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="scope">所属的作用域</param>
        /// <returns>服务实例 未注册时返回null</returns>
        internal object GetServiceInScope(Type serviceType, ServiceScope scope)
        {
            ThrowIfDisposed();
            if (scope != null) scope.EnsureNotDisposed();

            return GetService(serviceType, scope, new ResolutionContext());
        }

        /// <summary>
        ///     释放方法
        /// </summary>
        public void Dispose()
        {
            lock (_disposeLock)
            {
                //已经释放过了 直接返回 保证重复释放不出问题
                if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

                //先释放仍然存活的作用域 后创建的先释放
                var liveScopes = new List<ServiceScope>(_scopes.Keys);
                liveScopes.Sort((x, y) => y.Sequence.CompareTo(x.Sequence));
                foreach (var liveScope in liveScopes)
                    try
                    {
                        liveScope.Dispose();
                    }
                    catch (Exception)
                    {
                        //单个作用域释放失败 不影响其余作用域的释放
                    }

                _scopes.Clear();

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

                //清理已释放的实例 避免释放在此之后又被登记
                while (_disposables.TryDequeue(out _))
                {
                }

                _singletonInstances.Clear();
                //容器本身的作用域内单例同样已经释放
                _rootScopedInstances.Clear();
            }

            //从全局登记中移除自己 释放之后可以重新建造同一上下文的容器
            ServiceContainerInstance.Current.Remove(_contextType, this);
        }

        /// <summary>
        ///     获取服务
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            return GetService(serviceType, null, new ResolutionContext());
        }

        /// <summary>
        ///     获取服务
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <returns></returns>
        public TService GetService<TService>()
        {
            var service = GetService(typeof(TService));
            //取不到时 引用类型返回null 值类型返回默认值 不再抛出空引用异常
            return service == null ? default(TService) : (TService)service;
        }

        /// <summary>
        ///     建立服务定义索引
        /// </summary>
        /// <param name="serviceDefinitions">服务定义集合</param>
        /// <returns></returns>
        private static Dictionary<Type, List<ServiceDefinition>> BuildServiceIndex(
            IReadOnlyList<ServiceDefinition> serviceDefinitions)
        {
            var index = new Dictionary<Type, List<ServiceDefinition>>();
            if (serviceDefinitions == null) return index;

            foreach (var serviceDefinition in serviceDefinitions)
            {
                if (!index.TryGetValue(serviceDefinition.ServiceType, out var definitions))
                {
                    definitions = new List<ServiceDefinition>();
                    index.Add(serviceDefinition.ServiceType, definitions);
                }

                definitions.Add(serviceDefinition);
            }

            return index;
        }

        /// <summary>
        ///     获取服务
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="scope">所属的作用域 为null表示从容器解析</param>
        /// <param name="context">本次解析的上下文 用于检测循环依赖</param>
        /// <returns></returns>
        private object GetService(Type serviceType, ServiceScope scope, ResolutionContext context)
        {
            ThrowIfDisposed();
            //同一解析链上重复出现同一服务类型 说明存在循环依赖
            if (context.Contains(serviceType))
                throw new InvalidOperationException($"服务{serviceType.FullName}存在循环依赖,解析链:{context.Describe(serviceType)}.");

            context.Enter(serviceType);
            try
            {
                //先按服务类型精确查找
                if (_serviceIndex.TryGetValue(serviceType, out var definitions))
                    return ResolveSingle(serviceType, definitions[definitions.Count - 1], scope, context);

                //非泛型服务 未注册即返回null
                if (!serviceType.IsGenericType) return null;

                var definitionType = serviceType.GetGenericTypeDefinition();
                //按开放泛型的定义查找 解析时再闭合泛型参数
                if (_serviceIndex.TryGetValue(definitionType, out definitions))
                    return ResolveSingle(serviceType, definitions[definitions.Count - 1], scope, context);

                //根据约定 形如 List<Service> 的请求用于获取某服务类型的全部实现
                if (definitionType == typeof(List<>)) return ResolveAll(serviceType, scope, context);

                return null;
            }
            finally
            {
                context.Exit(serviceType);
            }
        }

        /// <summary>
        ///     解析一个服务类型下的全部实现 用于 List&lt;Service&gt; 形式的请求
        /// </summary>
        /// <param name="listType">List&lt;Service&gt; 形式的服务类型</param>
        /// <param name="scope">所属的作用域 为null表示从容器解析</param>
        /// <param name="context">本次解析的上下文</param>
        /// <returns></returns>
        private object ResolveAll(Type listType, ServiceScope scope, ResolutionContext context)
        {
            var elementType = listType.GetGenericArguments()[0];
            //元素类型本身是开放泛型时 无法确定要构造的集合元素类型
            if (elementType.ContainsGenericParameters) return null;

            //未注册该服务类型时 与原先一致 返回空的集合而不是null
            if (!_serviceIndex.TryGetValue(elementType, out var definitions))
            {
                definitions = new List<ServiceDefinition>();
                //元素类型本身是泛型时 还需要按开放泛型的定义查找
                if (elementType.IsGenericType &&
                    _serviceIndex.TryGetValue(elementType.GetGenericTypeDefinition(), out var genericDefinitions))
                    definitions = genericDefinitions;
            }

            //直接按List<元素类型>构造 避免逐个反射调用Add方法
            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
            foreach (var definition in definitions)
            {
                var service = ResolveSingle(elementType, definition, scope, context);
                if (service != null) list.Add(service);
            }

            return list;
        }

        /// <summary>
        ///     按一个服务定义解析服务实例
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="serviceDefinition">服务定义</param>
        /// <param name="scope">所属的作用域 为null表示从容器解析</param>
        /// <param name="context">本次解析的上下文</param>
        /// <returns></returns>
        private object ResolveSingle(Type serviceType, ServiceDefinition serviceDefinition,
            ServiceScope scope, ResolutionContext context)
        {
            switch (serviceDefinition.ServiceLifetime)
            {
                case EServiceLifetime.Singleton:
                    return ResolveSingleton(serviceType, serviceDefinition, context);
                case EServiceLifetime.Scoped:
                    return ResolveScoped(serviceType, serviceDefinition, scope, context);
                default:
                    return CreateTransient(serviceType, serviceDefinition, scope, context);
            }
        }

        /// <summary>
        ///     解析单例服务
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="serviceDefinition">服务定义</param>
        /// <param name="context">本次解析的上下文</param>
        /// <returns></returns>
        private object ResolveSingleton(Type serviceType, ServiceDefinition serviceDefinition,
            ResolutionContext context)
        {
            var key = new ServiceKey(serviceType, serviceDefinition);
            //GetOrAdd会并发执行工厂 因此工厂里只创建Lazy 真正的实例化由Lazy保证只执行一次
            var lazyInstance = _singletonInstances.GetOrAdd(key,
                _ => new Lazy<object>(() =>
                {
                    //单例归容器所有 与从哪个作用域解析无关
                    _singletonCreationDepth++;
                    try
                    {
                        var instance = CreateInstance(serviceType, serviceDefinition, null, context);
                        Track(instance, null);
                        return instance;
                    }
                    finally
                    {
                        _singletonCreationDepth--;
                    }
                }, LazyThreadSafetyMode.ExecutionAndPublication));

            try
            {
                return lazyInstance.Value;
            }
            catch (Exception)
            {
                //创建失败时移除缓存 否则异常会被Lazy记录下来 之后再也不能重试
                _singletonInstances.TryRemove(key, out _);
                throw;
            }
        }

        /// <summary>
        ///     解析作用域内单例的服务
        ///     同一个作用域内只创建一次 归该作用域所有
        ///     从容器解析时归容器所有 与单例的区别是容器本身被当成一个作用域
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="serviceDefinition">服务定义</param>
        /// <param name="scope">所属的作用域 为null表示归容器所有</param>
        /// <param name="context">本次解析的上下文</param>
        /// <returns></returns>
        private object ResolveScoped(Type serviceType, ServiceDefinition serviceDefinition,
            ServiceScope scope, ResolutionContext context)
        {
            //单例的生命周期比作用域长 一旦依赖作用域内单例 就会把某个作用域的实例长期持有
            if (_singletonCreationDepth > 0)
                throw new InvalidOperationException(
                    $"单例服务不能依赖作用域内单例的服务,当前正在解析:{serviceType.FullName}.解析链:{context.DescribeChain()}.");

            var key = new ServiceKey(serviceType, serviceDefinition);
            var instances = scope != null ? scope.ScopedInstances : _rootScopedInstances;

            var lazyInstance = instances.GetOrAdd(key,
                _ => new Lazy<object>(() =>
                {
                    //实例的依赖同样归这个作用域所有
                    var instance = CreateInstance(serviceType, serviceDefinition, scope, context);
                    Track(instance, scope);
                    return instance;
                }, LazyThreadSafetyMode.ExecutionAndPublication));

            try
            {
                return lazyInstance.Value;
            }
            catch (Exception)
            {
                //创建失败时移除缓存 否则异常会被Lazy记录下来 之后再也不能重试
                instances.TryRemove(key, out _);
                throw;
            }
        }

        /// <summary>
        ///     创建一个多例服务 并登记需要释放的实例
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="serviceDefinition">服务定义</param>
        /// <param name="scope">所属的作用域 为null表示归容器所有</param>
        /// <param name="context">本次解析的上下文</param>
        /// <returns></returns>
        private object CreateTransient(Type serviceType, ServiceDefinition serviceDefinition,
            ServiceScope scope, ResolutionContext context)
        {
            var instance = CreateInstance(serviceType, serviceDefinition, scope, context);
            Track(instance, scope);
            return instance;
        }

        /// <summary>
        ///     登记需要释放的实例
        /// </summary>
        /// <param name="instance">服务实例</param>
        /// <param name="scope">所属的作用域 为null表示归容器自身所有</param>
        private void Track(object instance, ServiceScope scope)
        {
            if (!(instance is IDisposable)) return;

            if (scope != null)
                scope.Track(instance);
            else
                _disposables.Enqueue(instance);
        }

        /// <summary>
        ///     创建服务实例
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="serviceDefinition">服务定义</param>
        /// <param name="scope">所属的作用域 为null表示从容器解析</param>
        /// <param name="context">本次解析的上下文</param>
        /// <returns></returns>
        private object CreateInstance(Type serviceType, ServiceDefinition serviceDefinition,
            ServiceScope scope, ResolutionContext context)
        {
            //解析嵌套过深 大概率是委托创建的服务之间存在循环调用 此时无法靠解析链发现 用深度兜底
            var depth = ++_resolutionDepth;
            try
            {
                if (depth > MaxResolutionDepth)
                    throw new InvalidOperationException(
                        $"服务{serviceType.FullName}的解析嵌套已超过{MaxResolutionDepth}层,可能存在循环依赖(例如在委托中互相调用GetService).解析链:{context.Describe(serviceType)}.");

                //定义了自己的方法 由定义方处理
                if (serviceDefinition.ImplementationFactory != null)
                    return serviceDefinition.ImplementationFactory.Invoke(this);

                var implementType = ResolveImplementType(serviceType, serviceDefinition);

                if (implementType.IsInterface || implementType.IsAbstract)
                    throw new InvalidOperationException(
                        $"实现类型不能是接口或者抽象类,服务类型:{serviceType.FullName},实现类型:{implementType.FullName}." +
                        $"请为{serviceType.FullName}指定具体的实现类型.");

                var constructor = GetConstructor(implementType);
                if (constructor == null)
                    throw new ArgumentException($"无法获取到服务{implementType.FullName}可用的构造函数.");

                var parameters = constructor.GetParameters();
                //没有参数
                if (parameters.Length == 0) return constructor.Invoke(null);

                var parameterValues = new object[parameters.Length];
                //有参数 从当前容器里取符合的参数
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    var parameterType = parameter.ParameterType;
                    var parameterValue = GetService(parameterType, scope, context);
                    if (parameterValue == null)
                        throw new ArgumentException(
                            $"处理服务{implementType.FullName}的构造函数参数{parameter.Name}出错," +
                            $"无法从已注册的服务中获取类型为{parameterType.FullName}的对象.解析链:{context.Describe(implementType)}.");

                    parameterValues[i] = parameterValue;
                }

                return constructor.Invoke(parameterValues);
            }
            finally
            {
                _resolutionDepth = depth - 1;
            }
        }

        /// <summary>
        ///     确定服务的实现类型
        ///     注册为开放泛型时 按请求的闭合类型闭合泛型参数
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="serviceDefinition">服务定义</param>
        /// <returns></returns>
        private static Type ResolveImplementType(Type serviceType, ServiceDefinition serviceDefinition)
        {
            var implementType = serviceDefinition.GetImplementType();

            //注册的是开放泛型 但请求的是闭合泛型 需要按请求闭合实现类型
            if (implementType.IsGenericTypeDefinition && serviceType.IsConstructedGenericType &&
                serviceDefinition.ServiceType.IsGenericTypeDefinition)
            {
                var arguments = serviceType.GetGenericArguments();
                if (implementType.GetGenericArguments().Length != arguments.Length)
                    throw new InvalidOperationException(
                        $"服务{serviceDefinition.ServiceType.FullName}的泛型参数个数与请求的{serviceType.FullName}不一致,无法确定实现类型.");
                implementType = implementType.MakeGenericType(arguments);
            }

            if (implementType.ContainsGenericParameters)
                throw new InvalidOperationException(
                    $"服务{serviceType.FullName}的实现类型{implementType.FullName}仍是开放泛型, 请以闭合的泛型类型请求服务.");

            return implementType;
        }

        /// <summary>
        ///     获取服务类型的构造函数（公开的优先 其次非公开的）
        /// </summary>
        /// <param name="implementType">实现类型</param>
        /// <returns></returns>
        private static System.Reflection.ConstructorInfo GetConstructor(Type implementType)
        {
            //从缓存里面取
            var constructor = ServiceConstructorInstance.Current.GetConstructor(implementType);
            if (constructor != null) return constructor;

            //获取所有公开的
            var constructors = implementType.GetConstructors();
            if (constructors.Length == 0)
                //没有就获取非公开的
                constructors = implementType.GetConstructors(
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            constructor = constructors.Length == 0 ? null : constructors[0];
            //不存在的放入缓存
            if (constructor != null) ServiceConstructorInstance.Current.SetConstructor(implementType, constructor);

            return constructor;
        }

        /// <summary>
        ///     已释放时不再提供服务 避免被释放的容器又创建出无人释放的新实例
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _disposed) == 0) return;

            throw new ObjectDisposedException(nameof(ServiceContainer),
                $"上下文{_contextType?.FullName}的服务容器已被释放,不能再获取服务.");
        }

        /// <summary>
        ///     本次解析的上下文
        ///     记录解析链 用于检测循环依赖并给出可读的解析链
        /// </summary>
        private sealed class ResolutionContext
        {
            /// <summary>
            ///     解析链 按进入顺序记录服务类型
            /// </summary>
            private readonly List<Type> _chain = new List<Type>();

            /// <summary>
            ///     解析链上是否已存在某个服务类型
            /// </summary>
            /// <param name="serviceType">服务类型</param>
            /// <returns></returns>
            public bool Contains(Type serviceType)
            {
                return _chain.Contains(serviceType);
            }

            /// <summary>
            ///     进入一层解析
            /// </summary>
            /// <param name="serviceType">服务类型</param>
            public void Enter(Type serviceType)
            {
                _chain.Add(serviceType);
            }

            /// <summary>
            ///     退出一层解析
            /// </summary>
            /// <param name="serviceType">服务类型</param>
            public void Exit(Type serviceType)
            {
                if (_chain.Count > 0 && _chain[_chain.Count - 1] == serviceType)
                    _chain.RemoveAt(_chain.Count - 1);
                else
                    _chain.Remove(serviceType);
            }

            /// <summary>
            ///     描述解析链 用于异常信息
            /// </summary>
            /// <param name="serviceType">出现循环的服务类型</param>
            /// <returns></returns>
            public string Describe(Type serviceType)
            {
                var names = new List<string>();
                foreach (var type in _chain) names.Add(type.Name);
                names.Add(serviceType.Name);
                return string.Join(" -> ", names);
            }

            /// <summary>
            ///     描述当前解析链 用于异常信息
            /// </summary>
            /// <returns></returns>
            public string DescribeChain()
            {
                var names = new List<string>();
                foreach (var type in _chain) names.Add(type.Name);
                return string.Join(" -> ", names);
            }
        }

        /// <summary>
        ///     服务类型键
        ///     表示某个服务类型的具体定义
        /// </summary>
        internal sealed class ServiceKey : IEquatable<ServiceKey>
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
            ///     不再拼接字符串 避免每次查找都产生一次字符串分配
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = _serviceType?.GetHashCode() ?? 0;
                    return (hash * 397) ^ (_implementType?.GetHashCode() ?? 0);
                }
            }
        }
    }
}
