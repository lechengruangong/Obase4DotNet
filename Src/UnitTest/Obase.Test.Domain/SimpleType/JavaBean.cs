using System;

namespace Obase.Test.Domain.SimpleType;

/// <summary>
///     模型接口
/// </summary>
public interface IModel
{
}

/// <summary>
///     一个类似于JavaBean的失血模型 包含若干常用的数据类型属性访问器
/// </summary>
public class JavaBean : IModel
{
    /// <summary>
    ///     int类型数字
    /// </summary>
    public int IntNumber { get; set; }

    /// <summary>
    ///     long类型数字
    /// </summary>
    public long LongNumber { get; set; }

    /// <summary>
    ///     byte类型数字
    /// </summary>
    public byte ByteNumber { get; set; }

    /// <summary>
    ///     char类型数字
    /// </summary>
    public char CharNumber { get; set; }

    /// <summary>
    ///     float类型数字
    /// </summary>
    public float FloatNumber { get; set; }

    /// <summary>
    ///     double类型数字
    /// </summary>
    public double DoubleNumber { get; set; }

    /// <summary>
    ///     Decimal数字
    /// </summary>
    public decimal DecimalNumber { get; set; }

    /// <summary>
    ///     日期时间类型
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    ///     时间类型
    /// </summary>
    public TimeSpan Time { get; set; }

    /// <summary>
    ///     日期类型
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    ///     字符串类型
    /// </summary>
    public string String { get; set; }

    /// <summary>
    ///     布尔值类型
    /// </summary>
    public bool Bool { get; set; }

    /// <summary>
    ///     GUID类型
    /// </summary>
    public Guid Guid { get; set; }

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
            $"JavaBean:{{IntNumber-{IntNumber},LongNumber-{LongNumber},ByteNumber-{ByteNumber},CharNumber-{CharNumber},FloatNumber-{FloatNumber},DoubleNumber-{DoubleNumber},DecimalNumber-{DecimalNumber},DateTime-\"{DateTime:yyyy-MM-dd HH:mm:ss.fff}\",Date-\"{Date:yyyy-MM-dd HH:mm:ss.fff}\",Time-\"{Time:c}\",String-\"{String}\",Bool-\"{Bool}\",Guid-\"{Guid:N}\",Strings-[{string.Join("|", Strings)}]}}";
    }
}

/// <summary>
///     小型的JavaBean的失血模型
/// </summary>
public class SmallJavaBeanLikeModel
{
    /// <summary>
    ///     布尔值类型
    /// </summary>
    public bool Bool { get; set; }

    /// <summary>
    ///     decimal类型数字
    /// </summary>
    public decimal DecimalNumber { get; set; }


    /// <summary>
    ///     转换为字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"SmallJavaBeanLikeModel:{{DecimalNumber-{DecimalNumber},Bool-\"{Bool}\"]}}";
    }
}