namespace Obase.Test.Domain.Association.Implement;

/// <summary>
///     车轮
/// </summary>
public class BikeWheel
{
    /// <summary>
    ///     车编码
    /// </summary>
    private string _bikeCode;

    /// <summary>
    ///     车轮编码
    /// </summary>
    private string _code;

    /// <summary>
    ///     车轮编码
    /// </summary>
    public string Code
    {
        get => _code;
        set => _code = value;
    }

    /// <summary>
    ///     车编码
    /// </summary>
    public string BikeCode
    {
        get => _bikeCode;
        set => _bikeCode = value;
    }
}