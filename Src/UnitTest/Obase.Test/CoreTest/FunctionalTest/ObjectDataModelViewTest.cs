using Obase.Core.Odm;
using Obase.Providers.Sql;
using Obase.Test.Configuration;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     对象数据模型视图测试
/// </summary>
[TestFixture]
public class ObjectDataModelViewTest
{
    /// <summary>
    ///     构造实例 为上下文赋值
    /// </summary>
    [OneTimeSetUp]
    public void SetUp()
    {
        //无需设置 对象数据模型视图测试不需要预置数据
    }

    /// <summary>
    ///     销毁对象
    /// </summary>
    [OneTimeTearDown]
    public void Dispose()
    {
        //无需清理 对象数据模型视图测试不需要清理数据
    }

    /// <summary>
    ///     测试对象数据模型视
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ViewTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //创建对象数据模型视图
        var view = ObjectDataModelViewer.GetFullObjectDataModelMappingView(context);

        //验证视图是否正确
        Assert.That(view.ToString(), Is.Not.Null);
        Assert.That(view.ToString(), Is.Not.Empty);
    }
}