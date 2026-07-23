namespace Obase.Test.Domain.Functional.KeyWords;

/// <summary>
///     Order关键字的同名类
/// </summary>
public class Order
{
    /// <summary>
    ///     主键
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    ///     名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     包装度
    /// </summary>
    public ushort Pack { get; set; }

    /// <summary>
    ///     用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    ///     用户
    /// </summary>
    public User User { get; set; }
}