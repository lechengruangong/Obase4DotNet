namespace Obase.Test.Domain.Association.DefaultAsNew;

/// <summary>
///     测试关联端默认是否新建对象学校
/// </summary>
public class DefaultSchool
{
    /// <summary>
    ///     学校名称
    /// </summary>
    private string _name;

    /// <summary>
    ///     学校ID
    /// </summary>
    private long _schoolId;

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
}