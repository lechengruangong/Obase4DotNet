using System.Collections.Generic;

namespace Obase.Test.Domain.Association.MultiImplicitAssociationSearch;

/// <summary>
///     产品
/// </summary>
public class Product
{
    /// <summary>
    ///     所属的分类
    /// </summary>
    private List<Category> _categories;

    /// <summary>
    ///     产品Code
    /// </summary>
    private string _code;

    /// <summary>
    ///     产品名称
    /// </summary>
    private string _name;

    /// <summary>
    ///     产品Code
    /// </summary>
    public string Code
    {
        get => _code;
        set => _code = value;
    }

    /// <summary>
    ///     产品名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     所属的分类
    /// </summary>
    public virtual List<Category> Categories
    {
        get => _categories;
        set => _categories = value;
    }
}