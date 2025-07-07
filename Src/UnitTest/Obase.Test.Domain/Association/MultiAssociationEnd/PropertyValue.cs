namespace Obase.Test.Domain.Association.MultiAssociationEnd;

/// <summary>
///     属性值
/// </summary>
public class PropertyValue
{
    /// <summary>
    ///     属性值编码
    /// </summary>
    private string _code;

    /// <summary>
    ///     具体值
    /// </summary>
    private string _value;

    /// <summary>
    ///     属性值编码
    /// </summary>
    public string Code
    {
        get => _code;
        set => _code = value;
    }

    /// <summary>
    ///     具体值
    /// </summary>
    public string Value
    {
        get => _value;
        set => _value = value;
    }
}