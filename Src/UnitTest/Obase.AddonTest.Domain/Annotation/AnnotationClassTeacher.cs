using Obase.Odm.Annotation;

namespace Obase.AddonTest.Domain.Annotation;

/// <summary>
///     标注建模测试用班级任课教师
/// </summary>
[Association]
public class AnnotationClassTeacher
{
    /// <summary>
    ///     班级
    /// </summary>
    private AnnotationClass _class;

    /// <summary>
    ///     班级ID
    /// </summary>
    private long _classId;

    /// <summary>
    ///     是否是班主任
    /// </summary>
    private bool _isManage;

    /// <summary>
    ///     是否是代课老师
    /// </summary>
    private bool _isSubstitute;

    /// <summary>
    ///     所授科目
    /// </summary>
    private string _subject;

    /// <summary>
    ///     教师
    /// </summary>
    private AnnotationTeacher _teacher;

    /// <summary>
    ///     教师ID
    /// </summary>
    private long _teacherId;

    /// <summary>
    ///     班级ID
    /// </summary>
    public long ClassId
    {
        get => _classId > 0 ? _classId : Class?.ClassId ?? 0;
        protected internal set => _classId = value;
    }

    /// <summary>
    ///     班级
    /// </summary>
    [AssociationEnd]
    [EndMapping("ClassId", "ClassId")]
    public virtual AnnotationClass Class
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
        protected internal set => _teacherId = value;
    }

    /// <summary>
    ///     教师
    /// </summary>
    [AssociationEnd]
    [EndMapping("TeacherId", "TeacherId")]
    public virtual AnnotationTeacher Teacher
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

    /// <summary>
    ///     是否是代课老师
    /// </summary>
    public bool IsSubstitute
    {
        get => _isSubstitute;
        set => _isSubstitute = value;
    }

    /// <summary>
    ///     所授科目
    /// </summary>
    public string Subject
    {
        get => _subject;
        set => _subject = value;
    }

    /// <summary>
    ///     转换为字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"ClassTeacher:{{ClassId-{_classId},TeacherId-{_teacherId},IsManage-\"{_isManage}\",IsSubstitute-\"{_isSubstitute}\",Subject-[{string.Join(",", _subject)}]}}";
    }
}