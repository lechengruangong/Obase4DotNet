namespace Obase.Test.Domain.Functional.Serialization;

/// <summary>
///     分析器C
/// </summary>
public class AnalyserC : Analyser
{
    /// <summary>
    ///     初始化分析器
    /// </summary>
    /// <param name="next">下一个分析器</param>
    public AnalyserC(Analyser next) : base("AnalyserC", next)
    {
    }

    /// <summary>
    ///     反序列化方法
    /// </summary>
    protected AnalyserC() : base(null, null)
    {
    }
}