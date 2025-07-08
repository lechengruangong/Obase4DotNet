namespace Obase.Test.Domain.Association.NoAssocationExtAttr;

/// <summary>
///     无关联冗余属性的学生
/// </summary>
public class NoAssocationExtAttrStudent
{
    /// <summary>
    ///     就读班级
    /// </summary>
    private NoAssocationExtAttrClass _class;

    /// <summary>
    ///     学生名称
    /// </summary>
    private string _name;

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
        set => _studentId = value;
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
    ///     就读班级
    /// </summary>
    public virtual NoAssocationExtAttrClass Class
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
        return $"NoAssocationExtAttrStudent:{{StudentId-{StudentId},Name-\"{Name}\"}}";
    }
}