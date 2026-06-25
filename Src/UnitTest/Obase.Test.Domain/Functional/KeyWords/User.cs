using System.Collections.Generic;

namespace Obase.Test.Domain.Functional.KeyWords;

/// <summary>
///     USER关键字同名类
/// </summary>
public class User
{
    /// <summary>
    ///     主键
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    ///     名称
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    ///     订单
    /// </summary>
    public List<Order> Orders { get; set; }
}