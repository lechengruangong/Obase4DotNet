/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：多租户注属性.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:51:22
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.MultiTenant
{
    /// <summary>
    ///     多租户注属性 用于指定哪个字段用于多租户
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MultiTenantAttribute : Attribute
    {
        /// <summary>
        ///     构造多租户标记
        /// </summary>
        /// <param name="multiTenantField">指定哪个字段用于多租户</param>
        /// <param name="tenantIdType">多租户ID类型</param>
        public MultiTenantAttribute(string multiTenantField, Type tenantIdType)
        {
            if (tenantIdType != typeof(int) && tenantIdType != typeof(long) && tenantIdType != typeof(string) &&
                tenantIdType != typeof(Guid))
                throw new ArgumentException("多租户主键属性必须为string,int,long,Guid类型中的一种");

            MultiTenantField = multiTenantField;
            TenantIdType = tenantIdType;
        }

        /// <summary>
        ///     哪个字段用于多租户
        /// </summary>
        public string MultiTenantField { get; }

        /// <summary>
        ///     多租户ID类型
        /// </summary>
        public Type TenantIdType { get; set; }
    }
}