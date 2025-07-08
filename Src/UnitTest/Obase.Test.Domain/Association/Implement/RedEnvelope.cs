namespace Obase.Test.Domain.Association.Implement;

/// <summary>
///     红包
/// </summary>
public class RedEnvelope : Prize
{
    /// <summary>
    ///     数额
    /// </summary>
    private int _amount;

    /// <summary>
    ///     数额
    /// </summary>
    public int Amount
    {
        get => _amount;
        set => _amount = value;
    }
}