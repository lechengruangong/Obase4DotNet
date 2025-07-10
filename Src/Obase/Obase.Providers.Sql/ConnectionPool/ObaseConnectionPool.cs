/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Obase连接池容器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 15:48:22
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Obase.Core.Common;
using SafeObjectPool;

namespace Obase.Providers.Sql.ConnectionPool
{
    /// <summary>
    ///     Obase连接池容器
    /// </summary>
    public class ObaseConnectionPool : IDisposable
    {
        /// <summary>
        ///     锁对象
        /// </summary>
        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        ///     单例对象
        /// </summary>
        private static volatile ObaseConnectionPool _current;

        /// <summary>
        ///     当前连接池个数
        /// </summary>
        private static int _poolCount;

        /// <summary>
        ///     连接字符串与所属上下文类型的映射关系
        /// </summary>
        private readonly Dictionary<string, List<Type>> _contextTypes = new Dictionary<string, List<Type>>();

        /// <summary>
        ///     连接字符串与连接池的映射关系
        /// </summary>
        private readonly Dictionary<string, DbConnectionPool> _pools = new Dictionary<string, DbConnectionPool>();

        /// <summary>
        ///     私有构造
        /// </summary>
        private ObaseConnectionPool()
        {
            _poolCount = 0;
            //注册退出事件
            AppDomain.CurrentDomain.ProcessExit += (s1, e1) => { Dispose(); };
            Console.CancelKeyPress += (s1, e1) => { Dispose(); };
        }

        /// <summary>
        ///     唯一实例
        /// </summary>
        public static ObaseConnectionPool Current
        {
            get
            {
                if (_current == null)
                    lock (typeof(ObaseConnectionPool))
                    {
                        if (_current == null) _current = new ObaseConnectionPool();
                    }

                return _current;
            }
        }

        /// <summary>
        ///     获取简要分析
        /// </summary>
        public string Statistics
        {
            get
            {
                var result = new StringBuilder();

                ReaderWriterLock.EnterUpgradeableReadLock();
                foreach (var pool in _pools)
                    result.Append($"{pool.Value.Policy.Name} / {pool.Value.Statistics}").AppendLine();
                ReaderWriterLock.ExitUpgradeableReadLock();

                return result.ToString();
            }
        }

        /// <summary>
        ///     获取完整分析
        /// </summary>
        public string StatisticsFully
        {
            get
            {
                var result = new StringBuilder();

                ReaderWriterLock.EnterUpgradeableReadLock();
                foreach (var pool in _pools)
                    result.Append($"{pool.Value.Policy.Name} / {pool.Value.StatisticsFullily}").AppendLine();
                ReaderWriterLock.ExitUpgradeableReadLock();

                return result.ToString();
            }
        }

        /// <summary>
        ///     释放资源方法
        /// </summary>
        public void Dispose()
        {
            foreach (var pool in _pools)
            {
                pool.Value.Dispose();
                if (!_contextTypes.TryGetValue(pool.Key, out var contextTypes)) continue;
                foreach (var contextType in contextTypes)
                {
                    //搞一些输出
                    var loggerFactory = Utils.GetDependencyInjectionServiceOrNull<ILoggerFactory>(contextType);
                    loggerFactory?.CreateLogger(GetType()).LogInformation($"{pool.Value.Policy.Name} Has Destroyed!");
                }
            }
        }


        /// <summary>
        ///     获取连接池
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="factory">数据库提供工厂</param>
        /// <param name="contextType">上下文类型</param>
        /// <returns></returns>
        public DbConnectionPool GetPool(string connectionString, DbProviderFactory factory, Type contextType)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "未设置连接字符串");

            ReaderWriterLock.EnterUpgradeableReadLock();
            try
            {
                if (!_pools.ContainsKey(connectionString))
                {
                    ReaderWriterLock.EnterWriteLock();
                    try
                    {
                        if (!_pools.ContainsKey(connectionString))
                        {
                            Interlocked.Increment(ref _poolCount);
                            //初始化连接池策略
                            var policy = InitPoolPolicy(contextType);
                            //构造连接池
                            var connectionPool = new DbConnectionPool(connectionString, factory, policy);
                            //输出连接池启动信息
                            var loggerFactory = Utils.GetDependencyInjectionServiceOrNull<ILoggerFactory>(contextType);
                            loggerFactory?.CreateLogger(GetType()).Log(LogLevel.Information,
                                $"{connectionPool.Policy.Name} - Starting...");
                            //预热连接池
                            connectionPool.PrevReheatConnectionPool();
                            loggerFactory?.CreateLogger(GetType()).Log(LogLevel.Information,
                                $"{connectionPool.Policy.Name} - Start completed.");
                            //添加连接池
                            _pools.Add(connectionString, connectionPool);
                            //记录上下文类型
                            if (!_contextTypes.TryGetValue(connectionString, out var types))
                            {
                                _contextTypes.Add(connectionString, new List<Type> { contextType });
                            }
                            else
                            {
                                if (!types.Contains(contextType))
                                    types.Add(contextType);
                            }
                        }
                    }
                    finally
                    {
                        ReaderWriterLock.ExitWriteLock();
                    }
                }

                return _pools[connectionString];
            }
            finally
            {
                ReaderWriterLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        ///     初始化连接池
        /// </summary>
        /// <param name="contextType">上下文类型</param>
        private DbConnectionPoolPolicy InitPoolPolicy(Type contextType)
        {
            //默认值
            var name = $"Obase ConnectionPool-{_poolCount}";
            var poolSize = 100;
            var policy = new DbConnectionPoolPolicy
            {
                Name = name,
                PoolSize = poolSize
            };
            //从依赖注入中获取值
            var configuration =
                Utils.GetDependencyInjectionServiceOrNull<IObaseConnectionPoolConfiguration>(contextType);
            if (configuration == null) return policy;
            //接口的参数值
            if (!string.IsNullOrEmpty(configuration.Name))
                name = configuration.Name;
            if (configuration.MaximumPoolSize > 0)
                poolSize = configuration.MaximumPoolSize;
            //再改一遍
            policy = new DbConnectionPoolPolicy
            {
                Name = name,
                PoolSize = poolSize
            };
            return policy;
        }

        /// <summary>
        ///     归还连接
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="connection">连接</param>
        public void ReturnConnection(string connectionString, Object<DbConnection> connection)
        {
            if (_pools.TryGetValue(connectionString, out var pool))
                pool.Return(connection);
        }
    }
}