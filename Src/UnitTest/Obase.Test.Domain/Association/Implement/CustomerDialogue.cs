namespace Obase.Test.Domain.Association.Implement;

/// <summary>
///     客户对话
/// </summary>
public class CustomerDialogue : Dialogue
{
    /// <summary>
    ///     客户名称
    /// </summary>
    public string CustomerName { get; set; }

    /// <summary>
    ///     客户备注
    /// </summary>
    public string CustomerMemo { get; set; }
}