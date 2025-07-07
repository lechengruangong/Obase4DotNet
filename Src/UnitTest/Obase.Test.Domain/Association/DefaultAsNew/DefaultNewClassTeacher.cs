namespace Obase.Test.Domain.Association.DefaultAsNew;

/// <summary>
///     测试关联端默认新建对象任课教师
/// </summary>
public class DefaultNewClassTeacher
{
    /// <summary>
    ///     班级
    /// </summary>
    private DefaultNewClass _class;

    /// <summary>
    ///     班级ID
    /// </summary>
    private long _classId;

    /// <summary>
    ///     是否是班主任
    /// </summary>
    private bool _isManage;

    /// <summary>
    ///     教师
    /// </summary>
    private DefaultTeacher _teacher;

    /// <summary>
    ///     教师ID
    /// </summary>
    private long _teacherId;

    /// <summary>
    ///     班级ID
    /// </summary>
    public long ClassId
    {
        get => _classId;
        set => _classId = value;
    }

    /// <summary>
    ///     班级
    /// </summary>
    public virtual DefaultNewClass Class
    {
        get => _class;
        set => _class = value;
    }

    /// <summary>
    ///     教师ID
    /// </summary>
    public long TeacherId
    {
        get => _teacherId > 0 ? _teacherId : Teacher?.TeacherId ?? 0;
        set => _teacherId = value;
    }

    /// <summary>
    ///     教师
    /// </summary>
    public virtual DefaultTeacher Teacher
    {
        get => _teacher;
        set => _teacher = value;
    }

    /// <summary>
    ///     是否是班主任
    /// </summary>
    public bool IsManage
    {
        get => _isManage;
        set => _isManage = value;
    }
}