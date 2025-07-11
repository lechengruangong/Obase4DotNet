﻿using System;
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
}