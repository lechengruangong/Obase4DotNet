using Obase.Odm.Annotation;

namespace Obase.AddonTest.Domain.Annotation;

/// <summary>
///     标注建模测试用学生
/// </summary>
[Entity(keyAttributes: "StudentId")]
public class AnnotationStudent
{
    /// <summary>
    ///     就读班级
    /// </summary>
    private AnnotationClass _class;

    /// <summary>
    ///     班级ID
    /// </summary>
    private long _classId;

    /// <summary>
    ///     学生名称
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
    ///     学生id
    /// </summary>
    private long _studentId;

    /// <summary>
    ///     学生id
    /// </summary>
    public long StudentId
    {
        get => _studentId;
        protected internal set => _studentId = value;
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
    [ImplicitAssociation("AnnotationStudent", true)]
    [LeftEndMapping("StudentId", "StudentId")]
    [RightEndMapping("SchoolId", "SchoolId")]
    public virtual AnnotationSchool School
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
    [ImplicitAssociation("AnnotationStudent", true)]
    [LeftEndMapping("StudentId", "StudentId")]
    [RightEndMapping("ClassId", "ClassId")]
    public virtual AnnotationClass Class
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
        return $"AnnotationStudent:{{StudentId-{_studentId},Name-\"{_name}\",SchoolId-{_schoolId},ClassId-{_classId}}}";
    }
}