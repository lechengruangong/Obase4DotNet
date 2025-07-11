﻿using System;
using Obase.LogicDeletion;
using Obase.Odm.Annotation;

namespace Obase.AddonTest.Domain.LogicDeletion;

/// <summary>
///     无定义字段逻辑删除标注测试域类
/// </summary>
[Entity("", false, "IntNumber")]
[LogicDeletion("Bool")]
public class LogicDeletionNoDefAnnotation
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
    ///     转换为字符串表示形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return
            $"LogicDeletionNoDefAnnotation:{{IntNumber-{IntNumber},DecimalNumber-{DecimalNumber},DateTime-\"{DateTime:yyyy-MM-dd HH:mm:ss}\",String-\"{String}\"}}";
    }
}