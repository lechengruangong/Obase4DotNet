/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：多租户标记标注属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:59:42
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.MultiTenant
{
    /// <summary>
    ///     多租户标记标注属性 用于指定多租户标记的属性的名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class MultiTenantMarkAttribute : Attribute
    {
    }
}