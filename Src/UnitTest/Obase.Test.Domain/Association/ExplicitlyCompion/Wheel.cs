namespace Obase.Test.Domain.Association.ExplicitlyCompion;

/// <summary>
///     表示车轮
/// </summary>
public class Wheel
{
    /// <summary>
    ///     车轮编号
    /// </summary>
    private string _wheelCode;

    /// <summary>
    ///     车轮编号
    /// </summary>
    public string WheelCode
    {
        get => _wheelCode;
        set => _wheelCode = value;
    }
}