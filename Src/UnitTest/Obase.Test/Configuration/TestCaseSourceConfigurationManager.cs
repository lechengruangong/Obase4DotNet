using System.Collections.Generic;
using System.Linq;
using Obase.Providers.Sql;
using Obase.Test.Infrastructure.Configuration;

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
            //构造结果
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
}