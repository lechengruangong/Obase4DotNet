using System;
using System.Collections.Generic;
using System.Linq;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Association.ExplicitlyCompion;

namespace Obase.Test.CoreTest.AssociationTest;

/// <summary>
///     伴随映射的显式关联测试
/// </summary>
[TestFixture]
public class ExplicitlyCompionTest
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
            context.CreateSet<Car>().Delete(p => p.CarCode != "");
            context.CreateSet<Wheel>().Delete(p => p.WheelCode != "");

            //初始化一辆车
            var car = new Car
            {
                CarCode = Guid.NewGuid().ToString("N"),
                CarName = "某车"
            };

            //初始化四个车轮
            var wheel1 = new Wheel
            {
                WheelCode = Guid.NewGuid().ToString("N")
            };
            var wheel2 = new Wheel
            {
                WheelCode = Guid.NewGuid().ToString("N")
            };
            var wheel3 = new Wheel
            {
                WheelCode = Guid.NewGuid().ToString("N")
            };
            var wheel4 = new Wheel
            {
                WheelCode = Guid.NewGuid().ToString("N")
            };

            //创建汽车车轮关系
            var carWheels = new List<CarWheel>
            {
                new(car, wheel1, WheelPosition.FrontLeft),
                new(car, wheel2, WheelPosition.FrontRight),
                new(car, wheel3, WheelPosition.BackLeft),
                new(car, wheel4, WheelPosition.BackRight)
            };

            //为车和车轮建立关联
            car.CarWheels = carWheels;
            //都附加至上下文
            context.Attach(car);
            context.Attach(wheel1);
            context.Attach(wheel2);
            context.Attach(wheel3);
            context.Attach(wheel4);
            foreach (var carWheel in carWheels) context.Attach(carWheel);
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
            context.CreateSet<Car>().Delete(p => p.CarCode != "");
            context.CreateSet<Wheel>().Delete(p => p.WheelCode != "");
        }
    }

    /// <summary>
    ///     测试伴随映射的显式关联查询
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void QueryTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //查询出来 使用Include一并加载CarWheels 和 Wheel
        var queryCar = context.CreateSet<Car>().Include(p => p.CarWheels.Select(q => q.Wheel)).First();
        //验证属性
        Assert.That(queryCar, Is.Not.Null);
        //有四个车轮
        Assert.That(queryCar.CarWheels.Count, Is.EqualTo(4));
        //左前为wheel1
        Assert.That(queryCar.GetWheel(WheelPosition.FrontLeft), Is.Not.Null);
        //右前为wheel2
        Assert.That(queryCar.GetWheel(WheelPosition.FrontRight), Is.Not.Null);
        //左后为wheel3
        Assert.That(queryCar.GetWheel(WheelPosition.BackLeft), Is.Not.Null);
        //右后为wheel4
        Assert.That(queryCar.GetWheel(WheelPosition.BackRight), Is.Not.Null);

        //删除
        context.CreateSet<Car>().Remove(queryCar);
        foreach (var carWheel in queryCar.CarWheels) context.CreateSet<Wheel>().Remove(carWheel.Wheel);
        //保存
        context.SaveChanges();
        //都没有对象
        var count = context.CreateSet<Car>().Count();
        Assert.That(count, Is.EqualTo(0));
        //都没有对象
        count = context.CreateSet<Wheel>().Count();
        Assert.That(count, Is.EqualTo(0));
    }
}