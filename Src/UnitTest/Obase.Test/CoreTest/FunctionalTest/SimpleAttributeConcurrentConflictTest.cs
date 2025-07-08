using Obase.Test.Configuration;
using Obase.Test.Domain.Functional;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     简单属性测试并发冲突策略
/// </summary>
[TestFixture]
public class SimpleAttributeConcurrentConflictTest
{
    /// <summary>
    ///     构造实例 为上下文赋值
    /// </summary>
    [OneTimeSetUp]
    public void SetUp()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //都是一个表 清理一次即可
            context.CreateSet<IngoreKeyValue>().Delete(p => p.Id > 0);
        }
    }


    /// <summary>
    ///     销毁对象
    /// </summary>
    [OneTimeTearDown]
    public void Dispose()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //都是一个表 清理一次即可
            context.CreateSet<IngoreKeyValue>().Delete(p => p.Id > 0);
        }
    }
}