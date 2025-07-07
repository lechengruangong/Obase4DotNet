namespace Obase.AddonTest.Domain.MultiTenant;

/// <summary>
///     未定义多租户字段的教师
/// </summary>
public class MultiTenantTeacherNoDef
{
    /// <summary>
    ///     教师姓名
    /// </summary>
    private string _name;

    /// <summary>
    ///     所属学校
    /// </summary>
    private MultiTenantSchoolNoDef _school;

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
    public virtual MultiTenantSchoolNoDef School
    {
        get => _school;
        set => _school = value;
    }

    /// <summary>
    ///     转换为字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"MultiTenantTeacherNoDef:{{TeacherId-{_teacherId},Name-\"{_name}\",SchoolId-{_schoolId}}}";
    }
}