/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：适用于SQL数据源的对象上下文配置提供程序.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-1 16:35:22
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Data.Common;
using Obase.Core;
using Obase.Core.Odm;

namespace Obase.Providers.Sql
{
    /// <summary>
    ///     适用于SQL数据源的对象上下文配置提供程序。
    /// </summary>
    public abstract class SqlContextConfigProvider : ContextConfigProvider
    {
        /// <summary>
        ///     由派生类实现，获取特定于数据库服务器（SQL Server、Oracle等）的数据提供程序工厂。
        /// </summary>
        protected internal abstract DbProviderFactory DbProviderFactory { get; }

        /// <summary>
        ///     由派生类实现，获取数据库连接字符串。
        /// </summary>
        protected internal abstract string ConnectionString { get; }

        /// <summary>
        ///     获取数据源类型。
        /// </summary>
        protected virtual EDataSource SourceType => (EDataSource)(-1);

        /// <summary>
        ///     获取一个值，该值指示是否启用存储结构映射
        /// </summary>
        protected virtual bool EnableStructMapping => false;

        /// <summary>
        ///     由派生类实现，创建指定存储标记对应的存储提供程序。
        /// </summary>
        /// <param name="symbol">存储标记</param>
        /// <param name="model">对象数据模型</param>
        /// <returns></returns>
        protected override IStorageProvider CreateStorageProvider(StorageSymbol symbol, ObjectDataModel model)
        {
            var sourceType = SourceType;

            //未重写SourceType
            if ((int)sourceType == -1)
            {
                var targetSourceName = DbProviderFactory.GetType().FullName;

                if (targetSourceName != null)
                    switch (targetSourceName.ToLower())
                    {
                        case "system.data.sqlclient.sqlclientfactory":
                            sourceType = EDataSource.SqlServer;
                            break;
                        case "oracle.manageddataaccess.client.oracleclientfactory":
                            sourceType = EDataSource.Oracle;
                            break;
                        case "mysql.data.mysqlclient.mysqlclientfactory":
                        case "mysqlconnector.mysqlconnector.mysqlconnectorfactory ":
                            sourceType = EDataSource.MySql;
                            break;
                        case "system.data.sqlite.sqlitefactory":
                        case "microsoft.data.sqlite.sqlitefactory":
                            sourceType = EDataSource.Sqlite;
                            break;
                        case "npgsql.npgsqlfactory":
                            sourceType = EDataSource.PostgreSql;
                            break;
                        default:
                            throw new ArgumentException(
                                $"未能识别数据提供程序工厂{DbProviderFactory}的名称,请重写SqlContextConfigProvider.SourceType显式指定数据源类型");
                    }
                else
                    throw new ArgumentException(
                        $"未能识别数据提供程序工厂{DbProviderFactory}的名称,请重写SqlContextConfigProvider.SourceType显式指定数据源类型");
            }

            var sqlExecutor =
                new StandardSqlExecutor(DbProviderFactory, sourceType, ConnectionString, ObjectContext.GetType());
            var sqlStorageProvider = new SqlStorageProvider(sqlExecutor);

            return sqlStorageProvider;
        }

        /// <summary>
        ///     创建面向Sql服务器的存储结构映射提供程序。
        /// </summary>
        /// <param name="storageSymbol">存储标记。</param>
        protected override IStorageStructMappingProvider CreateStorageStructMappingProvider(StorageSymbol storageSymbol)
        {
            if (EnableStructMapping)
            {
                var sourceType = SourceType;

                //未重写SourceType
                if ((int)sourceType == -1)
                {
                    var targetSourceName = DbProviderFactory.GetType().FullName;

                    if (targetSourceName != null)
                        switch (targetSourceName.ToLower())
                        {
                            case "system.data.sqlclient.sqlclientfactory":
                                sourceType = EDataSource.SqlServer;
                                break;
                            case "oracle.manageddataaccess.client.oracleclientfactory":
                                sourceType = EDataSource.Oracle;
                                break;
                            case "mysql.data.mysqlclient.mysqlclientfactory":
                            case "mysqlconnector.mysqlconnector.mysqlconnectorfactory ":
                                sourceType = EDataSource.MySql;
                                break;
                            case "system.data.sqlite.sqlitefactory":
                            case "microsoft.data.sqlite.sqlitefactory":
                                sourceType = EDataSource.Sqlite;
                                break;
                            case "npgsql.npgsqlfactory":
                                sourceType = EDataSource.PostgreSql;
                                break;
                            default:
                                throw new ArgumentException(
                                    $"未能识别数据提供程序工厂{DbProviderFactory}的名称,请重写SqlContextConfigProvider.SourceType显式指定数据源类型");
                        }
                    else
                        throw new ArgumentException(
                            $"未能识别数据提供程序工厂{DbProviderFactory}的名称,请重写SqlContextConfigProvider.SourceType显式指定数据源类型");
                }

                return new SqlStorageStructMappingProvider(new StandardSqlExecutor(DbProviderFactory, sourceType,
                    ConnectionString, ObjectContext.GetType()));
            }

            return null;
        }
    }
}