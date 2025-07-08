using System.Linq;
using Obase.Core;
using Obase.Providers.Sql;
using Obase.Test.Configuration;
using Obase.Test.Domain.Association.MultiplexAssociation;

namespace Obase.Test.CoreTest.AssociationTest;

/// <summary>
///     两个类之间有多种关系测试
/// </summary>
[TestFixture]
public class MultiplexAssociationTest
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
            var employees = context.CreateSet<Employee>().Include(p => p.ManageRooms).ToList();
            foreach (var emp in employees) emp.ManageRooms?.Clear();
            context.SaveChanges();

            context.CreateSet<OfficeRoom>().Delete(p => p.RoomCode != "");
            context.CreateSet<Employee>().Delete(p => p.EmployeeCode != "");
            //加入员工和房间
            var room1 = new OfficeRoom { RoomCode = "L101", Name = "某房间1" };
            var room2 = new OfficeRoom { RoomCode = "L102", Name = "某房间2" };
            var room3 = new OfficeRoom { RoomCode = "L103", Name = "某房间3" };
            var employee = new Employee
            {
                EmployeeCode = "A01",
                Name = "某员工",
                ManageRooms = [room2, room3],
                WorkRoom = room1
            };

            context.Attach(room1);
            context.Attach(room2);
            context.Attach(room3);
            context.Attach(employee);
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
            var employees = context.CreateSet<Employee>().Include(p => p.ManageRooms).ToList();
            foreach (var emp in employees) emp.ManageRooms?.Clear();
            context.SaveChanges();

            context.CreateSet<OfficeRoom>().Delete(p => p.RoomCode != "");
            context.CreateSet<Employee>().Delete(p => p.EmployeeCode != "");
        }
    }

    /// <summary>
    ///     测试两个类之间有多种关系
    /// </summary>
    /// <param name="dataSource"></param>
    [TestCaseSource(typeof(TestCaseSourceConfigurationManager),
        nameof(TestCaseSourceConfigurationManager.DataSourceTestCases))]
    public void Test(EDataSource dataSource)
    {
        var context = ContextUtils.CreateContext(dataSource);
        //查询员工 加载工作房间和管理的房间
        var employee = context.CreateSet<Employee>().Include(p => p.WorkRoom).Include(p => p.ManageRooms)
            .FirstOrDefault();
        //检查各个关联属性 有2个管理的房间 1个工作房间
        Assert.That(employee, Is.Not.Null);
        Assert.That(employee.WorkRoom, Is.Not.Null);
        Assert.That(employee.ManageRooms, Is.Not.Null);
        Assert.That(employee.ManageRooms.Count, Is.EqualTo(2));

        //测试投影查询
        var rooms = context.CreateSet<Employee>().Where(p => p.Name == "某员工").SelectMany(p => p.ManageRooms)
            .Where(p => p.Name == "某房间2").ToList();
        //检查投影查询结果
        Assert.That(rooms, Is.Not.Null);
        Assert.That(rooms.Count, Is.EqualTo(1));
        Assert.That(rooms[0].RoomCode, Is.EqualTo("L102"));
    }
}