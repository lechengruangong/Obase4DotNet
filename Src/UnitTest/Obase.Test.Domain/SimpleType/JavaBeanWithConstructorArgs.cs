using System;

namespace Obase.Test.Domain.SimpleType;

/// <summary>
///     仅有构造函数的JavaBean
/// </summary>
public class JavaBeanWithConstructorArgs
{
    /// <summary>
    ///     构造函数
    /// </summary>
    public JavaBeanWithConstructorArgs(int intNumber, long longNumber, byte byteNumber, char charNumber,
        float floatNumber, double doubleNumber, decimal decimalNumber, DateTime dateTime, TimeSpan time, DateTime date,
        string @string, bool @bool, string[] strings)
    {
        IntNumber = intNumber;
        LongNumber = longNumber;
        ByteNumber = byteNumber;
        CharNumber = charNumber;
        FloatNumber = floatNumber;
        DoubleNumber = doubleNumber;
        DecimalNumber = decimalNumber;
        DateTime = dateTime;
        Time = time;
        Date = date;
        String = @string;
        Bool = @bool;
        Strings = strings;
    }

    /// <summary>
    ///     int类型数字
    /// </summary>
    public int IntNumber { get; }

    /// <summary>
    ///     long类型数字
    /// </summary>
    public long LongNumber { get; }

    /// <summary>
    ///     byte类型数字
    /// </summary>
    public byte ByteNumber { get; }

    /// <summary>
    ///     char类型数字
    /// </summary>
    public char CharNumber { get; }

    /// <summary>
    ///     float类型数字
    /// </summary>
    public float FloatNumber { get; }

    /// <summary>
    ///     double类型数字
    /// </summary>
    public double DoubleNumber { get; }

    /// <summary>
    ///     Decimal数字
    /// </summary>
    public decimal DecimalNumber { get; }

    /// <summary>
    ///     日期时间类型
    /// </summary>
    public DateTime DateTime { get; }

    /// <summary>
    ///     时间类型
    /// </summary>
    public TimeSpan Time { get; }

    /// <summary>
    ///     日期类型
    /// </summary>
    public DateTime Date { get; }

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
    public string[] Strings { get; }

    /// <summary>
    ///     转换为字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"JavaBeanWithConstructorArgs:{{IntNumber-{IntNumber},LongNumber-{LongNumber},ByteNumber-{ByteNumber},CharNumber-{CharNumber},FloatNumber-{FloatNumber},DoubleNumber-{DoubleNumber},DecimalNumber-{DecimalNumber},DateTime-\"{DateTime:yyyy-MM-dd HH:mm:ss}\",Date-\"{Date:yyyy-MM-dd HH:mm:ss}\",Time-\"{Time:c}\",String-\"{String}\",Bool-\"{Bool}\",Strings-[{string.Join("|", Strings)}]}}";
    }
}