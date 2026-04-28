namespace Obase.Test.Domain.Functional.Serialization;

/// <summary>
///     分析器A
/// </summary>
public class AnalyserA : Analyser
{
    /// <summary>
    ///     初始化分析器
    /// </summary>
    /// <param name="next">下一个分析器</param>
    public AnalyserA(Analyser next) : base("AnalyserA", next)
    {
    }


    /// <summary>
    ///     反序列化方法
    /// </summary>
    protected AnalyserA() : base(null, null)
    {
    }
}