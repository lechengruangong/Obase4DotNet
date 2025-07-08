using System.Collections.Generic;

namespace Obase.Test.Domain.Association.MultiplexAssociation;

/// <summary>
///     表示员工
/// </summary>
public class Employee
{
    /// <summary>
    ///     员工编码
    /// </summary>
    private string _employeeCode;

    /// <summary>
    ///     管理的房间
    /// </summary>
    private List<OfficeRoom> _manageRooms;

    /// <summary>
    ///     名称
    /// </summary>
    private string _name;

    /// <summary>
    ///     工作的房间
    /// </summary>
    private OfficeRoom _workRoom;

    /// <summary>
    ///     工作的房间编码
    /// </summary>
    private string _workRoomCode;

    /// <summary>
    ///     员工编码
    /// </summary>
    public string EmployeeCode
    {
        get => _employeeCode;
        set => _employeeCode = value;
    }

    /// <summary>
    ///     名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     管理的房间
    /// </summary>
    public virtual List<OfficeRoom> ManageRooms
    {
        get => _manageRooms;
        set => _manageRooms = value;
    }

    /// <summary>
    ///     工作的房间
    /// </summary>
    public virtual OfficeRoom WorkRoom
    {
        get => _workRoom;
        set => _workRoom = value;
    }

    /// <summary>
    ///     工作的房间编码
    /// </summary>
    public string WorkRoomCode
    {
        get => _workRoomCode;
        set => _workRoomCode = value;
    }

    /// <summary>
    ///     转换为字符串形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"Employee:{{EmployeeCode-{_employeeCode},Name-{_name}}}";
    }
}