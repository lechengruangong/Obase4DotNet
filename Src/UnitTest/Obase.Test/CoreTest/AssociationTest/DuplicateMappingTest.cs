using System.Linq;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Association.DuplicateMapping;

namespace Obase.Test.CoreTest.AssociationTest;

/// <summary>
///     在关联型中有重复的映射测试
/// </summary>
[TestFixture]
public class DuplicateMappingTest
{
    /// <summary>
    ///     构造实例 装载初始对象
    /// </summary>
    [OneTimeSetUp]
    public void SetUp()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //清理可能的冗余数据
            context.CreateSet<GoodsAttribute>().Delete(p => p.AttributeId > 0 || p.GoodsId > 0);
            context.CreateSet<SelectableValue>().Delete(p => p.AttributeId > 0 || p.CategoryId > 0);
            context.CreateSet<StandardValue>().Delete(p => p.AttributeId > 0 || p.CategoryId > 0 || p.GoodsId > 0);

            context = ContextUtils.CreateContext(dataSource);

            //加入测试数据
            var gen = new TimeBasedIdGenerator();

            var goodsAttr = new GoodsAttribute(gen.Next(), gen.Next())
            {
                InputValue = "测试输入值"
            };
            var selectableValue = new SelectableValue(gen.Next(), goodsAttr.AttributeId)
            {
                Alias = "测试属性值",
                Sequence = 10
            };

            context.Attach(goodsAttr);
            context.Attach(selectableValue);

            context.SaveChanges();
        }
    }

    /// <summary>
    ///     销毁
    /// </summary>
    [OneTimeTearDown]
    public void TearDown()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);

            //清理可能的冗余数据
            context.CreateSet<GoodsAttribute>().Delete(p => p.AttributeId > 0 || p.GoodsId > 0);
            context.CreateSet<SelectableValue>().Delete(p => p.AttributeId > 0 || p.CategoryId > 0);
            context.CreateSet<StandardValue>().Delete(p => p.AttributeId > 0 || p.CategoryId > 0 || p.GoodsId > 0);
        }
    }

    /// <summary>
    ///     测试关联型中有重复的映射
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Test(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //查询GoodsAttribute
        var goodsAttr = context.CreateSet<GoodsAttribute>().FirstOrDefault();
        //检测值
        Assert.That(goodsAttr, Is.Not.Null);
        Assert.That(goodsAttr.InputValue, Is.EqualTo("测试输入值"));

        //查询SelectableValue
        var selectableValues = context.CreateSet<SelectableValue>().FirstOrDefault();
        //检测值
        Assert.That(selectableValues, Is.Not.Null);
        Assert.That(selectableValues.Alias, Is.EqualTo("测试属性值"));
        Assert.That(selectableValues.Sequence, Is.EqualTo(10));

        //新增StandardValue
        var standardValue = new StandardValue(goodsAttr, selectableValues)
            { Alias = "某某", Photo = "1.jpg" };
        context.Attach(standardValue);
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);

        //查询StandardValue
        var qstandardValue = context.CreateSet<StandardValue>().Include(p => p.GoodsAttribute)
            .Include(p => p.SelectedValue).FirstOrDefault();
        //检测值
        Assert.That(qstandardValue, Is.Not.Null);
        Assert.That(qstandardValue.Alias, Is.EqualTo("某某"));
        Assert.That(qstandardValue.Photo, Is.EqualTo("1.jpg"));
        Assert.That(qstandardValue.GoodsAttribute, Is.Not.Null);
        Assert.That(qstandardValue.GoodsAttribute.InputValue, Is.EqualTo("测试输入值"));
        Assert.That(qstandardValue.SelectedValue, Is.Not.Null);
        Assert.That(qstandardValue.SelectedValue.Alias, Is.EqualTo("测试属性值"));
        Assert.That(qstandardValue.SelectedValue.Sequence, Is.EqualTo(10));
    }
}