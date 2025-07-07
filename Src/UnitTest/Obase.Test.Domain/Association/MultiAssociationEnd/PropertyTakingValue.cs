namespace Obase.Test.Domain.Association.MultiAssociationEnd;

/// <summary>
///     属性取值
/// </summary>
public class PropertyTakingValue
{
    /// <summary>
    ///     产品
    /// </summary>
    private Product _product;

    /// <summary>
    ///     产品编码
    /// </summary>
    private string _productCode;

    /// <summary>
    ///     属性
    /// </summary>
    private Property _property;

    /// <summary>
    ///     属性编码
    /// </summary>
    private string _propertyCode;

    /// <summary>
    ///     属性取值图片
    /// </summary>
    private string _propertyPhotoUrl;

    /// <summary>
    ///     属性取值
    /// </summary>
    private PropertyValue _propertyValue;

    /// <summary>
    ///     属性取值编码
    /// </summary>
    private string _propertyValueCode;

    /// <summary>
    ///     属性取值图片
    /// </summary>
    public string PropertyPhotoUrl
    {
        get => _propertyPhotoUrl;
        set => _propertyPhotoUrl = value;
    }

    /// <summary>
    ///     产品编码
    /// </summary>
    public string ProductCode
    {
        get => _productCode;
        set => _productCode = value;
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
    ///     属性编码
    /// </summary>
    public string PropertyCode
    {
        get => _propertyCode;
        set => _propertyCode = value;
    }

    /// <summary>
    ///     属性
    /// </summary>
    public Property Property
    {
        get => _property;
        set => _property = value;
    }

    /// <summary>
    ///     属性取值编码
    /// </summary>
    public string PropertyValueCode
    {
        get => _propertyValueCode;
        set => _propertyValueCode = value;
    }

    /// <summary>
    ///     属性取值
    /// </summary>
    public PropertyValue PropertyValue
    {
        get => _propertyValue;
        set => _propertyValue = value;
    }
}