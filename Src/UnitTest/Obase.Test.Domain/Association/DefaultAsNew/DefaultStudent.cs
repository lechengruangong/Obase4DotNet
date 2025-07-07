namespace Obase.Test.Domain.Association.DefaultAsNew;

/// <summary>
///     测试关联端默认是否新建对象学生
/// </summary>
public class DefaultStudent
{
    /// <summary>
    ///     班级ID
    /// </summary>
    private long _classId;

    /// <summary>
    ///     学生名称
    /// </summary>
    private string _name;

    /// <summary>
    ///     学生id
    /// </summary>
    private long _studentId;

    /// <summary>
    ///     学生名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     学生id
    /// </summary>
    public long StudentId
    {
        get => _studentId;
        set => _studentId = value;
    }

    /// <summary>
    ///     班级ID
    /// </summary>
    public long ClassId
    {
        get => _classId;
        set => _classId = value;
    }
}