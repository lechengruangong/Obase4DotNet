namespace Obase.Test.Domain.Association.DuplicateMapping;

/// <summary>
///     表示为一个商品属性从可选值清单中选定了一个值
/// </summary>
public class StandardValue
{
    /// <summary>
    ///     取值别名，即属性值作为当前商品属性的取值时的别名。
    /// </summary>
    private string _alias;

    /// <summary>
    ///     属性的标识。
    /// </summary>
    private long _attributeId;

    /// <summary>
    ///     商品所属类目的标识。
    /// </summary>
    private long _categoryId;

    /// <summary>
    ///     商品属性。
    /// </summary>
    private GoodsAttribute _goodsAttribute;

    /// <summary>
    ///     商品的标识。
    /// </summary>
    private long _goodsId;

    /// <summary>
    ///     属性图片，即针对商品的特定属性（如颜色为红色）拍摄的展示图片。
    /// </summary>
    private string _photo;

    /// <summary>
    ///     被选中的属性值。
    /// </summary>
    private SelectableValue _selectedValue;

    /// <summary>
    ///     属性值的标识。
    /// </summary>
    private long _valueId;


    /// <summary>
    ///     初始化StandardValue类的新实例。
    /// </summary>
    /// <param name="goodsAttribute">商品属性。</param>
    /// <param name="selectedValue">被选中的属性值。</param>
    public StandardValue(GoodsAttribute goodsAttribute, SelectableValue selectedValue)
    {
        GoodsAttribute = goodsAttribute;
        SelectedValue = selectedValue;
    }

    /// <summary>
    ///     供对象重建（如反持久化）使用的构造函数。
    /// </summary>
    protected StandardValue()
    {
    }

    /// <summary>
    ///     获取或设置取值别名，取值别名是属性值作为当前商品属性的取值时的别名。
    /// </summary>
    public string Alias
    {
        get => _alias;
        set => _alias = value;
    }

    /// <summary>
    ///     获取属性的标识。
    /// </summary>
    public long AttributeId
    {
        get => _attributeId;
        protected internal set => _attributeId = value;
    }

    /// <summary>
    ///     商品所属类目的标识。
    /// </summary>
    public long CategoryId
    {
        get => _categoryId;
        protected internal set => _categoryId = value;
    }

    /// <summary>
    ///     获取商品属性。
    /// </summary>
    public GoodsAttribute GoodsAttribute
    {
        get => _goodsAttribute;
        protected internal set => _goodsAttribute = value;
    }

    /// <summary>
    ///     获取商品的标识。
    /// </summary>
    public long GoodsId
    {
        get => _goodsId;
        protected internal set => _goodsId = value;
    }

    /// <summary>
    ///     获取或设置属性图片，属性图片是指针对商品的特定属性（如颜色为红色）拍摄的展示图片。
    /// </summary>
    public string Photo
    {
        get => _photo;
        set => _photo = value;
    }

    /// <summary>
    ///     获取属性的取值。
    ///     实施说明
    ///     如果未加载属性的取值，使用其仓储加载；应寄存加载结果，避免重复加载。
    /// </summary>
    public SelectableValue SelectedValue
    {
        get => _selectedValue;
        protected internal set => _selectedValue = value;
    }

    /// <summary>
    ///     获取属性值的标识。
    /// </summary>
    public long ValueId
    {
        get => _valueId;
        protected internal set => _valueId = value;
    }
}