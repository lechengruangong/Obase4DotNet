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

    /// <summary>
    ///     抽象的显示名称
    /// </summary>
    public override string DisplayName { get; set; } = "红包";

    /// <summary>
    ///     抽象的获取描述
    /// </summary>
    /// <param name="prefix">前缀</param>
    /// <returns></returns>
    public override string GetDescription(string prefix)
    {
        return $"这是一个{prefix}的红包,里面{_amount}元钱";
    }
}