using System.Collections.Generic;

namespace Obase.Test.Domain.Association.NoAssocationExtAttr;

/// <summary>
///     无关联冗余属性的班级
/// </summary>
public class NoAssocationExtAttrClass
{
    /// <summary>
    ///     班级id
    /// </summary>
    private long _classId;

    /// <summary>
    ///     班级任课老师
    /// </summary>
    private List<NoAssocationExtAttrClassTeacher> _classTeachers;

    /// <summary>
    ///     班级名称
    /// </summary>
    private string _name;

    /// <summary>
    ///     学校
    /// </summary>
    private NoAssocationExtAttrSchool _school;

    /// <summary>
    ///     学生
    /// </summary>
    private List<NoAssocationExtAttrStudent> _students;

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
    ///     学校
    /// </summary>
    public virtual NoAssocationExtAttrSchool School
    {
        get => _school;
        set => _school = value;
    }

    /// <summary>
    ///     学生
    /// </summary>
    public virtual List<NoAssocationExtAttrStudent> Students
    {
        get => _students;
        set => _students = value;
    }

    /// <summary>
    ///     任课教师
    /// </summary>
    public virtual List<NoAssocationExtAttrClassTeacher> ClassTeachers
    {
        get => _classTeachers;
        set => _classTeachers = value;
    }

    /// <summary>
    ///     设置任课老师
    /// </summary>
    /// <param name="classTeacher">任课老师</param>
    public void SetTeacher(NoAssocationExtAttrClassTeacher classTeacher)
    {
        _classTeachers ??= [];
        _classTeachers.Add(classTeacher);
    }

    /// <summary>
    ///     设置学生
    /// </summary>
    /// <param name="student">学生</param>
    public void SetStudent(NoAssocationExtAttrStudent student)
    {
        _students ??= [];
        _students.Add(student);
    }

    /// <summary>
    ///     转换为字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"NoAssocationExtAttrClass:{{ClassId-{_classId},Name-\"{_name}\"}}";
    }
}