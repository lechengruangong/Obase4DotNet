using System;
using System.Collections.Generic;
using Obase.MultiTenant;

namespace Obase.Test.Service;

/// <summary>
///     租户ID读取器
///     全局只有一个
/// </summary>
public class TenantIdReader : ITenantIdReader
{
    /// <summary>
    ///     获取租户ID
    /// </summary>
    /// <returns></returns>
    public object GetTenantId()
    {
        return TenantIdCenter.TenantIds[TenantIdCenter.CurrentUserIndex];
    }
}

/// <summary>
///     模拟的租户中心
/// </summary>
public static class TenantIdCenter
{
    /// <summary>
    ///     单例的租户列表
    /// </summary>
    public static readonly List<Guid> TenantIds = [Guid.NewGuid(), Guid.NewGuid(), new Guid()];

    /// <summary>
    ///     模拟的用户索引 切换此索引模拟切换了用户0和1分别是普通的用户ID 2是全局ID 3全是0
    /// </summary>
    public static int CurrentUserIndex { get; set; }
}