using System;
using Obase.Odm.Annotation;

namespace Obase.AddonTest.Domain.Annotation;

/// <summary>
///     标注建模测试用JAVABEAN 带有自定义取值器设值器 和 表名
/// </summary>
[Entity("AnnotationJavaBean", false, "IntNumber")]
public class AnnotationJavaBeanWithCustomAttribute
{
    /// <summary>
    ///     自定义构造函数
    /// </summary>
    /// <param name="intNumber"></param>
    /// <param name="decimalNumber"></param>
    /// <param name="dateTime"></param>
    /// <param name="s"></param>
    /// <param name="b"></param>
    /// <param name="strings"></param>
    public AnnotationJavaBeanWithCustomAttribute(int intNumber, double decimalNumber, DateTime dateTime, string s,
        bool b, string[] strings)
    {
        IntNumber = intNumber;
        DecimalNumber = decimalNumber;
        DateTime = dateTime;
        String = s;
        Bool = b;
        Strings = strings;
    }

    /// <summary>
    ///     反序列化构造函数
    /// </summary>
    [Constructor("IntNumber", "DecimalNumber", "DateTime", "String", "Bool")]
    protected AnnotationJavaBeanWithCustomAttribute(int intNumber, double decimalNumber, DateTime dateTime, string s,
        bool b)
    {
        IntNumber = intNumber;
        DecimalNumber = decimalNumber;
        DateTime = dateTime;
        String = s;
        Bool = b;
    }

    /// <summary>
    ///     int类型数字
    /// </summary>
    public int IntNumber { get; }

    /// <summary>
    ///     decimal类型数字
    /// </summary>
    public double DecimalNumber { get; }

    /// <summary>
    ///     时间类型
    /// </summary>
    public DateTime DateTime { get; }

    /// <summary>
    ///     字符串类型
    /// </summary>
    public string String { get; }

    /// <summary>
    ///     布尔值类型
    /// </summary>
    public bool Bool { get; }

    /// <summary>
    ///     以某种分隔符分割的数组
    /// </summary>
    public string[] Strings { get; set; }

    /// <summary>
    ///     转换为字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"JavaBeanLikeModel:{{IntNumber-{IntNumber},DecimalNumber-{DecimalNumber},DateTime-\"{DateTime:yyyy-MM-dd HH:mm:ss}\",String-\"{String}\",Bool-\"{Bool}\",Strings-[{string.Join("|", Strings)}]}}";
    }
}