/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：数据库连接池.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:24:54
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using SafeObjectPool;

namespace Obase.Providers.Sql.ConnectionPool
{
    /// <summary>
    ///     数据库连接池
    /// </summary>
    public class DbConnectionPool : ObjectPool<DbConnection>
    {
        /// <summary>
        ///     数据源提供工厂
        /// </summary>
        private readonly DbProviderFactory _providerFactory;

        /// <summary>
        ///     可用时调用的委托
        /// </summary>
        internal readonly Action AvailableHandler;

        /// <summary>
        ///     不可用时调用的委托
        /// </summary>
        internal readonly Action UnavailableHandler;

        /// <summary>
        ///     构造数据库连接池
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dbProviderFactory">数据提供器工厂</param>
        /// <param name="policy">连接池策略</param>
        /// <param name="availableHandler">可用时的回调</param>
        /// <param name="unavailableHandler">不可用时的回调</param>
        public DbConnectionPool(string connectionString, DbProviderFactory dbProviderFactory,
            DbConnectionPoolPolicy policy,
            Action availableHandler = null, Action unavailableHandler = null) : base(null)
        {
            AvailableHandler = availableHandler;
            UnavailableHandler = unavailableHandler;
            _providerFactory = dbProviderFactory;
            policy.Pool = this;
            policy.ConnectionString = connectionString;
            Policy = policy;
        }

        /// <summary>
        ///     数据源提供工厂
        /// </summary>
        public DbProviderFactory ProviderFactory => _providerFactory;

        /// <summary>
        ///     归还对象方法
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="exception">发生的异常</param>
        /// <param name="isRecreate">是否重置对象</param>
        public void Return(Object<DbConnection> obj, Exception exception = null, bool isRecreate = false)
        {
            if (exception != null && exception is DbException)
                try
                {
                    if (obj.Value.Ping() == false) obj.Value.Open();
                }
                catch
                {
                    SetUnavailable(exception, DateTime.Now);
                }

            Return(obj, isRecreate);
        }

        /// <summary>
        ///     预热方法
        /// </summary>
        internal void PrevReheatConnectionPool()
        {
            var minPoolSize = 5;
            var initTestOk = true;
            var initStartTime = DateTime.Now;
            var initConns = new ConcurrentBag<Object<DbConnection>>();

            try
            {
                var conn = Get();
                initConns.Add(conn);
                Policy.OnCheckAvailable(conn);
            }
            catch
            {
                //预热一次失败，后面将不进行
                initTestOk = false;
            }

            for (var a = 1; initTestOk && a < minPoolSize; a += 10)
            {
                //预热耗时超过3秒，退出
                if (initStartTime.Subtract(DateTime.Now).TotalSeconds > 3) break;
                //每10个预热
                var b = Math.Min(minPoolSize - a, 10);
                var initTasks = new Task[b];
                for (var c = 0; c < b; c++)
                    initTasks[c] = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var conn = Get();
                            initConns.Add(conn);
                            Policy.OnCheckAvailable(conn);
                        }
                        catch
                        {
                            //有失败，下一组退出预热
                            initTestOk = false;
                        }
                    });
                Task.WaitAll(initTasks);
            }

            while (initConns.TryTake(out var conn)) Return(conn);
        }
    }

    /// <summary>
    ///     扩展类 实现一个简单的ping命令
    /// </summary>
    internal static class DbConnectionExtensions
    {
        /// <summary>
        ///     针对数据库的Ping
        /// </summary>
        /// <param name="conn">连接</param>
        /// <returns></returns>
        private static DbCommand PingCommand(DbConnection conn)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandTimeout = 5;
            cmd.CommandText = "select 1";
            return cmd;
        }

        /// <summary>
        ///     扩展DbConnection
        /// </summary>
        /// <param name="that">扩展的源</param>
        /// <param name="isThrow">是否抛异常</param>
        /// <returns></returns>
        public static bool Ping(this DbConnection that, bool isThrow = false)
        {
            try
            {
                PingCommand(that).ExecuteNonQuery();
                return true;
            }
            catch
            {
                if (that.State != ConnectionState.Closed)
                    try
                    {
                        that.Close();
                    }
                    catch
                    {
                        //ignore
                    }

                if (isThrow) throw;
                return false;
            }
        }
    }
}