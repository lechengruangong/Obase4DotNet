using System;
using System.Linq;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Functional;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     测试简单类型的复杂属性
/// </summary>
[TestFixture]
public class ComplexAttributeTest
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

            //销毁所有对象
            context.CreateSet<DomesticAddress>().Delete(p => p.Key != "");

            //构造对象
            var domesticAddr = new DomesticAddress
            {
                Key = Guid.NewGuid().ToString(),
                Province = new Province { Name = "某某省", Code = 1750300 },
                City = new City { Name = "某某市", Code = 1865220 },
                Region = new Region { Name = "某某区", Code = 475900 },
                DetailAdress = "某某小区某某栋某某某某"
            };

            //附加 保存
            context.Attach(domesticAddr);
            context.SaveChanges();
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
            //销毁所有对象
            context.CreateSet<DomesticAddress>().Delete(p => p.Key != "");
        }
    }

    /// <summary>
    ///     测试增删改查
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void CurdTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //查询出来
        var queryAddr = context.CreateSet<DomesticAddress>().FirstOrDefault();
        //针对每个复杂属性进行判断
        Assert.That(queryAddr, Is.Not.Null);
        Assert.That(queryAddr.Province.Name, Is.EqualTo("某某省"));
        Assert.That(queryAddr.Province.Code, Is.EqualTo(1750300));
        Assert.That(queryAddr.City.Name, Is.EqualTo("某某市"));
        Assert.That(queryAddr.City.Code, Is.EqualTo(1865220));
        Assert.That(queryAddr.Region.Name, Is.EqualTo("某某区"));
        Assert.That(queryAddr.Region.Code, Is.EqualTo(475900));

        //修改
        queryAddr.City = new City { Code = 1865230, Name = "某某市" };
        context.SaveChanges();
        //重新查询
        queryAddr = context.CreateSet<DomesticAddress>().FirstOrDefault();
        //校验
        Assert.That(queryAddr, Is.Not.Null);
        Assert.That(queryAddr.City.Code, Is.EqualTo(1865230));
        //移除
        context.Remove(queryAddr);
        context.SaveChanges();

        queryAddr = context.CreateSet<DomesticAddress>().FirstOrDefault();
        //查询结果应该为null
        Assert.That(queryAddr, Is.Null);
    }
}