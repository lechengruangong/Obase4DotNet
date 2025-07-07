namespace Obase.Test.Domain.Association.Implement;

/// <summary>
///     车灯
/// </summary>
public class BikeLight
{
    /// <summary>
    ///     车灯编码
    /// </summary>
    private string _code;

    /// <summary>
    ///     亮度
    /// </summary>
    private int _value;

    /// <summary>
    ///     车灯编码
    /// </summary>
    public string Code
    {
        get => _code;
        set => _code = value;
    }

    /// <summary>
    ///     亮度
    /// </summary>
    public int Value
    {
        get => _value;
        set => _value = value;
    }
}