namespace Obase.Test.Domain.Functional.Serialization;

/// <summary>
///     分析器B
/// </summary>
public class AnalyserB : Analyser
{
    /// <summary>
    ///     初始化分析器
    /// </summary>
    /// <param name="next">下一个分析器</param>
    public AnalyserB(Analyser next) : base("AnalyserB", next)
    {
    }

    /// <summary>
    ///     反序列化方法
    /// </summary>
    protected AnalyserB() : base(null, null)
    {
    }
}