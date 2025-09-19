namespace Obase.Test.Domain.Association.DuplicateMapping;

/// <summary>
///     表示一个属性值可以作为某一类目属性的可选值。
/// </summary>
public class SelectableValue
{
    /// <summary>
    ///     作为当前属性的可选值时的别名。
    /// </summary>
    private string _alias;

    /// <summary>
    ///     属性的标识。
    /// </summary>
    private long _attributeId;

    /// <summary>
    ///     类目的标识。
    /// </summary>
    private long _categoryId;

    /// <summary>
    ///     在当前属性可选值集合中的排序。
    /// </summary>
    private int _sequence;

    /// <summary>
    ///     SelectableValue类的新实例。
    /// </summary>
    /// <param name="categoryId">类目的标识。</param>
    /// <param name="attributeId">属性的标识。</param>
    public SelectableValue(long categoryId, long attributeId)
    {
        CategoryId = categoryId;
        AttributeId = attributeId;
    }

    /// <summary>
    ///     供对象重建（如反持久化）使用的构造函数。
    /// </summary>
    protected SelectableValue()
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
    ///     作为当前属性的可选值时的别名。
    /// </summary>
    public string Alias
    {
        get => _alias;
        set => _alias = value;
    }

    /// <summary>
    ///     类目的标识。
    /// </summary>
    public long CategoryId
    {
        get => _categoryId;
        protected internal set => _categoryId = value;
    }

    /// <summary>
    ///     在当前属性可选值集合中的排序。
    /// </summary>
    public int Sequence
    {
        get => _sequence;
        set => _sequence = value;
    }
}