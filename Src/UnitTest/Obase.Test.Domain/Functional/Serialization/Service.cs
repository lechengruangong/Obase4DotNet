namespace Obase.Test.Domain.Functional.Serialization;

/// <summary>
///     模拟某种服务
/// </summary>
public class Service
{
    /// <summary>
    ///     代码
    /// </summary>
    private string _code;

    /// <summary>
    ///     身份
    /// </summary>
    private Identity _identity;

    /// <summary>
    ///     路由
    /// </summary>
    private Route _route;

    /// <summary>
    ///     子路由
    /// </summary>
    private Route[] _subRoute;

    /// <summary>
    ///     代码
    /// </summary>
    public string Code
    {
        get => _code;
        set => _code = value;
    }

    /// <summary>
    ///     路由
    /// </summary>
    public Route Route
    {
        get => _route;
        set => _route = value;
    }

    /// <summary>
    ///     子路由
    /// </summary>
    public Route[] SubRoute
    {
        get => _subRoute;
        set => _subRoute = value;
    }

    /// <summary>
    ///     身份
    /// </summary>
    public Identity Identity
    {
        get => _identity;
        set => _identity = value;
    }
}