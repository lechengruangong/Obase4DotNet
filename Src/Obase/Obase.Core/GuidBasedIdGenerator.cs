/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：实现基于GUID的标识生成器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-7-2 10:46:36
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core
{
    /// <summary>
    ///     实现基于GUID的标识生成器。
    ///     生成规则：返回GUID的无连接符小写字符格式。
    /// </summary>
    public class GuidBasedIdGenerator : IDGenerator<string>
    {
        /// <summary>
        ///     生成下一个标识。
        /// </summary>
        public string Next()
        {
            return Guid.NewGuid().ToString("N").ToLowerInvariant();
        }
    }
}