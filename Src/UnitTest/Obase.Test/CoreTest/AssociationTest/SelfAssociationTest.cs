using System;
using System.Linq;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Association.ExplicitlySelf;
using Obase.Test.Domain.Association.Self;

namespace Obase.Test.CoreTest.AssociationTest;

/// <summary>
///     自关联测试
///     包括显式和隐式自关联
/// </summary>
[TestFixture]
public class SelfAssociationTest
{
    /// <summary>
    ///     构造实例 装载初始数据
    /// </summary>
    [OneTimeSetUp]
    public void SetUp()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //清理可能的旧数据
            context.CreateSet<Area>().Delete(p => p.Code != "");
            context.CreateSet<FriendlyArea>().Delete(p => p.AreaCode != "");
            context.CreateSet<Guest>().Delete(p => p.GuestId > 0);
            context.CreateSet<Friend>().Delete(p => p.MySelfId > 0 || p.FriendId > 0);

            //几个区域
            var area1 = new Area
            {
                Code = "P1",
                Name = "某某省"
            };

            var area2 = new Area
            {
                Code = "C1",
                Name = "某某市A",
                ParentCode = "P1"
            };

            var area3 = new Area
            {
                Code = "C2",
                Name = "某某市B",
                ParentCode = "P1"
            };
            //C2和C3 是友好区域
            var friendly = new FriendlyArea
            {
                Area = area2,
                AreaCode = area2.Code,
                Friend = area3,
                FriendlyAreaCode = area3.Code,
                StartTime = DateTime.Now
            };

            context.Attach(area1);
            context.Attach(area2);
            context.Attach(area3);
            context.Attach(friendly);

            //初始化宾客
            var guest1 = new Guest
            {
                Name = "宾客1"
            };

            var guest2 = new Guest
            {
                Name = "宾客2"
            };

            var guest3 = new Guest
            {
                Name = "宾客3"
            };


            //建立朋友关系 此处的朋友关系是表示单向的 即分为我的朋友 和 朋友是我的人 类似于qq中 我加了你好友 但是你只是同意了 但没有加我做你的好友
            //friend1即表示guest1和guest2交了朋友 但从guest2来看 guest1是朋友是我的人并不是我的好友 因为没有创建一个MySelf是宾客2 Friend是宾客1的关系 其余同理
            var friend1 = new Friend
            {
                MeetIn = "某活动1",
                MySelf = guest1,
                FriendGuest = guest2
            };

            var friend2 = new Friend
            {
                MeetIn = "某活动2",
                MySelf = guest1,
                FriendGuest = guest3
            };

            var friend3 = new Friend
            {
                MeetIn = "某活动3",
                MySelf = guest2,
                FriendGuest = guest3
            };

            //附加至上下文
            context.Attach(guest1);
            context.Attach(guest2);
            context.Attach(guest3);
            context.Attach(friend1);
            context.Attach(friend2);
            context.Attach(friend3);

            //保存数据
            context.SaveChanges();
        }
    }

    /// <summary>
    ///     销毁
    /// </summary>
    [OneTimeTearDown]
    public void Dispose()
    {
        foreach (var dataSource in TestCaseSourceConfigurationManager.DataSources)
        {
            var context = ContextUtils.CreateContext(dataSource);
            //清理可能的旧数据
            context.CreateSet<Area>().Delete(p => p.Code != "");
            context.CreateSet<FriendlyArea>().Delete(p => p.AreaCode != "");
            context.CreateSet<Guest>().Delete(p => p.GuestId > 0);
            context.CreateSet<Friend>().Delete(p => p.MySelfId > 0 || p.FriendId > 0);
        }
    }

    /// <summary>
    ///     测试隐式自关联
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void SelfTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //查询P1区域
        var p1 = context.CreateSet<Area>().FirstOrDefault(p => p.Code == "P1");
        //P1区域的下级区域是C1和C2 第1个是C1友好区域是C2
        Assert.That(p1, Is.Not.Null);
        Assert.That(p1.SubAreas[0], Is.Not.Null);
        Assert.That(p1.SubAreas[0].FriendlyAreas[0], Is.Not.Null);
        Assert.That(p1.SubAreas[0].FriendlyAreas[0].Friend, Is.Not.Null);
        Assert.That(p1.SubAreas[0].FriendlyAreas[0].FriendlyAreaCode, Is.EqualTo("C2"));

        //查询C1区域
        var p2 = context.CreateSet<Area>().Include(p => p.ParentArea).FirstOrDefault(p => p.Code == "C1");
        //C1区域的父级区域是P1
        Assert.That(p2, Is.Not.Null);
        Assert.That(p2.ParentArea, Is.Not.Null);
    }

    /// <summary>
    ///     测试显式自关联
    /// </summary>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void ExplicitlySelfTest(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);

        //查询宾客 此处使用Include一并加载朋友 和 宾客
        var guestList = context.CreateSet<Guest>()
            .Include(p => p.MyFriends.Select(q => q.FriendGuest))
            .Include(p => p.FriendOfmes.Select(q => q.MySelf)).ToList();

        //排序
        guestList.ForEach(p =>
        {
            p.MyFriends = p.MyFriends?.OrderBy(q => q.FriendGuest.GuestId).ToList();
            p.FriendOfmes = p.FriendOfmes?.OrderBy(q => q.MySelf.GuestId).ToList();
        });

        //验证关系
        //共有三名宾客
        Assert.That(guestList.Count, Is.EqualTo(3));
        //第一名宾客 宾客1
        Assert.That(guestList[0].Name, Is.EqualTo("宾客1"));
        //宾客1有两个朋友 宾客2 和 宾客3
        Assert.That(guestList[0].MyFriends.Count, Is.EqualTo(2));
        Assert.That(guestList[0].MyFriends[0].FriendGuest.Name, Is.EqualTo("宾客2"));
        Assert.That(guestList[0].MyFriends[1].FriendGuest.Name, Is.EqualTo("宾客3"));
        //分别是在活动1和活动2里认识的
        Assert.That(guestList[0].MyFriends[0].MeetIn, Is.EqualTo("某活动1"));
        Assert.That(guestList[0].MyFriends[1].MeetIn, Is.EqualTo("某活动2"));
        //没有好友是宾客1的人
        Assert.That(guestList[0].FriendOfmes.Count, Is.EqualTo(0));

        //第二名是宾客2
        Assert.That(guestList[1].Name, Is.EqualTo("宾客2"));
        //宾客2有一个朋友 宾客3
        Assert.That(guestList[1].MyFriends.Count, Is.EqualTo(1));
        Assert.That(guestList[1].MyFriends[0].FriendGuest.Name, Is.EqualTo("宾客3"));
        //在活动3里认识的
        Assert.That(guestList[1].MyFriends[0].MeetIn, Is.EqualTo("某活动3"));
        //好友是宾客2的人只有宾客1
        Assert.That(guestList[1].FriendOfmes.Count, Is.EqualTo(1));
        Assert.That(guestList[1].FriendOfmes[0].MySelf.Name, Is.EqualTo("宾客1"));
        //在活动3里认识的
        Assert.That(guestList[1].FriendOfmes[0].MeetIn, Is.EqualTo("某活动1"));

        //第二名是宾客3
        Assert.That(guestList[2].Name, Is.EqualTo("宾客3"));
        //宾客3没有好友
        Assert.That(guestList[2].MyFriends.Count, Is.EqualTo(0));
        //好友是宾客2的人有宾客1 和 宾客2
        Assert.That(guestList[2].FriendOfmes.Count, Is.EqualTo(2));
        Assert.That(guestList[2].FriendOfmes[0].MySelf.Name, Is.EqualTo("宾客1"));
        Assert.That(guestList[2].FriendOfmes[1].MySelf.Name, Is.EqualTo("宾客2"));
        //在活动2和活动3里认识的
        Assert.That(guestList[2].FriendOfmes[0].MeetIn, Is.EqualTo("某活动2"));
        Assert.That(guestList[2].FriendOfmes[1].MeetIn, Is.EqualTo("某活动3"));
    }
}