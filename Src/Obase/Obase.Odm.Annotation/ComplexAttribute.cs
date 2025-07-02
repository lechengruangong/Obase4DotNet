/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：复杂类型标注属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 11:19:18
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Odm.Annotation
{
    /// <summary>
    ///     复杂类型标注属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ComplexAttribute : Attribute
    {
    }
}