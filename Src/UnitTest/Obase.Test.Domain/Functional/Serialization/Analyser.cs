namespace Obase.Test.Domain.Functional.Serialization;

/// <summary>
///     某种分析器
/// </summary>
public abstract class Analyser : IComponent
{
    /// <summary>
    ///     初始化分析器
    /// </summary>
    /// <param name="name">组件名称</param>
    /// <param name="next">下一个分析器</param>
    protected Analyser(string name, Analyser next)
    {
        Name = name;
        Next = next;
    }

    /// <summary>
    ///     下一个分析器
    /// </summary>
    public Analyser Next { get; protected internal set; }

    /// <summary>
    ///     子分析器集合
    /// </summary>
    public Analyser[] SubAnalysers { get; set; }

    /// <summary>
    ///     组件名称
    /// </summary>
    public string Name { get; protected internal set; }

    /// <summary>
    ///     返回字符串
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{nameof(Next)}: {Next}, {nameof(Name)}: {Name}";
    }
}