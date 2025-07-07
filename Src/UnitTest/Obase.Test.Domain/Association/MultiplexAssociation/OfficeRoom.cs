namespace Obase.Test.Domain.Association.MultiplexAssociation;

/// <summary>
///     表示办公室房间
/// </summary>
public class OfficeRoom
{
    /// <summary>
    ///     房间名称
    /// </summary>
    private string _name;

    /// <summary>
    ///     房间号
    /// </summary>
    private string _roomCode;

    /// <summary>
    ///     房间号
    /// </summary>
    public string RoomCode
    {
        get => _roomCode;
        set => _roomCode = value;
    }

    /// <summary>
    ///     房间名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     转换为字符串形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"OfficeRoom:{{RoomCode-{_roomCode},RoomName-{_name}}}";
    }
}