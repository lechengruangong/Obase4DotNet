/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：数据库连接池策略.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:23:58
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using SafeObjectPool;

namespace Obase.Providers.Sql.ConnectionPool
{
    /// <summary>
    ///     数据库连接池策略
    /// </summary>
    public class DbConnectionPoolPolicy : IPolicy<DbConnection>
    {
        /// <summary>
        ///     所属的连接池
        /// </summary>
        internal DbConnectionPool Pool;

        /// <summary>
        ///     连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>名称</summary>
        public string Name { get; set; } = "Obase ConnectionPool";

        /// <summary>池容量</summary>
        public int PoolSize { get; set; } = 100;

        /// <summary>默认获取超时设置</summary>
        public TimeSpan SyncGetTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>空闲时间，获取时若超出，则重新创建</summary>
        public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromSeconds(20);

        /// <summary>异步获取排队队列大小，小于等于0不生效</summary>
        public int AsyncGetCapacity { get; set; } = 10000;

        /// <summary>获取超时后，是否抛出异常</summary>
        public bool IsThrowGetTimeoutException { get; set; } = true;

        /// <summary>
        ///     监听 AppDomain.CurrentDomain.ProcessExit/Console.CancelKeyPress 事件自动释放
        /// </summary>
        public bool IsAutoDisposeWithSystem { get; set; } = true;

        /// <summary>后台定时检查可用性间隔秒数</summary>
        public int CheckAvailableInterval { get; set; } = 5;

        /// <summary>对象池的对象被创建时</summary>
        /// <returns>返回被创建的对象</returns>
        public DbConnection OnCreate()
        {
            var conn = Pool.ProviderFactory.CreateConnection();
            if (conn == null)
                throw new ArgumentException(
                    $"创建连接失败,【{Pool.ProviderFactory.GetType().FullName}】无法根据连接字符串【{ConnectionString}】创建连接.");
            conn.ConnectionString = ConnectionString;
            return conn;
        }

        /// <summary>销毁对象</summary>
        /// <param name="obj">资源对象</param>
        public void OnDestroy(DbConnection obj)
        {
            try
            {
                if (obj.State != ConnectionState.Closed) obj.Close();
            }
            catch
            {
                // ignored
            }

            obj.Dispose();
        }

        /// <summary>从对象池获取对象超时的时候触发，通过该方法统计</summary>
        public void OnGetTimeout()
        {
            //未处理超时
        }

        /// <summary>从对象池获取对象成功的时候触发，通过该方法统计或初始化对象</summary>
        /// <param name="obj">资源对象</param>
        public void OnGet(Object<DbConnection> obj)
        {
            if (Pool.IsAvailable)
            {
                if (obj.Value == null)
                    throw new ArgumentException(
                        $"创建连接失败,【{Pool.ProviderFactory.GetType().FullName}】无法根据连接字符串【{ConnectionString}】创建连接.");

                if (obj.Value.State != ConnectionState.Open ||
                    (DateTime.Now.Subtract(obj.LastReturnTime).TotalSeconds > 60 && obj.Value.Ping() == false))
                    try
                    {
                        obj.Value.Open();
                    }
                    catch (Exception ex)
                    {
                        if (Pool.SetUnavailable(ex, DateTime.Now))
                            throw new Exception($"【{Name}】状态不可用，等待后台检查程序恢复方可使用。{ex.Message}");
                    }
            }
        }

        /// <summary>从对象池获取对象成功的时候触发，通过该方法统计或初始化对象</summary>
        /// <param name="obj">资源对象</param>
        public async Task OnGetAsync(Object<DbConnection> obj)
        {
            if (Pool.IsAvailable)
            {
                if (obj.Value == null)
                    throw new ArgumentException(
                        $"创建连接失败,【{Pool.ProviderFactory.GetType().FullName}】无法根据连接字符串【{ConnectionString}】创建连接.");
                if (obj.Value.State != ConnectionState.Open ||
                    (DateTime.Now.Subtract(obj.LastReturnTime).TotalSeconds > 60 && obj.Value.Ping() == false))
                    try
                    {
                        await obj.Value.OpenAsync();
                    }
                    catch (Exception ex)
                    {
                        if (Pool.SetUnavailable(ex, DateTime.Now))
                            throw new Exception($"【{Name}】状态不可用，等待后台检查程序恢复方可使用。{ex.Message}");
                    }
            }
        }

        /// <summary>归还对象给对象池的时候触发</summary>
        /// <param name="obj">资源对象</param>
        public void OnReturn(Object<DbConnection> obj)
        {
            //未处理归还
        }

        /// <summary>检查可用性</summary>
        /// <param name="obj">资源对象</param>
        /// <returns></returns>
        public bool OnCheckAvailable(Object<DbConnection> obj)
        {
            if (obj.Value.Ping() == false) obj.Value.Open();
            return obj.Value.Ping();
        }

        /// <summary>事件：可用时触发</summary>
        public void OnAvailable()
        {
            Pool.AvailableHandler?.Invoke();
        }

        /// <summary>事件：不可用时触发</summary>
        public void OnUnavailable()
        {
            Pool.UnavailableHandler?.Invoke();
        }
    }
}