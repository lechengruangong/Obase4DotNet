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

    /// <summary>
    ///     抽象的显示名称
    /// </summary>
    public override string DisplayName
    {
        get => GetDescription("优质");
        set => _name = value;
    }

    /// <summary>
    ///     抽象的获取描述
    /// </summary>
    /// <param name="prefix">前缀</param>
    /// <returns></returns>
    public override string GetDescription(string prefix)
    {
        return $"这是一个{prefix}的礼物,里面是{_name}";
    }
}