using System.Collections.Generic;

namespace Obase.Test.Domain.Association.NoAssociationExtAttr;

/// <summary>
///     无关联冗余属性的班级任课教师
/// </summary>
public class NoAssociationExtAttrClassTeacher
{
    /// <summary>
    ///     班级
    /// </summary>
    private NoAssociationExtAttrClass _class;

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
    private List<string> _subject = new();

    /// <summary>
    ///     教师
    /// </summary>
    private NoAssociationExtAttrTeacher _teacher;


    /// <summary>
    ///     班级
    /// </summary>
    public virtual NoAssociationExtAttrClass Class
    {
        get => _class;
        set => _class = value;
    }


    /// <summary>
    ///     教师
    /// </summary>
    public virtual NoAssociationExtAttrTeacher Teacher
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
            $"NoAssocationExtAttrClassTeacher:{{IsManage-\"{_isManage}\",IsSubstitute-\"{_isSubstitute}\",Subject-[{string.Join(",", _subject)}]}}";
    }
}