namespace Obase.Test.Domain.Association;

/// <summary>
///     学生
/// </summary>
public class Student : BaseStudent
{
    /// <summary>
    ///     就读班级
    /// </summary>
    private Class _class;

    /// <summary>
    ///     班级ID
    /// </summary>
    private long _classId;


    /// <summary>
    ///     学校
    /// </summary>
    private School _school;

    /// <summary>
    ///     学校ID
    /// </summary>
    private long _schoolId;


    /// <summary>
    ///     学校ID
    /// </summary>
    public long SchoolId
    {
        get => _schoolId;
        set => _schoolId = value;
    }

    /// <summary>
    ///     学校
    /// </summary>
    public virtual School School
    {
        get => _school;
        set => _school = value;
    }

    /// <summary>
    ///     就读班级ID
    /// </summary>
    public long ClassId
    {
        get => _classId;
        set => _classId = value;
    }

    /// <summary>
    ///     就读班级
    /// </summary>
    public virtual Class Class
    {
        get => _class;
        set => _class = value;
    }


    /// <summary>
    ///     转换为字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"Student:{{StudentId-{StudentId},Name-\"{Name}\",SchoolId-{_schoolId},ClassId-{_classId}}}";
    }
}