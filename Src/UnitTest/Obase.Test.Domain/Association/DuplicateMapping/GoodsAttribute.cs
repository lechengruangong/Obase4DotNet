namespace Obase.Test.Domain.Association.DuplicateMapping;

/// <summary>
///     表示在一个商品上为某一个属性设置了值，简称商品属性。
/// </summary>
public class GoodsAttribute
{
    /// <summary>
    ///     属性的标识。
    /// </summary>
    private long _attributeId;

    /// <summary>
    ///     商品的标识。
    /// </summary>
    private long _goodsId;

    /// <summary>
    ///     该商品属性的输入值（与标准值对举）。
    /// </summary>
    private string _inputValue;

    /// <summary>
    ///     初始化GoodsAttribute类的新实例。
    /// </summary>
    /// <param name="goodsId">商品的标识。</param>
    /// <param name="attributeId">属性的标识。</param>
    public GoodsAttribute(long goodsId, long attributeId)
    {
        GoodsId = goodsId;
        AttributeId = attributeId;
    }

    /// <summary>
    ///     供对象重建（如反持久化）使用的构造函数。
    /// </summary>
    protected GoodsAttribute()
    {
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
    ///     获取商品的标识。
    /// </summary>
    public long GoodsId
    {
        get => _goodsId;
        protected internal set => _goodsId = value;
    }

    /// <summary>
    ///     获取或设置该商品属性的输入值（与标准值对举）。
    /// </summary>
    public string InputValue
    {
        get => _inputValue;
        set => _inputValue = value;
    }
}