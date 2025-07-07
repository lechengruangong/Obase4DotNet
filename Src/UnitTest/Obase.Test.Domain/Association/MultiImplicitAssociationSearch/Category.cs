using System.Collections.Generic;

namespace Obase.Test.Domain.Association.MultiImplicitAssociationSearch;

/// <summary>
///     产品分类
/// </summary>
public class Category
{
    /// <summary>
    ///     产品分类ID
    /// </summary>
    private int _categoryId;

    /// <summary>
    ///     产品分类名称
    /// </summary>
    private string _name;

    /// <summary>
    ///     分类下的产品
    /// </summary>
    private List<Product> _products;

    /// <summary>
    ///     产品分类ID
    /// </summary>
    public int CategoryId
    {
        get => _categoryId;
        set => _categoryId = value;
    }

    /// <summary>
    ///     产品分类名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     分类下的产品
    /// </summary>
    public List<Product> Products
    {
        get => _products;
        set => _products = value;
    }
}