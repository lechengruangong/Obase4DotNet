using System.Linq;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Association.Implement;

namespace Obase.Test.CoreTest.AssociationTest;

/// <summary>
///     继承关系测试
/// </summary>
[TestFixture]
public class ImplementTest
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
            context.CreateSet<Bike>().Delete(p => p.Code != "");
            context.CreateSet<BikeLight>().Delete(p => p.Code != "");
            context.CreateSet<BikeWheel>().Delete(p => p.Code != "");
            context.CreateSet<BikeFlag>().Delete(p => p.Code != "");
            context.CreateSet<BikeBucket>().Delete(p => p.Code != "");
            context.CreateSet<Prize>().Delete(p => p.Id > 0);
            context.CreateSet<Activity>().Delete(p => p.Id > 0);

            //新增对象
            var bikeLight = new BikeLight
            {
                Code = "AAA-L",
                Value = 5
            };
            context.Attach(bikeLight);

            var bike = new Bike
            {
                Code = "AAA",
                LightCode = "AAA-L",
                Name = "AAA号自行车"
            };
            context.Attach(bike);

            var bikeWheel1 = new BikeWheel
            {
                Code = "AAA-W-1",
                BikeCode = "AAA"
            };
            var bikeWheel2 = new BikeWheel
            {
                Code = "AAA-W-2",
                BikeCode = "AAA"
            };
            context.Attach(bikeWheel1);
            context.Attach(bikeWheel2);

            var myBikeALight = new BikeLight
            {
                Code = "AAA-L-A",
                Value = 10
            };
            context.Attach(myBikeALight);

            var myBikeAFlag = new BikeFlag
            {
                Code = "AAA-F-A",
                Value = "I am RICH!"
            };
            context.Attach(myBikeAFlag);

            var myBikeA = new MyBikeA
            {
                Code = "AAA-A",
                LightCode = "AAA-L-A",
                Name = "AAA-A号自行车",
                FlagCode = "AAA-F-A"
            };
            context.Attach(myBikeA);

            var myBikeAWheel1 = new BikeWheel
            {
                Code = "AAA-W-A-1",
                BikeCode = "AAA-A"
            };
            var myBikeAWheel2 = new BikeWheel
            {
                Code = "AAA-W-A-2",
                BikeCode = "AAA-A"
            };
            context.Attach(myBikeAWheel1);
            context.Attach(myBikeAWheel2);

            var myBikeBLight = new BikeLight
            {
                Code = "AAA-L-B",
                Value = 15
            };
            context.Attach(myBikeBLight);

            var myBikeBBucket = new BikeBucket
            {
                Code = "AAA-B-B",
                Sp = "500cm3"
            };
            context.Attach(myBikeBBucket);

            var myBikeB = new MyBikeB
            {
                Code = "AAA-B",
                LightCode = "AAA-L-B",
                Name = "AAA-B号自行车",
                BucketCode = "AAA-B-B"
            };
            context.Attach(myBikeB);

            var myBikeBWheel1 = new BikeWheel
            {
                Code = "AAA-W-B-1",
                BikeCode = "AAA-B"
            };
            var myBikeBWheel2 = new BikeWheel
            {
                Code = "AAA-W-B-2",
                BikeCode = "AAA-B"
            };
            context.Attach(myBikeBWheel1);
            context.Attach(myBikeBWheel2);

            var myBikeCLight = new BikeLight
            {
                Code = "AAA-L-C",
                Value = 15
            };
            context.Attach(myBikeCLight);

            var myBikeCFlag = new BikeFlag
            {
                Code = "AAA-F-C",
                Value = "I am RICH!"
            };
            context.Attach(myBikeCFlag);

            var myBikeC = new MyBikeC
            {
                Code = "AAA-C",
                LightCode = "AAA-L-C",
                Name = "AAA-C号自行车",
                FlagCode = "AAA-F-C",
                CanShared = true
            };
            context.Attach(myBikeC);

            var myBikeCWheel1 = new BikeWheel
            {
                Code = "AAA-W-C-1",
                BikeCode = "AAA-C"
            };
            var myBikeCWheel2 = new BikeWheel
            {
                Code = "AAA-W-C-2",
                BikeCode = "AAA-C"
            };
            context.Attach(myBikeCWheel1);
            context.Attach(myBikeCWheel2);

            //保存
            context.SaveChanges();

            //初始化一个活动
            var activity = new Activity
            {
                Name = "某活动"
            };

            //构造两个不同的奖品
            var inKindPrize = new InKindPrize
            {
                Name = "某某奖品"
            };

            var redEnvelope = new RedEnvelope
            {
                Amount = 5
            };

            var luckyRedEnvelope = new LuckyRedEnvelope
            {
                Amount = 5,
                Actual = 10
            };

            //建立关系
            activity.PrizeList = [inKindPrize, redEnvelope, luckyRedEnvelope];

            //附加至上下文
            context.Attach(activity);
            context.Attach(inKindPrize);
            context.Attach(redEnvelope);
            context.Attach(luckyRedEnvelope);
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
            context.CreateSet<Bike>().Delete(p => p.Code != "");
            context.CreateSet<BikeLight>().Delete(p => p.Code != "");
            context.CreateSet<BikeWheel>().Delete(p => p.Code != "");
            context.CreateSet<BikeFlag>().Delete(p => p.Code != "");
            context.CreateSet<BikeBucket>().Delete(p => p.Code != "");
            context.CreateSet<Prize>().Delete(p => p.Id > 0);
            context.CreateSet<Activity>().Delete(p => p.Id > 0);
        }
    }

    /// <summary>
    ///     继承测试
    ///     测试的情景为A有B和C两个继承类 A,B,C均可被构造 并且在基类A内定义了用于区分具体类型的属性
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void CurdTest1(EDataSource dataSource)
    {
        //查询验证
        var context = ContextUtils.CreateContext(dataSource);

        //一起查出来
        var bikeList = context.CreateSet<Bike>().Include(p => p.Light).Include(p => p.Wheels).ToList();

        //有三个 分别是Bike MyBikeA MyBikeB
        Assert.That(bikeList, Is.Not.Null);
        Assert.That(bikeList.Count, Is.EqualTo(4));
        //检查每一项
        Assert.That(bikeList[0], Is.Not.Null);
        Assert.That(bikeList[0].Type, Is.EqualTo(1));
        Assert.That(bikeList[1] is MyBikeA, Is.True);
        Assert.That(bikeList[1].Type, Is.EqualTo(2));
        Assert.That(((MyBikeA)bikeList[1]).FlagCode, Is.EqualTo("AAA-F-A"));
        Assert.That(bikeList[2] is MyBikeB, Is.True);
        Assert.That(bikeList[2].Type, Is.EqualTo(3));
        Assert.That(((MyBikeB)bikeList[2]).BucketCode, Is.EqualTo("AAA-B-B"));
        Assert.That(bikeList[3] is MyBikeC, Is.True);
        Assert.That(bikeList[3].Type, Is.EqualTo(4));
        Assert.That(((MyBikeC)bikeList[3]).CanShared, Is.True);

        //检查关联对象
        foreach (var b in bikeList)
        {
            Assert.That(b.Light, Is.Not.Null);
            Assert.That(b.Wheels, Is.Not.Null);
            Assert.That(b.Wheels.Count, Is.EqualTo(2));
        }

        //只查MyBikeA
        var myBikeAList = context.CreateSet<MyBikeA>().Include(p => p.Light).Include(p => p.Wheels)
            .Include(p => p.Flag).ToList();

        //两个 一个A 一个C
        Assert.That(myBikeAList, Is.Not.Null);
        Assert.That(myBikeAList.Count, Is.EqualTo(2));
        //A
        Assert.That(myBikeAList[0], Is.Not.Null);
        Assert.That(myBikeAList[0].Type, Is.EqualTo(2));
        //C
        Assert.That(myBikeAList[1], Is.Not.Null);
        Assert.That(myBikeAList[1].Type, Is.EqualTo(4));

        //检查关联对象
        Assert.That(myBikeAList[0].Light, Is.Not.Null);
        Assert.That(myBikeAList[0].Wheels, Is.Not.Null);
        Assert.That(myBikeAList[0].Wheels.Count, Is.EqualTo(2));
        Assert.That(myBikeAList[0].Flag, Is.Not.Null);

        Assert.That(myBikeAList[1].Light, Is.Not.Null);
        Assert.That(myBikeAList[1].Wheels, Is.Not.Null);
        Assert.That(myBikeAList[1].Wheels.Count, Is.EqualTo(2));
        Assert.That(myBikeAList[1].Flag, Is.Not.Null);

        //只查MyBikeB
        var myBikeBList = context.CreateSet<MyBikeB>().Include(p => p.Light).Include(p => p.Wheels)
            .Include(p => p.Bucket).ToList();

        //只有一个
        Assert.That(myBikeBList, Is.Not.Null);
        Assert.That(myBikeBList.Count, Is.EqualTo(1));

        Assert.That(myBikeBList[0], Is.Not.Null);
        Assert.That(myBikeBList[0].Type, Is.EqualTo(3));

        //检查关联对象
        Assert.That(myBikeBList[0].Light, Is.Not.Null);
        Assert.That(myBikeBList[0].Wheels, Is.Not.Null);
        Assert.That(myBikeBList[0].Wheels.Count, Is.EqualTo(2));
        Assert.That(myBikeBList[0].Bucket, Is.Not.Null);

        //修改对象
        context = ContextUtils.CreateContext(dataSource);

        var qMyBikeA = context.CreateSet<MyBikeA>().FirstOrDefault();

        Assert.That(qMyBikeA, Is.Not.Null);
        //修改普通属性
        qMyBikeA.Name = "AAA-A号自行车-New";

        context.SaveChanges();

        //检查修改的值
        qMyBikeA = context.CreateSet<MyBikeA>().FirstOrDefault();

        Assert.That(qMyBikeA, Is.Not.Null);
        Assert.That(qMyBikeA.Name, Is.EqualTo("AAA-A号自行车-New"));

        //删除对象
        context = ContextUtils.CreateContext(dataSource);

        bikeList = context.CreateSet<Bike>().ToList();
        //移除
        foreach (var b in bikeList) context.Remove(b);
        //保存
        context.SaveChanges();

        //验证
        var count = context.CreateSet<Bike>().Count();

        Assert.That(count, Is.EqualTo(0));
    }

    /// <summary>
    ///     继承测试
    ///     测试的情景为A有B和C两个继承类 A是抽象的 B,C可被构造 并且没有在基类A内定义了用于区分具体类型的属性
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void CurdTest2(EDataSource dataSource)
    {
        //查询出来验证
        var context = ContextUtils.CreateContext(dataSource);
        //一并加载奖品
        var queryActivity = context.CreateSet<Activity>().Include(p => p.PrizeList).First();
        //不为空
        Assert.That(queryActivity, Is.Not.Null);
        //有两个奖品
        Assert.That(queryActivity.PrizeList.Count, Is.EqualTo(3));
        //实体奖品有一个
        Assert.That(queryActivity.PrizeList.Count(p => p is InKindPrize), Is.EqualTo(1));
        //是某某奖品
        Assert.That(queryActivity.PrizeList.Where(p => p is InKindPrize).Cast<InKindPrize>().First().Name,
            Is.EqualTo("某某奖品"));
        //红包有两个
        Assert.That(queryActivity.PrizeList.Count(p => p is RedEnvelope), Is.EqualTo(2));
        //一个是5块钱的红包
        Assert.That(queryActivity.PrizeList.Where(p => p is RedEnvelope).Cast<RedEnvelope>().First().Amount,
            Is.EqualTo(5));
        //幸运红包有一个
        Assert.That(queryActivity.PrizeList.Count(p => p is LuckyRedEnvelope), Is.EqualTo(1));
        //5块钱
        Assert.That(queryActivity.PrizeList.Where(p => p is LuckyRedEnvelope).Cast<LuckyRedEnvelope>().First().Amount,
            Is.EqualTo(5));
        //实际翻倍了
        Assert.That(queryActivity.PrizeList.Where(p => p is LuckyRedEnvelope).Cast<LuckyRedEnvelope>().First().Actual,
            Is.EqualTo(10));

        //查询出来验证
        context = ContextUtils.CreateContext(dataSource);
        //根据某个具体类型查询
        var qInKindPrize = context.CreateSet<InKindPrize>().FirstOrDefault();
        //不为空
        Assert.That(qInKindPrize, Is.Not.Null);
        Assert.That(qInKindPrize.Name, Is.EqualTo("某某奖品"));

        //根据某个具体类型查询
        var qRedEnvelope = context.CreateSet<RedEnvelope>().FirstOrDefault();
        //不为空
        Assert.That(qRedEnvelope, Is.Not.Null);
        Assert.That(qRedEnvelope.Amount, Is.EqualTo(5));
        Assert.That(qRedEnvelope.DisplayName, Is.EqualTo("红包"));

        //根据某个具体类型查询
        var qLuckyRedEnvelope = context.CreateSet<LuckyRedEnvelope>().FirstOrDefault();
        //不为空
        Assert.That(qLuckyRedEnvelope, Is.Not.Null);
        Assert.That(qLuckyRedEnvelope.Amount, Is.EqualTo(5));
        Assert.That(qLuckyRedEnvelope.Actual, Is.EqualTo(10));

        //修改
        qInKindPrize.Name = "某某奖品-New";
        qRedEnvelope.Amount = 2;
        qLuckyRedEnvelope.Amount = 10;
        qLuckyRedEnvelope.Actual = 20;
        context.SaveChanges();

        //查询出来验证
        context = ContextUtils.CreateContext(dataSource);
        //根据某个具体类型查询
        var qInKindPrizes = context.CreateSet<InKindPrize>().ToList();
        //不为空
        Assert.That(qInKindPrizes, Is.Not.Null);
        Assert.That(qInKindPrizes.Count, Is.EqualTo(1));
        Assert.That(qInKindPrizes[0].Name, Is.EqualTo("某某奖品-New"));
        Assert.That(qInKindPrizes[0].DisplayName, Is.EqualTo("这是一个优质的礼物,里面是某某奖品-New"));

        //根据某个具体类型查询
        var qRedEnvelopes = context.CreateSet<RedEnvelope>().ToList();
        //不为空
        Assert.That(qRedEnvelopes, Is.Not.Null);
        Assert.That(qRedEnvelopes.Count, Is.EqualTo(2));
        Assert.That(qRedEnvelopes[0].Amount, Is.EqualTo(2));
        Assert.That(qRedEnvelopes[1].Amount, Is.EqualTo(10));
        Assert.That(((LuckyRedEnvelope)qRedEnvelopes[1]).Actual, Is.EqualTo(20));

        //删除
        context = ContextUtils.CreateContext(dataSource);
        //一并加载奖品
        var queryActivitys = context.CreateSet<Activity>().Include(p => p.PrizeList).ToList();
        foreach (var query in queryActivitys)
        {
            Assert.That(query, Is.Not.Null);
            context.Remove(query);
            foreach (var priz in query.PrizeList)
            {
                Assert.That(priz, Is.Not.Null);
                context.Remove(priz);
            }
        }

        context.SaveChanges();
        //验证
        var count = context.CreateSet<Prize>().Count();
        Assert.That(count, Is.EqualTo(0));

        count = context.CreateSet<Activity>().Count();
        Assert.That(count, Is.EqualTo(0));
    }
}