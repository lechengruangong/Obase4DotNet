/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：租户Id读取接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:45:41
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.MultiTenant
{
    /// <summary>
    ///     租户Id读取接口
    /// </summary>
    public interface ITenantIdReader
    {
        /// <summary>
        ///     获取租户ID
        /// </summary>
        /// <returns></returns>
        object GetTenantId();
    }
}