namespace Obase.Test.Domain.Association.Implement;

/// <summary>
///     车筐
/// </summary>
public class BikeBucket
{
    /// <summary>
    ///     车筐编码
    /// </summary>
    private string _code;

    /// <summary>
    ///     车筐大小
    /// </summary>
    private string _sp;

    /// <summary>
    ///     车旗子编码
    /// </summary>
    public string Code
    {
        get => _code;
        set => _code = value;
    }

    /// <summary>
    ///     车筐大小
    /// </summary>
    public string Sp
    {
        get => _sp;
        set => _sp = value;
    }
}