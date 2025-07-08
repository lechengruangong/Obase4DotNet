namespace Obase.Test.Domain.Association.Implement;

/// <summary>
///     特殊的我的自行车A 有一个额外的旗子
/// </summary>
public class MyBikeA : Bike
{
    /// <summary>
    ///     旗子
    /// </summary>
    private BikeFlag _flag;

    /// <summary>
    ///     旗子编码
    /// </summary>
    private string _flagCode;

    /// <summary>
    ///     旗子编码
    /// </summary>
    public string FlagCode
    {
        get => _flagCode;
        set => _flagCode = value;
    }

    /// <summary>
    ///     旗子
    /// </summary>
    public virtual BikeFlag Flag
    {
        get => _flag;
        set => _flag = value;
    }
}