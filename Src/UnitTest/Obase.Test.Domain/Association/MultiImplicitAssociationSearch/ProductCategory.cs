namespace Obase.Test.Domain.Association.MultiImplicitAssociationSearch;

/// <summary>
///     显式化的产品分类隐式关联型
/// </summary>
public class ProductCategory
{
    /// <summary>
    ///     分类
    /// </summary>
    private Category _category;

    /// <summary>
    ///     分类ID
    /// </summary>
    private int _categoryId;

    /// <summary>
    ///     分类名称
    /// </summary>
    private string _categoryName;

    /// <summary>
    ///     产品
    /// </summary>
    private Product _product;

    /// <summary>
    ///     产品Code
    /// </summary>
    private string _productCode;

    /// <summary>
    ///     产品Code
    /// </summary>
    public string ProductCode
    {
        get => _productCode;
        set => _productCode = value;
    }

    /// <summary>
    ///     分类ID
    /// </summary>
    public int CategoryId
    {
        get => _categoryId;
        set => _categoryId = value;
    }

    /// <summary>
    ///     分类名称
    /// </summary>
    public string CategoryName
    {
        get => _categoryName;
        set => _categoryName = value;
    }

    /// <summary>
    ///     产品
    /// </summary>
    public Product Product
    {
        get => _product;
        set => _product = value;
    }

    /// <summary>
    ///     分类
    /// </summary>
    public Category Category
    {
        get => _category;
        set => _category = value;
    }
}