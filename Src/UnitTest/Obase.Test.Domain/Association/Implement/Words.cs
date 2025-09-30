namespace Obase.Test.Domain.Association.Implement;

/// <summary>
///     表示发言
/// </summary>
public class Words
{
    /// <summary>
    ///     发言ID
    /// </summary>
    public long WordsId { get; set; }

    /// <summary>
    ///     内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    ///     对话ID
    /// </summary>
    public long DialogueId { get; set; }

    /// <summary>
    ///     所属对话
    /// </summary>
    public Dialogue Dialogue { get; set; }
}