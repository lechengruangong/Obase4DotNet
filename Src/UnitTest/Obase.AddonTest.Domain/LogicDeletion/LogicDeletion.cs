using System;

namespace Obase.AddonTest.Domain.LogicDeletion;

/// <summary>
///     逻辑删除测试域类
/// </summary>
public class LogicDeletion
{
    /// <summary>
    ///     int类型数字
    /// </summary>
    public int IntNumber { get; set; }

    /// <summary>
    ///     decimal类型数字
    /// </summary>
    public double DecimalNumber { get; set; }

    /// <summary>
    ///     时间类型
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    ///     字符串类型
    /// </summary>
    public string String { get; set; }

    /// <summary>
    ///     布尔值类型
    /// </summary>
    public bool Bool { get; set; }

    /// <summary>
    ///     转换为字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"LogicDeletion:{{IntNumber-{IntNumber},DecimalNumber-{DecimalNumber},DateTime-\"{DateTime:yyyy-MM-dd HH:mm:ss.fff}\",String-\"{String}\",Bool-\"{Bool}\"}}";
    }
}