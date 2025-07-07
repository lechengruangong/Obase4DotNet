using System;
using Obase.MultiTenant;
using Obase.Odm.Annotation;

namespace Obase.AddonTest.Domain.MultiTenant;

/// <summary>
///     定义了多租户字段的教师 标注配置
/// </summary>
[Entity("Teacher", keyAttributes: "TeacherId")]
//映射字段相同 此行不用标注
//[MultiTenant("MultiTenantId",typeof(Guid))] 
public class MultiTenantTeacherAnnotation
{
    /// <summary>
    ///     多租户ID
    /// </summary>
    private Guid _multiTenantId;

    /// <summary>
    ///     教师姓名
    /// </summary>
    private string _name;

    /// <summary>
    ///     所属学校
    /// </summary>
    private MultiTenantSchoolAnnotation _school;

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
    [ImplicitAssociation("Teacher", true)]
    [LeftEndMapping("TeacherId", "TeacherId")]
    [RightEndMapping("SchoolId", "SchollId")]
    public virtual MultiTenantSchoolAnnotation School
    {
        get => _school;
        set => _school = value;
    }

    /// <summary>
    ///     多租户ID
    /// </summary>
    [MultiTenantMark]
    public Guid MultiTenantId
    {
        get => _multiTenantId;
        set => _multiTenantId = value;
    }

    /// <summary>
    ///     转换为字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"MultiTenantTeacherAnnotation:{{TeacherId-{_teacherId},Name-\"{_name}\",SchoolId-{_schoolId}}}";
    }
}