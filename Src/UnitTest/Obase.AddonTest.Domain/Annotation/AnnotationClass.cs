using System.Collections.Generic;
using Obase.Odm.Annotation;

namespace Obase.AddonTest.Domain.Annotation;

/// <summary>
///     标注建模测试用班级
/// </summary>
[Entity(keyAttributes: "ClassId")]
public class AnnotationClass
{
    /// <summary>
    ///     班级id
    /// </summary>
    private long _classId;

    /// <summary>
    ///     班级任课老师
    /// </summary>
    private List<AnnotationClassTeacher> _classTeachers;

    /// <summary>
    ///     班级名称
    /// </summary>
    private string _name;

    /// <summary>
    ///     学校
    /// </summary>
    private AnnotationSchool _school;

    /// <summary>
    ///     学校ID
    /// </summary>
    private long _schoolId;

    /// <summary>
    ///     学生
    /// </summary>
    private List<AnnotationStudent> _students;

    /// <summary>
    ///     班级id
    /// </summary>
    public long ClassId
    {
        get => _classId;
        protected internal set => _classId = value;
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
    [ImplicitAssociation("AnnotationClass")]
    [LeftEndMapping("ClassId", "ClassId")]
    [RightEndMapping("SchoolId", "SchoolId")]
    public virtual AnnotationSchool School
    {
        get => _school;
        set => _school = value;
    }

    /// <summary>
    ///     学生
    /// </summary>
    [ImplicitAssociation("AnnotationStudent", true)]
    [LeftEndMapping("ClassId", "ClassId")]
    [RightEndMapping("StudentId", "StudentId")]
    public virtual List<AnnotationStudent> Students
    {
        get => _students;
        set => _students = value;
    }

    /// <summary>
    ///     任课教师
    /// </summary>
    [AssociationReference]
    public virtual List<AnnotationClassTeacher> ClassTeachers
    {
        get => _classTeachers;
        set => _classTeachers = value;
    }

    /// <summary>
    ///     转换为字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"AnnotationClass:{{ClassId-{_classId},Name-\"{_name}\",SchoolId-{_schoolId}}}";
    }
}