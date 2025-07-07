namespace Obase.Test.Domain.Association.Implement;

/// <summary>
///     车旗子
/// </summary>
public class BikeFlag
{
    /// <summary>
    ///     车旗子编码
    /// </summary>
    private string _code;

    /// <summary>
    ///     车旗子文字
    /// </summary>
    private string _value;

    /// <summary>
    ///     车旗子编码
    /// </summary>
    public string Code
    {
        get => _code;
        set => _code = value;
    }

    /// <summary>
    ///     车旗子文字
    /// </summary>
    public string Value
    {
        get => _value;
        set => _value = value;
    }
}