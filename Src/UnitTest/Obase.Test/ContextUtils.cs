using System;
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
    /// <exception cref="ArgumentException">未定义此类型上下文</exception>
    /// <exception cref="ArgumentOutOfRangeException">未知的数据源</exception>
    public static ObjectContext CreateContext(EDataSource dataSource)
    {
        switch (dataSource)
        {
            case EDataSource.SqlServer:
                return new SqlServerContext();
            case EDataSource.Oracle:
                throw new ArgumentException("暂无此类型的上下文");
            case EDataSource.Oledb:
                throw new ArgumentException("暂无此类型的上下文");
            case EDataSource.MySql:
                return new MySqlContext();
            case EDataSource.Sqlite:
                return new SqliteContext();
            case EDataSource.PostgreSql:
                return new PostgreSqlContext();
            case EDataSource.Other:
                throw new ArgumentException("暂无此类型的上下文");
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, "未知的数据源");
        }
    }
}