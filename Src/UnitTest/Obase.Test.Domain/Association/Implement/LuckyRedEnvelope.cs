namespace Obase.Test.Domain.Association.Implement;

/// <summary>
///     幸运红包 获得的钱翻倍
/// </summary>
public class LuckyRedEnvelope : RedEnvelope
{
    /// <summary>
    ///     实际获得金额
    /// </summary>
    private int _actual;

    /// <summary>
    ///     实际获得金额
    /// </summary>
    public int Actual
    {
        get => _actual;
        set => _actual = value;
    }
}