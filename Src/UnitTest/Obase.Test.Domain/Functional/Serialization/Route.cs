namespace Obase.Test.Domain.Functional.Serialization;

/// <summary>
///     某种路由
/// </summary>
public class Route
{
    /// <summary>
    ///     路由操作
    /// </summary>
    private EAction _action;

    /// <summary>
    ///     路由规则
    /// </summary>
    private string _rule;

    /// <summary>
    ///     初始化某种路由
    /// </summary>
    /// <param name="rule">路由规则</param>
    /// <param name="action">路由操作</param>
    public Route(string rule, EAction action)
    {
        _rule = rule;
        _action = action;
    }

    /// <summary>
    ///     供反序列化使用
    /// </summary>
    protected Route()
    {
    }

    /// <summary>
    ///     路由规则
    /// </summary>
    public string Rule
    {
        get => _rule;
        protected internal set => _rule = value;
    }

    /// <summary>
    ///     路由操作
    /// </summary>
    public EAction Action
    {
        get => _action;
        set => _action = value;
    }

    /// <summary>
    ///     空对象
    /// </summary>
    public string PalaceHolder { get; set; }

    /// <summary>
    ///     排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    ///     权重
    /// </summary>
    public double Weight { get; set; }

    /// <summary>
    ///     是否启用
    /// </summary>
    public bool? Enabled { get; set; }

    /// <summary>
    ///     内部值
    /// </summary>
    public decimal Inner { get; set; }

    /// <summary>
    ///     返回字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"{nameof(_action)}: {_action}, {nameof(_rule)}: {_rule}, {nameof(Sort)}: {Sort}, {nameof(Weight)}: {Weight}, {nameof(Enabled)}: {Enabled}, {nameof(Inner)}: {Inner}";
    }
}