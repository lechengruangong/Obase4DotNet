namespace Obase.Test.Domain.Association.Implement;

/// <summary>
///     奖品(抽象基类)
/// </summary>
public abstract class Prize
{
    /// <summary>
    ///     活动ID
    /// </summary>
    private int _activityId;

    /// <summary>
    ///     奖品ID
    /// </summary>
    private int _id;

    /// <summary>
    ///     奖品ID
    /// </summary>
    public int Id
    {
        get => _id;
        set => _id = value;
    }

    /// <summary>
    ///     活动ID
    /// </summary>
    public int ActivityId
    {
        get => _activityId;
        set => _activityId = value;
    }

    /// <summary>
    ///     抽象的显示名称
    /// </summary>
    public abstract string DisplayName { get; set; }

    /// <summary>
    ///     抽象的获取描述
    /// </summary>
    /// <param name="prefix">前缀</param>
    /// <returns></returns>
    public abstract string GetDescription(string prefix);
}