using System.Collections.Generic;

namespace Obase.Test.Domain.Association.Implement;

/// <summary>
///     活动
/// </summary>
public class Activity
{
    /// <summary>
    ///     活动ID
    /// </summary>
    private int _id;

    /// <summary>
    ///     活动名称
    /// </summary>
    private string _name;

    /// <summary>
    ///     礼物
    /// </summary>
    private List<Prize> _prizeList;

    /// <summary>
    ///     活动ID
    /// </summary>
    public int Id
    {
        get => _id;
        set => _id = value;
    }

    /// <summary>
    ///     活动名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     礼物
    /// </summary>
    public List<Prize> PrizeList
    {
        get => _prizeList;
        set => _prizeList = value;
    }
}