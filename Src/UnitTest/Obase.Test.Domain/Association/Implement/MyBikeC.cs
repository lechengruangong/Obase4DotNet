namespace Obase.Test.Domain.Association.Implement;

/// <summary>
///     特殊的我的自行车C 是可以共享的
/// </summary>
public class MyBikeC : MyBikeA
{
    /// <summary>
    ///     是否可共享
    /// </summary>
    private bool _canShared;


    /// <summary>
    ///     是否可共享
    /// </summary>
    public bool CanShared
    {
        get => _canShared;
        set => _canShared = value;
    }
}