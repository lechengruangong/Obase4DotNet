using System;
using Obase.Odm.Annotation;

namespace Obase.AddonTest.Domain.Annotation;

/// <summary>
///     标注建模测试用JAVABEAN
/// </summary>
[Entity("", false, "IntNumber")]
public class AnnotationJavaBean
{
    /// <summary>
    ///     int类型数字
    /// </summary>
    public int IntNumber { get; set; }

    /// <summary>
    ///     decimal类型数字
    /// </summary>
    [TypeAttribute("DecimalNumber", precision: 5)]
    public decimal DecimalNumber { get; set; }

    /// <summary>
    ///     时间类型
    /// </summary>
    [TypeAttribute("DateTime", nullable: false)]
    public DateTime DateTime { get; set; }

    /// <summary>
    ///     字符串类型
    /// </summary>
    [TypeAttribute("Strings", 50)]
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
            $"AnnotationJavaBean:{{IntNumber-{IntNumber},DecimalNumber-{DecimalNumber},DateTime-\"{DateTime:yyyy-MM-dd HH:mm:ss.fff}\",String-\"{String}\",Bool-\"{Bool}\"}}";
    }
}