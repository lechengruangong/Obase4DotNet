namespace Obase.Test.Domain.Association.Implement;

/// <summary>
///     实体礼物
/// </summary>
public class InKindPrize : Prize
{
    /// <summary>
    ///     礼物名称
    /// </summary>
    private string _name;

    /// <summary>
    ///     礼物名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }
}