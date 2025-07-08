using System.Collections.Generic;

namespace Obase.Test.Domain.Association;

/// <summary>
///     班级任课教师
/// </summary>
public class ClassTeacher
{
    /// <summary>
    ///     班级
    /// </summary>
    private Class _class;

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
    private List<string> _subject = [];

    /// <summary>
    ///     教师
    /// </summary>
    private Teacher _teacher;

    /// <summary>
    ///     教师ID
    /// </summary>
    private long _teacherId;

    /// <summary>
    ///     普通构造函数
    /// </summary>
    /// <param name="class">班级</param>
    /// <param name="teacher">教师</param>
    public ClassTeacher(Class @class, Teacher teacher)
    {
        _class = @class;
        _teacher = teacher;
    }

    /// <summary>
    ///     新实例构造函数
    /// </summary>
    /// <param name="classId">班级ID</param>
    /// <param name="teacherId">教师ID</param>
    /// <param name="isManage">是否班主任</param>
    /// <param name="isSubstitute">是否代课</param>
    /// <param name="subject">教授科目</param>
    public ClassTeacher(long classId, long teacherId, bool isManage, bool isSubstitute, List<string> subject)
    {
        _classId = classId;
        _teacherId = teacherId;
        _isManage = isManage;
        _isSubstitute = isSubstitute;
        _subject = subject;
    }

    /// <summary>
    ///     反序列化构造函数
    /// </summary>
    /// <param name="classId">班级ID</param>
    /// <param name="teacherId">教师ID</param>
    protected ClassTeacher(long classId, long teacherId)
    {
        _classId = classId;
        _teacherId = teacherId;
    }

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
    public virtual Class Class
    {
        get => _class;
        protected internal set => _class = value;
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
    public virtual Teacher Teacher
    {
        get => _teacher;
        protected internal set => _teacher = value;
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
    public List<string> Subject
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