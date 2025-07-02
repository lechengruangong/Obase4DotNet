/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：适用于Sqlite数据源的对象上下文配置提供程序.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:44:46
└──────────────────────────────────────────────────────────────┘
*/

using System.Data.Common;
using Microsoft.Data.Sqlite;
using Obase.Providers.Sql;

namespace Obase.Providers.Sqlite
{
    /// <summary>
    ///     适用于Sqlite数据源的对象上下文配置提供程序
    /// </summary>
    public abstract class SqliteContextConfigProvider : SqlContextConfigProvider
    {
        /// <summary>
        ///     由派生类实现，获取特定于数据库服务器（SQL Server、Oracle等）的数据提供程序工厂。
        /// </summary>
        protected override DbProviderFactory DbProviderFactory => SqliteFactory.Instance;

        /// <summary>
        ///     获取数据源类型。
        /// </summary>
        protected override EDataSource SourceType => EDataSource.Sqlite;
    }
}