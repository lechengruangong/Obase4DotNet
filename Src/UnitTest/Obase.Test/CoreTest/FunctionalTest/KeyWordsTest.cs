using System.Linq;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Functional.KeyWords;

namespace Obase.Test.CoreTest.FunctionalTest;

/// <summary>
///     关键字同名类测试类
/// </summary>
[TestFixture]
public class KeyWordsTest
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
            //清理
            context.CreateSet<User>().Delete(p => p.UserId > 0);
            context.CreateSet<Order>().Delete(p => p.Code != "");

            //插入数据
            var user = new User
            {
                UserName = "张三"
            };

            var order1 = new Order
            {
                Code = "001",
                Name = "订单1",
                User = user,
                Pack = 1
            };

            var order2 = new Order
            {
                Code = "002",
                Name = "订单2",
                User = user,
                Pack = 2
            };
            //附加
            context.Attach(user);
            context.Attach(order1);
            context.Attach(order2);
            //保存
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
            //清理
            context.CreateSet<User>().Delete(p => p.UserId > 0);
            context.CreateSet<Order>().Delete(p => p.Code != "");
        }
    }

    /// <summary>
    ///     测试方法
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Test(EDataSource dataSource)
    {
        //构造上下文
        var context = ContextUtils.CreateContext(dataSource);

        var user = context.CreateSet<User>().Include(p => p.Orders).FirstOrDefault(p => p.UserName == "张三");
        //验证
        Assert.That(user, Is.Not.Null);
        Assert.That(user.Orders, Is.Not.Null);
        Assert.That(user.Orders.Count, Is.EqualTo(2));
        Assert.That(user.Orders[0].Pack, Is.EqualTo(1));
        Assert.That(user.Orders[1].Pack, Is.EqualTo(2));
    }
}