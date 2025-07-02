/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：多租户的拓展方法.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:43:59
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core;
using Obase.Core.Common;

namespace Obase.MultiTenant
{
    /// <summary>
    ///     多租户的拓展方法
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     启用多租户
        /// </summary>
        /// <param name="context">对象上下文</param>
        public static void EnableMultiTenant(this ObjectContext context)
        {
            context.RegisterModule(new MultiTenantModule());
        }

        /// <summary>
        ///     获取租户ID
        /// </summary>
        /// <param name="contextType">对象上下文类型</param>
        /// <returns></returns>
        public static object GetTenantId(Type contextType)
        {
            var reader = Utils.GetDependencyInjectionService<ITenantIdReader>(contextType);

            return reader.GetTenantId();
        }
    }
}