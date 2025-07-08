namespace Obase.Test.Domain.Association.MultiAssociationEnd;

/// <summary>
///     属性
/// </summary>
public class Property
{
    /// <summary>
    ///     属性编码
    /// </summary>
    private string _code;

    /// <summary>
    ///     属性名称
    /// </summary>
    private string _name;

    /// <summary>
    ///     属性编码
    /// </summary>
    public string Code
    {
        get => _code;
        set => _code = value;
    }

    /// <summary>
    ///     属性名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }
}