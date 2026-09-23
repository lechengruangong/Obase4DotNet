using Obase.Core.Odm;
using Obase.Providers.Sql;
using Obase.Providers.Sql.ConnectionPool;
using Obase.Test.Configuration;
using Obase.Test.Infrastructure.Context;

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

        //获取当前连接池的信息
        var statistics = ObaseConnectionPool.Current.Statistics;
        Assert.That(statistics, Is.Not.Null);
        Assert.That(statistics, Is.Not.Empty);

        //获取当前连接池的完整信息
        var statisticsFully = ObaseConnectionPool.Current.StatisticsFully;
        Assert.That(statisticsFully, Is.Not.Null);
        Assert.That(statisticsFully, Is.Not.Empty);
    }

    /// <summary>
    ///     测试ODM验证器 模型合法时返回模型的完整视图
    /// </summary>
    [Test]
    public void ValidatorTest()
    {
        //使用简单的ODM验证器验证核心模型
        var result = new SimpleOdmValidator().Validate();

        //验证通过
        Assert.That(result.IsValid, Is.True);
        //返回的是对象数据模型的完整视图
        Assert.That(result.Message, Is.Not.Null);
        Assert.That(result.Message, Is.Not.Empty);
        Assert.That(result.Message, Does.Contain("本模型共包含"));
        //没有任何完整性检查错误信息
        Assert.That(result.Message, Does.Not.Contain(IntegrityCheckFailException.OdmMessagePrefix));
        Assert.That(result.Message, Does.Not.Contain(IntegrityCheckFailException.SodmMessagePrefix));
    }

    /// <summary>
    ///     测试ODM验证器 ODM完整性检查未通过时生成带ODM前缀的错误信息
    /// </summary>
    [Test]
    public void OdmErrorMessageTest()
    {
        //使用一个未配置主键的模型进行验证
        var result = new InvalidOdmValidator().Validate();

        //验证未通过
        Assert.That(result.IsValid, Is.False);
        //错误信息带ODM前缀
        Assert.That(result.Message, Does.Contain(IntegrityCheckFailException.OdmMessagePrefix));
        //不含有SODM的错误信息
        Assert.That(result.Message, Does.Not.Contain(IntegrityCheckFailException.SodmMessagePrefix));
        //按照类型分组输出 包含类型名称 错误个数和具体的错误信息
        Assert.That(result.Message, Does.Contain("NullableJavaBean"));
        Assert.That(result.Message, Does.Contain("未配置主键"));
        Assert.That(result.Message, Does.Contain("1. "));
    }

    /// <summary>
    ///     测试ODM验证器 SODM完整性检查未通过时生成带SODM前缀的错误信息
    /// </summary>
    [Test]
    public void SodmErrorMessageTest()
    {
        //使用一个序列化模型构造器参数配置不全的模型进行验证
        var result = new InvalidSodmValidator().Validate();

        //验证未通过
        Assert.That(result.IsValid, Is.False);
        //错误信息带SODM前缀
        Assert.That(result.Message, Does.Contain(IntegrityCheckFailException.SodmMessagePrefix));
        //不含有ODM的错误信息
        Assert.That(result.Message, Does.Not.Contain(IntegrityCheckFailException.OdmMessagePrefix));
        //包含具体的错误信息
        Assert.That(result.Message, Does.Contain("构造器应有"));
    }
}