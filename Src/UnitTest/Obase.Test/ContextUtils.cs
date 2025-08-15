using System;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Npgsql;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Infrastructure.Context;

namespace Obase.Test;

/// <summary>
///     上下文工具
/// </summary>
public static class ContextUtils
{
    /// <summary>
    ///     创建一个新的上下文
    /// </summary>
    /// <param name="dataSource">数据源类型</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">未定义此类型上下文</exception>
    public static ObjectContext CreateContext(EDataSource dataSource)
    {
        switch (dataSource)
        {
            case EDataSource.SqlServer:
                return new SqlServerContext();
            case EDataSource.MySql:
                return new MySqlContext();
            case EDataSource.Sqlite:
                return new SqliteContext();
            case EDataSource.PostgreSql:
                return new PostgreSqlContext();
            case EDataSource.Oracle:
            case EDataSource.Oledb:
            case EDataSource.Other:
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, $"暂无{dataSource}对应的对象上下文.");
        }
    }

    /// <summary>
    ///     创建一个新的插件上下文
    /// </summary>
    /// <param name="dataSource">数据源类型</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">未定义此类型上下文</exception>
    public static ObjectContext CreateAddonContext(EDataSource dataSource)
    {
        switch (dataSource)
        {
            case EDataSource.SqlServer:
                return new SqlServerAddonContext();
            case EDataSource.MySql:
                return new MySqlAddonContext();
            case EDataSource.Sqlite:
                return new SqliteAddonContext();
            case EDataSource.PostgreSql:
                return new PostgreSqlAddonContext();
            case EDataSource.Oracle:
            case EDataSource.Oledb:
            case EDataSource.Other:
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, $"暂无{dataSource}对应的对象上下文.");
        }
    }

    /// <summary>
    ///     创建一个新的使用已有连接的上下文
    /// </summary>
    /// <param name="connection">已有的连接</param>
    /// <param name="dataSource">数据源类型</param>
    /// <param name="transaction">当前连接中已执行命令的事务,如果没有开启过事务,此项可以不传</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">未定义此类型上下文</exception>
    public static ObjectContext CreateExistingConnectionContext(DbConnection connection, EDataSource dataSource,
        DbTransaction transaction = null)
    {
        switch (dataSource)
        {
            case EDataSource.SqlServer:
                return new SqlServerExistingConnectionContext(SqlClientFactory.Instance, connection, transaction);
            case EDataSource.MySql:
                return new MySqlExistingConnectionContext(MySqlClientFactory.Instance, connection, transaction);
            case EDataSource.Sqlite:
                return new SqliteExistingConnectionContext(SqliteFactory.Instance, connection, transaction);
            case EDataSource.PostgreSql:
                return new PostgreSqlExistingConnectionContext(NpgsqlFactory.Instance, connection, transaction);
            case EDataSource.Oracle:
            case EDataSource.Oledb:
            case EDataSource.Other:
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, $"暂无{dataSource}对应的对象上下文.");
        }
    }
}