using System;
using System.Linq;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Association.MultiAssociationEnd;

namespace Obase.Test.CoreTest.AssociationTest;

/// <summary>
///     多方(多个关联端)关联测试
/// </summary>
[TestFixture]
public class MultiAssociationEndTest
{
    /// <summary>
    ///     构造测试 清理数据
    /// </summary>
    [OneTimeSetUp]
    public void SetUp()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);

            //清理可能的冗余数据
            context.CreateSet<Product>().Delete(p => p.Code != "");
            context.CreateSet<Property>().Delete(p => p.Code != "");
            context.CreateSet<PropertyValue>().Delete(p => p.Code != "");
            context.CreateSet<PropertyTakingValue>().Delete(p =>
                p.ProductCode != "" || p.PropertyCode != "" || p.PropertyValueCode != "");
        }
    }

    /// <summary>
    ///     销毁方法
    /// </summary>
    [OneTimeTearDown]
    public void Dispose()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);

            //清理可能的冗余数据
            context.CreateSet<Product>().Delete(p => p.Code != "");
            context.CreateSet<Property>().Delete(p => p.Code != "");
            context.CreateSet<PropertyValue>().Delete(p => p.Code != "");
            context.CreateSet<PropertyTakingValue>().Delete(p =>
                p.ProductCode != "" || p.PropertyCode != "" || p.PropertyValueCode != "");
        }
    }

    /// <summary>
    ///     显式多方关联测试
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ExplicitTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //初始化产品
        var product1 = new Product
        {
            Code = "A",
            Name = "上衣"
        };

        var product2 = new Product
        {
            Code = "B",
            Name = "裤子"
        };

        //初始化属性
        var property = new Property
        {
            Code = "Color",
            Name = "颜色"
        };

        //初始属性值
        var propertyValue1 = new PropertyValue
        {
            Code = "ColorA",
            Value = "白色"
        };

        var propertyValue2 = new PropertyValue
        {
            Code = "ColorB",
            Value = "黑色"
        };

        //建立关系
        //产品1 有两种颜色 白色和黑色
        var propertyTakingValue1 = new PropertyTakingValue
        {
            Product = product1,
            ProductCode = product1.Code,
            Property = property,
            PropertyCode = property.Code,
            PropertyPhotoUrl = "/产品1/白色.jpg",
            PropertyValue = propertyValue1,
            PropertyValueCode = propertyValue1.Code
        };

        var propertyTakingValue2 = new PropertyTakingValue
        {
            Product = product1,
            ProductCode = product1.Code,
            Property = property,
            PropertyCode = property.Code,
            PropertyPhotoUrl = "/产品1/黑色.jpg",
            PropertyValue = propertyValue2,
            PropertyValueCode = propertyValue2.Code
        };
        //产品2有一种颜色 白色
        var propertyTakingValue3 = new PropertyTakingValue
        {
            Product = product2,
            ProductCode = product2.Code,
            Property = property,
            PropertyCode = property.Code,
            PropertyPhotoUrl = "/产品2/白色.jpg",
            PropertyValue = propertyValue1,
            PropertyValueCode = propertyValue1.Code
        };

        //附加至上下文
        context.Attach(product1);
        context.Attach(product2);
        context.Attach(property);
        context.Attach(propertyValue1);
        context.Attach(propertyValue2);
        context.Attach(propertyTakingValue1);
        context.Attach(propertyTakingValue2);
        context.Attach(propertyTakingValue3);
        //保存
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);

        //查询出来 验证属性 此处使用了Include加载PropertyTakingValue->Property和PropertyTakingValue->PropertyValue
        var productList = context.CreateSet<Product>()
            .Include(p => p.PropertyTakingValues.Select(q => q.Property))
            .Include("PropertyTakingValues.PropertyValue").ToList();

        //根据颜色代码排序
        productList.ForEach(product =>
        {
            product.PropertyTakingValues =
                product.PropertyTakingValues.OrderBy(p => p.PropertyValueCode).ToList();
        });

        //有两个商品
        Assert.That(productList.Count, Is.EqualTo(2));
        //分别是裤子和上衣
        Assert.That(productList[0].Name, Is.EqualTo("上衣"));
        Assert.That(productList[1].Name, Is.EqualTo("裤子"));
        //上衣的属性是颜色
        Assert.That(productList[0].PropertyTakingValues[0].Property.Name, Is.EqualTo("颜色"));
        Assert.That(productList[0].PropertyTakingValues[1].Property.Name, Is.EqualTo("颜色"));
        //上衣有白色和黑色
        Assert.That(productList[0].PropertyTakingValues[0].PropertyValue.Value, Is.EqualTo("白色"));
        Assert.That(productList[0].PropertyTakingValues[1].PropertyValue.Value, Is.EqualTo("黑色"));
        //图片分别是/产品1/白色.jpg /产品1/黑色.jpg
        Assert.That(productList[0].PropertyTakingValues[0].PropertyPhotoUrl, Is.EqualTo("/产品1/白色.jpg"));
        Assert.That(productList[0].PropertyTakingValues[1].PropertyPhotoUrl, Is.EqualTo("/产品1/黑色.jpg"));

        //裤子的属性是颜色
        Assert.That(productList[1].PropertyTakingValues[0].Property.Name, Is.EqualTo("颜色"));
        //裤子有白色
        Assert.That(productList[1].PropertyTakingValues[0].PropertyValue.Value, Is.EqualTo("白色"));
        //图片是/产品2/白色.jpg
        Assert.That(productList[1].PropertyTakingValues[0].PropertyPhotoUrl, Is.EqualTo("/产品2/白色.jpg"));

        //删除
        context.CreateSet<Product>().Delete(p => p.Code != "");
        context.CreateSet<Property>().Delete(p => p.Code != "");
        context.CreateSet<PropertyValue>().Delete(p => p.Code != "");
        context.CreateSet<PropertyTakingValue>().Delete(p =>
            p.ProductCode != "" || p.PropertyCode != "" || p.PropertyValueCode != "");
    }

    /// <summary>
    ///     隐式多方关联测试
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ImplicitTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //初始化产品
        var product1 = new Product
        {
            Code = "A",
            Name = "上衣"
        };

        var product2 = new Product
        {
            Code = "B",
            Name = "裤子"
        };

        //初始化属性
        var property = new Property
        {
            Code = "Color",
            Name = "颜色"
        };

        //初始属性值
        var propertyValue1 = new PropertyValue
        {
            Code = "ColorA",
            Value = "白色"
        };

        var propertyValue2 = new PropertyValue
        {
            Code = "ColorB",
            Value = "黑色"
        };

        //建立关系
        //产品1 有两种颜色 白色和黑色
        product1.PropertyValues =
        [
            new Tuple<Property, PropertyValue>(property, propertyValue1),
            new Tuple<Property, PropertyValue>(property, propertyValue2)
        ];

        //产品2有一种颜色 白色
        product2.PropertyValues = [new Tuple<Property, PropertyValue>(property, propertyValue1)];

        //附加至上下文
        context.Attach(product1);
        context.Attach(product2);
        context.Attach(property);
        context.Attach(propertyValue1);
        context.Attach(propertyValue2);
        //保存
        context.SaveChanges();

        context = ContextUtils.CreateContext(dataSource);

        //查询出来 验证属性 此处使用了Include加载PropertyTakingValue->Property和PropertyTakingValue->PropertyValue
        var productList = context.CreateSet<Product>()
            .Include(p => p.PropertyValues.Select(q => q.Item1))
            .Include("PropertyValues.Item2").ToList();

        //根据颜色代码排序
        productList.ForEach(product =>
        {
            product.PropertyValues =
                product.PropertyValues.OrderBy(p => p.Item2.Code).ToList();
        });

        //有两个商品
        Assert.That(productList.Count, Is.EqualTo(2));
        //分别是裤子和上衣
        Assert.That(productList[0].Name, Is.EqualTo("上衣"));
        Assert.That(productList[1].Name, Is.EqualTo("裤子"));
        //上衣的属性是颜色
        Assert.That(productList[0].PropertyValues[0].Item1.Name, Is.EqualTo("颜色"));
        Assert.That(productList[0].PropertyValues[1].Item1.Name, Is.EqualTo("颜色"));
        //上衣有白色和黑色
        Assert.That(productList[0].PropertyValues[0].Item2.Value, Is.EqualTo("白色"));
        Assert.That(productList[0].PropertyValues[1].Item2.Value, Is.EqualTo("黑色"));

        //裤子的属性是颜色
        Assert.That(productList[1].PropertyValues[0].Item1.Name, Is.EqualTo("颜色"));
        //裤子有白色
        Assert.That(productList[1].PropertyValues[0].Item2.Value, Is.EqualTo("白色"));

        //测试延迟加载
        context = ContextUtils.CreateContext(dataSource);

        //查询出来 验证延迟加载
        productList = context.CreateSet<Product>().ToList();

        //根据颜色代码排序
        productList.ForEach(product =>
        {
            var values = product.PropertyValues;
            product.PropertyValues = values
                .OrderBy(p => p.Item2.Code).ToList();
        });

        //有两个商品
        Assert.That(productList.Count, Is.EqualTo(2));
        //分别是裤子和上衣
        Assert.That(productList[0].Name, Is.EqualTo("上衣"));
        Assert.That(productList[1].Name, Is.EqualTo("裤子"));
        //上衣的属性是颜色
        Assert.That(productList[0].PropertyValues[0].Item1.Name, Is.EqualTo("颜色"));
        Assert.That(productList[0].PropertyValues[1].Item1.Name, Is.EqualTo("颜色"));
        //上衣有白色和黑色
        Assert.That(productList[0].PropertyValues[0].Item2.Value, Is.EqualTo("白色"));
        Assert.That(productList[0].PropertyValues[1].Item2.Value, Is.EqualTo("黑色"));

        //裤子的属性是颜色
        Assert.That(productList[1].PropertyValues[0].Item1.Name, Is.EqualTo("颜色"));
        //裤子有白色
        Assert.That(productList[1].PropertyValues[0].Item2.Value, Is.EqualTo("白色"));

        //删除
        context.CreateSet<Product>().Delete(p => p.Code != "");
        context.CreateSet<Property>().Delete(p => p.Code != "");
        context.CreateSet<PropertyValue>().Delete(p => p.Code != "");
        context.CreateSet<PropertyTakingValue>().Delete(p =>
            p.ProductCode != "" || p.PropertyCode != "" || p.PropertyValueCode != "");
    }
}