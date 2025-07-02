/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：多租户扩展.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:53:59
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;

namespace Obase.MultiTenant
{
    /// <summary>
    ///     多租户扩展
    /// </summary>
    public class MultiTenantExtension : TypeExtension
    {
        /// <summary>
        ///     全局多租户ID
        /// </summary>
        private object _globalTenantId;

        /// <summary>
        ///     是否包含全局Id进行查询
        /// </summary>
        private bool _loadingGlobal;

        /// <summary>
        ///     多租户标记的映射字段
        /// </summary>
        private string _tenantIdField;

        /// <summary>
        ///     多租户标记的属性的名称
        /// </summary>
        private string _tenantIdMark;

        /// <summary>
        ///     多租户标记的类型
        /// </summary>
        private Type _tenantIdType;

        /// <summary>
        ///     多租户标记的属性的名称
        /// </summary>
        public string TenantIdMark
        {
            get => _tenantIdMark;
            set => _tenantIdMark = value;
        }

        /// <summary>
        ///     多租户标记的映射字段
        /// </summary>
        public string TenantIdField
        {
            get => _tenantIdField;
            set => _tenantIdField = value;
        }

        /// <summary>
        ///     多租户标记的类型
        /// </summary>
        public Type TenantIdType
        {
            get => _tenantIdType;
            set => _tenantIdType = value;
        }

        /// <summary>
        ///     是否包含全局Id进行查询
        /// </summary>
        public bool LoadingGlobal
        {
            get => _loadingGlobal;
            set => _loadingGlobal = value;
        }

        /// <summary>
        ///     全局多租户ID
        /// </summary>
        public object GlobalTenantId
        {
            get => _globalTenantId;
            set => _globalTenantId = value;
        }
    }
}