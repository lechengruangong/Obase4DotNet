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
}