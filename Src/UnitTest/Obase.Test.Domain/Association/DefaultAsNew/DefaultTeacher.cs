namespace Obase.Test.Domain.Association.DefaultAsNew;

/// <summary>
///     测试关联端默认是否新建对象教师
/// </summary>
public class DefaultTeacher
{
    /// <summary>
    ///     教师姓名
    /// </summary>
    private string _name;

    /// <summary>
    ///     教师ID
    /// </summary>
    private long _teacherId;

    /// <summary>
    ///     教师姓名
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     教师ID
    /// </summary>
    public long TeacherId
    {
        get => _teacherId;
        set => _teacherId = value;
    }
}