namespace Obase.Test.Domain.Association;

/// <summary>
///     教师的通行证
/// </summary>
public class PassPaper
{
    /// <summary>
    ///     教师ID
    /// </summary>
    private readonly long _teacherId;

    /// <summary>
    ///     通行证类型
    /// </summary>
    private readonly EPassPaperType _type;

    /// <summary>
    ///     备注
    /// </summary>
    private string _memo;

    /// <summary>
    ///     所属的教师
    /// </summary>
    private Teacher _teacher;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="teacherId">教师ID</param>
    /// <param name="type">通行证类型</param>
    public PassPaper(long teacherId, EPassPaperType type)
    {
        _teacherId = teacherId;
        _type = type;
    }

    /// <summary>
    ///     教师ID
    /// </summary>
    public long TeacherId => _teacherId;

    /// <summary>
    ///     通行证类型
    /// </summary>
    public EPassPaperType Type => _type;

    /// <summary>
    ///     备注
    /// </summary>
    public string Memo
    {
        get => _memo;
        set => _memo = value;
    }

    /// <summary>
    ///     所属的教师
    /// </summary>
    public virtual Teacher Teacher
    {
        get => _teacher;
        protected internal set => _teacher = value;
    }
}