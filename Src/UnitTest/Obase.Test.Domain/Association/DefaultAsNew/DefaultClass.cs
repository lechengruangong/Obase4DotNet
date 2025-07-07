using System.Collections.Generic;

namespace Obase.Test.Domain.Association.DefaultAsNew;

/// <summary>
///     关联端默认不创建新对象的班级
/// </summary>
public class DefaultClass
{
    /// <summary>
    ///     班级id
    /// </summary>
    private long _classId;

    /// <summary>
    ///     班级任课老师
    /// </summary>
    private List<DefaultClassTeacher> _classTeachers;

    /// <summary>
    ///     班级名称
    /// </summary>
    private string _name;

    /// <summary>
    ///     学校
    /// </summary>
    private DefaultSchool _school;

    /// <summary>
    ///     学校ID
    /// </summary>
    private long _schoolId;

    /// <summary>
    ///     学生
    /// </summary>
    private List<DefaultStudent> _students;

    /// <summary>
    ///     班级id
    /// </summary>
    public long ClassId
    {
        get => _classId;
        set => _classId = value;
    }

    /// <summary>
    ///     班级名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     学生
    /// </summary>
    public List<DefaultStudent> Students
    {
        get => _students;
        set => _students = value;
    }

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
    public DefaultSchool School
    {
        get => _school;
        set => _school = value;
    }

    /// <summary>
    ///     班级任课老师
    /// </summary>
    public List<DefaultClassTeacher> ClassTeachers
    {
        get => _classTeachers;
        set => _classTeachers = value;
    }
}