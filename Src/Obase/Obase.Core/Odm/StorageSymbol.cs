/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：存储标记.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 11:50:46
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm
{
    /// <summary>
    ///     表示存储标记。
    ///     存储标记是系统运行期间用来指代存储服务的临时标记。例如，规定某一SQL Server实例的存储标记为A，那么所有打上该标记的类型的实例都存储于该SQL
    ///     Server实例。
    /// </summary>
    public sealed class StorageSymbol
    {
        /// <summary>
        ///     用于调试的名称
        /// </summary>
        internal string DebugName { get; set; }
    }

    /// <summary>
    ///     存储标记集，包含当前系统使用的所有存储标记。
    ///     说明
    ///     StorageSymbols预定义了一批存储标记，涵盖了常用的存储服务。如果应用程序需要更多的存储服务，可以扩展本类以定义更多的存储标记。
    /// </summary>
    public class StorageSymbols
    {
        /// <summary>
        ///     指代HBase数据库的存储标记
        /// </summary>
        private readonly StorageSymbol _hBase;

        /// <summary>
        ///     指代系统使用的主数据库的存储标记
        /// </summary>
        private readonly StorageSymbol _major;

        /// <summary>
        ///     指代MemoryCache数据库的存储标记
        /// </summary>
        private readonly StorageSymbol _memoryCache;

        /// <summary>
        ///     指代MongoDB的存储标记
        /// </summary>
        private readonly StorageSymbol _mongoDb;

        /// <summary>
        ///     指代关系数据库的存储标记
        /// </summary>
        private readonly StorageSymbol _rdb;

        /// <summary>
        ///     指代Redis数据库的存储标记
        /// </summary>
        private readonly StorageSymbol _redis;

        /// <summary>
        ///     指代系统使用的从数据库的存储标记
        /// </summary>
        private readonly StorageSymbol _sub;

        /// <summary>
        ///     初始化StorageSymbols的新实例，将RDB作为默认标记。
        /// </summary>
        private StorageSymbols()
        {
            _hBase = new StorageSymbol { DebugName = "HBase" };
            _major = new StorageSymbol { DebugName = "Major" };
            _memoryCache = new StorageSymbol { DebugName = "MemoryCache" };
            _mongoDb = new StorageSymbol { DebugName = "MongoDB" };
            _rdb = new StorageSymbol { DebugName = "RDB" };
            _redis = new StorageSymbol { DebugName = "Redis" };
            _sub = new StorageSymbol { DebugName = "Sub" };
        }

        /// <summary>
        ///     获取表示默认数据库的存储标记。
        /// </summary>
        public virtual StorageSymbol Default => Rdb;

        /// <summary>
        ///     指代HBase数据库的存储标记。
        /// </summary>
        public virtual StorageSymbol HBase => _hBase;

        /// <summary>
        ///     指代系统使用的主数据库的存储标记。
        /// </summary>
        public virtual StorageSymbol Major => _major;

        /// <summary>
        ///     指代MemoryCache数据库的存储标记。
        /// </summary>
        public virtual StorageSymbol MemoryCache => _memoryCache;

        /// <summary>
        ///     指代MongoDB的存储标记。
        /// </summary>
        public virtual StorageSymbol MongoDb => _mongoDb;

        /// <summary>
        ///     指代关系数据库的存储标记。
        /// </summary>
        public virtual StorageSymbol Rdb => _rdb;

        /// <summary>
        ///     指代Redis数据库的存储标记。
        /// </summary>
        public virtual StorageSymbol Redis => _redis;

        /// <summary>
        ///     指代系统使用的从数据库的存储标记。
        /// </summary>
        public virtual StorageSymbol Sub => _sub;

        /// <summary>
        ///     获取当前应用程序域中唯一的存储标记集。
        /// </summary>
        public static StorageSymbols Current { get; } = new StorageSymbols();
    }
}