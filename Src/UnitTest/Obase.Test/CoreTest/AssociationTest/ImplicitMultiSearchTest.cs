using System.Linq;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Association.MultiImplicitAssociationSearch;

namespace Obase.Test.CoreTest.AssociationTest;

/// <summary>
///     隐式多对多关联搜索优化测试
/// </summary>
[TestFixture]
public class ImplicitMultiSearchTest
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
            context.CreateSet<Category>().Delete(p => p.CategoryId > 0);
            context.CreateSet<ProductCategory>().Delete(p => p.ProductCode != "" || p.CategoryId > 0);

            //构造产品分类
            var category1 = new Category
            {
                Name = "产品分类A"
            };

            var category2 = new Category
            {
                Name = "产品分类B"
            };

            var category3 = new Category
            {
                Name = "产品分类C"
            };

            //构造产品
            var product1 = new Product
            {
                Code = "CodeX",
                Categories = [category1, category2],
                Name = "产品AB"
            };

            var product2 = new Product
            {
                Code = "CodeY",
                Categories = [category2, category3],
                Name = "产品BC"
            };
            //附加
            context.Attach(category1);
            context.Attach(category2);
            context.Attach(category3);
            context.Attach(product1);
            context.Attach(product2);
            //保存
            context.SaveChanges();
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
            context.CreateSet<Category>().Delete(p => p.CategoryId > 0);
            context.CreateSet<ProductCategory>().Delete(p => p.ProductCode != "" || p.CategoryId > 0);
        }
    }

    /// <summary>
    ///     测试隐式多对多关联搜索优化
    /// </summary>
    /// <param name="dataSource"></param>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Test(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //根据产品ID查询所属的分类 对于显式化的隐式多对多 可以将关联型作为查询基点 再Include至Category
        var productCategories = context.CreateSet<ProductCategory>().Where(p => p.ProductCode == "CodeX")
            .Include(p => p.Category).ToList();

        //有两个
        Assert.That(productCategories.Count, Is.EqualTo(2));
        //分别是产品分类A 和 产品分类B
        Assert.That(productCategories.Count(p => p.Category.Name == "产品分类A"), Is.EqualTo(1));
        Assert.That(productCategories.Count(p => p.Category.Name == "产品分类B"), Is.EqualTo(1));

        //根据分类名称查询下属的产品 此处需要借助冗余属性CategoryName
        productCategories = context.CreateSet<ProductCategory>().Where(p => p.CategoryName == "产品分类B")
            .Include(p => p.Product).ToList();

        //有两个
        Assert.That(productCategories.Count, Is.EqualTo(2));
        //分别是产品AB 和 产品BC
        Assert.That(productCategories.Count(p => p.Product.Name == "产品AB"), Is.EqualTo(1));
        Assert.That(productCategories.Count(p => p.Product.Name == "产品BC"), Is.EqualTo(1));

        context = ContextUtils.CreateContext(dataSource);
        //测试表达式Include
        var products = context.CreateSet<Product>().Include(p => p.Categories).ToList();

        //有两个
        Assert.That(products.Count, Is.EqualTo(2));
        //分别是产品分类A 和 产品分类B
        Assert.That(products.First(p => p.Name == "产品AB").Categories.Count(p => p.Name == "产品分类A"), Is.EqualTo(1));
        Assert.That(products.First(p => p.Name == "产品AB").Categories.Count(p => p.Name == "产品分类B"), Is.EqualTo(1));
        //分别是产品分类C 和 产品分类B
        Assert.That(products.First(p => p.Name == "产品BC").Categories.Count(p => p.Name == "产品分类C"), Is.EqualTo(1));
        Assert.That(products.First(p => p.Name == "产品BC").Categories.Count(p => p.Name == "产品分类B"), Is.EqualTo(1));

        //测试字符串Include
        products = context.CreateSet<Product>().Include("Categories").ToList();

        //有两个
        Assert.That(products.Count, Is.EqualTo(2));
        //分别是产品分类A 和 产品分类B
        Assert.That(products.First(p => p.Name == "产品AB").Categories.Count(p => p.Name == "产品分类A"), Is.EqualTo(1));
        Assert.That(products.First(p => p.Name == "产品AB").Categories.Count(p => p.Name == "产品分类B"), Is.EqualTo(1));
        //分别是产品分类C 和 产品分类B
        Assert.That(products.First(p => p.Name == "产品BC").Categories.Count(p => p.Name == "产品分类C"), Is.EqualTo(1));
        Assert.That(products.First(p => p.Name == "产品BC").Categories.Count(p => p.Name == "产品分类B"), Is.EqualTo(1));

        context = ContextUtils.CreateContext(dataSource);
        //测试延迟加载
        products = context.CreateSet<Product>().ToList();

        //根据颜色代码排序 触发延迟加载
        products.ForEach(product =>
        {
            var values = product.Categories;
            product.Categories = values
                .OrderBy(p => p.CategoryId).ToList();
        });

        //有两个
        Assert.That(products.Count, Is.EqualTo(2));
        //分别是产品分类A 和 产品分类B
        Assert.That(products.First(p => p.Name == "产品AB").Categories.Count(p => p.Name == "产品分类A"), Is.EqualTo(1));
        Assert.That(products.First(p => p.Name == "产品AB").Categories.Count(p => p.Name == "产品分类B"), Is.EqualTo(1));
        //分别是产品分类C 和 产品分类B
        Assert.That(products.First(p => p.Name == "产品BC").Categories.Count(p => p.Name == "产品分类C"), Is.EqualTo(1));
        Assert.That(products.First(p => p.Name == "产品BC").Categories.Count(p => p.Name == "产品分类B"), Is.EqualTo(1));
    }
}