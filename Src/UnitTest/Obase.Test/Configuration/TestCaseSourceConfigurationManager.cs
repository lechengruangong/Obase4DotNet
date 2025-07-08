using System;
using System.Collections.Generic;
using System.Linq;
using Obase.Providers.Sql;
using Obase.Test.Infrastructure.Configuration;
using Obase.Test.Infrastructure.Context;

namespace Obase.Test.Configuration;

/// <summary>
///     测试用例数据源配置管理器
/// </summary>
public class TestCaseSourceConfigurationManager
{
    /// <summary>
    ///     获取测试用例数据源
    ///     根据获取数据源的结果来构造测试用例数据源
    /// </summary>
    public static IEnumerable<TestCaseData> DataSourceTestCases
    {
        get
        {
            //构造TestCaseData结果
            return DataSources.Select(p => new TestCaseData<EDataSource>(p)).ToList();
        }
    }

    /// <summary>
    ///     获取数据源
    ///     根据RelationshipDataBaseConfigurationManager的配置来获取测试用例数据源
    /// </summary>
    public static IEnumerable<EDataSource> DataSources
    {
        get
        {
            var result = new List<EDataSource>();
            //根据配置是否存在返回相应的数据源类型
            if (!string.IsNullOrEmpty(RelationshipDataBaseConfigurationManager.MySqlConnectionString))
                result.Add(EDataSource.MySql);

            if (!string.IsNullOrEmpty(RelationshipDataBaseConfigurationManager.SqlServerConnectionString))
                result.Add(EDataSource.SqlServer);

            if (!string.IsNullOrEmpty(RelationshipDataBaseConfigurationManager.SqliteConnectionString))
                result.Add(EDataSource.Sqlite);

            if (!string.IsNullOrEmpty(RelationshipDataBaseConfigurationManager.PostgreSqlConnectionString))
                result.Add(EDataSource.PostgreSql);

            return result;
        }
    }

    /// <summary>
    ///     获取数据源对应的对象上下文类型
    /// </summary>
    /// <param name="dataSource">数据源类型</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">无此数据源类型的上下文</exception>
    public static Type GetDataSourceContextType(EDataSource dataSource)
    {
        switch (dataSource)
        {
            case EDataSource.SqlServer:
                return typeof(SqlServerContext);
            case EDataSource.MySql:
                return typeof(MySqlContext);
            case EDataSource.Sqlite:
                return typeof(SqliteContext);
            case EDataSource.PostgreSql:
                return typeof(PostgreSqlContext);
            case EDataSource.Oracle:
            case EDataSource.Oledb:
            case EDataSource.Other:
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, $"暂无{dataSource}对应的对象上下文.");
        }
    }

    /// <summary>
    ///     获取数据源对应的插件对象上下文类型
    /// </summary>
    /// <param name="dataSource">数据源类型</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">无此数据源类型的上下文</exception>
    public static Type GetDataSourceAddonContextType(EDataSource dataSource)
    {
        switch (dataSource)
        {
            case EDataSource.SqlServer:
                return typeof(SqlServerAddonContext);
            case EDataSource.MySql:
                return typeof(MySqlAddonContext);
            case EDataSource.Sqlite:
                return typeof(SqliteAddonContext);
            case EDataSource.PostgreSql:
                return typeof(PostgreSqlAddonContext);
            case EDataSource.Oracle:
            case EDataSource.Oledb:
            case EDataSource.Other:
            default:
                throw new ArgumentOutOfRangeException(nameof(dataSource), dataSource, $"暂无{dataSource}对应的对象上下文.");
        }
    }
}