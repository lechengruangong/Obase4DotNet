namespace Obase.Test.Domain.Association.Implement;

/// <summary>
///     特殊的我的自行车B 有一个额外的车筐
/// </summary>
public class MyBikeB : Bike
{
    /// <summary>
    ///     车筐
    /// </summary>
    private BikeBucket _bucket;

    /// <summary>
    ///     车筐编码
    /// </summary>
    private string _bucketCode;

    /// <summary>
    ///     车筐
    /// </summary>
    public virtual BikeBucket Bucket
    {
        get => _bucket;
        set => _bucket = value;
    }

    /// <summary>
    ///     车筐编码
    /// </summary>
    public string BucketCode
    {
        get => _bucketCode;
        set => _bucketCode = value;
    }
}