namespace Obase.Test.Domain.Association.NoAssocationExtAttr;

/// <summary>
///     无关联冗余属性的教师
/// </summary>
public class NoAssocationExtAttrTeacher
{
    /// <summary>
    ///     教师姓名
    /// </summary>
    private string _name;

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
    ///     转换为字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"NoAssocationExtAttrTeacher:{{TeacherId-{_teacherId},Name-\"{_name}\"}}";
    }
}