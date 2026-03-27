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
    public string PalceHolder { get; set; }
}