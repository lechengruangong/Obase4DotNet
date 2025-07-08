namespace Obase.Test.Domain.Functional.DataError;

/// <summary>
///     数据错误的学生测试类
///     用于测试引用是一对一但实际数据确是一对多的情况
/// </summary>
public class DataErrorStudent
{
    /// <summary>
    ///     学生名称
    /// </summary>
    private string _name;

    /// <summary>
    ///     学生id
    /// </summary>
    private long _studentId;

    /// <summary>
    ///     学生详细信息
    /// </summary>
    private DataErrorStudentInfo _studentInfo;

    /// <summary>
    ///     学生id
    /// </summary>
    public long StudentId
    {
        get => _studentId;
        set => _studentId = value;
    }

    /// <summary>
    ///     学生名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     学生详细信息
    /// </summary>
    public DataErrorStudentInfo StudentInfo
    {
        get => _studentInfo;
        set => _studentInfo = value;
    }
}