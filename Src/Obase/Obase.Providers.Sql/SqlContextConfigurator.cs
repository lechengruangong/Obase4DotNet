/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：需要自主控制Sql执行器的对象上下文配置提供程序.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:36:52
└──────────────────────────────────────────────────────────────┘
*/


using Obase.Core;
using Obase.Core.Odm;

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     适用于SQL数据源的对象上下文配置提供程序。
    ///     建议仅在需要自主控制Sql执行器（如需要在对象上下文之外管理事务）时使用本实现，通常情况下强烈建议使用SqlContextConfigProvider
    /// </summary>
    public abstract class SqlContextConfigurator : ContextConfigProvider
    {
        /// <summary>
        ///     指示是否启用存储架构映射。
        /// </summary>
        private readonly bool _enableStructMapping;

        /// <summary>
        ///     Sql语句执行器
        /// </summary>
        private readonly ISqlExecutor _sqlExecutor;

        /// <summary>
        ///     使用指定的Sql执行器初始化SqlContextConfigurator的新实例。
        /// </summary>
        /// <param name="sqlExecutor">Sql语句执行器。</param>
        /// <param name="enableStructMapping">指示是否启用存储架构映射</param>
        protected SqlContextConfigurator(ISqlExecutor sqlExecutor, bool enableStructMapping = false)
        {
            _sqlExecutor = sqlExecutor;
            _enableStructMapping = enableStructMapping;
        }

        /// <summary>
        ///     由派生类实现，创建指定存储标记对应的存储提供程序。
        /// </summary>
        /// <returns>存储提供程序。</returns>
        /// <param name="symbol">存储标记。</param>
        /// <param name="model">对象数据模型。</param>
        protected override IStorageProvider CreateStorageProvider(StorageSymbol symbol, ObjectDataModel model)
        {
            return new SqlStorageProvider(_sqlExecutor);
        }

        /// <summary>
        ///     创建面向Sql服务器的存储结构映射提供程序。
        /// </summary>
        /// <param name="storageSymbol">存储标记。</param>
        protected override IStorageStructMappingProvider CreateStorageStructMappingProvider(StorageSymbol storageSymbol)
        {
            return _enableStructMapping ? new SqlStorageStructMappingProvider(_sqlExecutor) : null;
        }
    }
}