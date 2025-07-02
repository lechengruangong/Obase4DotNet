/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：适用于PostgreSQL数据源的对象上下文配置提供程序.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:41:40
└──────────────────────────────────────────────────────────────┘
*/

using System.Data.Common;
using Npgsql;
using Obase.Providers.Sql;

namespace Obase.Providers.PostgreSql
{
    /// <summary>
    ///     适用于PostgreSQL数据源的对象上下文配置提供程序
    /// </summary>
    public abstract class PostgreSqlContextConfigProvider : SqlContextConfigProvider
    {
        /// <summary>
        ///     由派生类实现，获取特定于数据库服务器（SQL Server、Oracle等）的数据提供程序工厂。
        /// </summary>
        protected override DbProviderFactory DbProviderFactory => NpgsqlFactory.Instance;

        /// <summary>
        ///     获取数据源类型。
        /// </summary>
        protected override EDataSource SourceType => EDataSource.PostgreSql;
    }
}