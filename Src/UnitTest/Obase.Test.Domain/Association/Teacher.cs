using System.Collections.Generic;

namespace Obase.Test.Domain.Association;

/// <summary>
///     教师
/// </summary>
public class Teacher
{
    /// <summary>
    ///     教师姓名
    /// </summary>
    private string _name;

    /// <summary>
    ///     所拥有的的通行证
    /// </summary>
    private List<PassPaper> _passPaperList;

    /// <summary>
    ///     所属学校
    /// </summary>
    private School _school;

    /// <summary>
    ///     学校ID
    /// </summary>
    private long _schoolId;

    /// <summary>
    ///     教师ID
    /// </summary>
    private long _teacherId;

    /// <summary>
    ///     教师ID
    /// </summary>
    public long TeacherId
    {
        get => _teacherId;
        set => _teacherId = value;
    }

    /// <summary>
    ///     学校名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
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
    ///     所属学校
    /// </summary>
    public virtual School School
    {
        get => _school;
        set => _school = value;
    }

    /// <summary>
    ///     所拥有的的通行证
    /// </summary>
    public virtual List<PassPaper> PassPaperList
    {
        get => _passPaperList ?? [];
        set => _passPaperList = value;
    }

    /// <summary>
    ///     转换为字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"Teacher:{{TeacherId-{_teacherId},Name-\"{_name}\",SchoolId-{_schoolId}}}";
    }
}