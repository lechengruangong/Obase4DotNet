/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：标明类型为可映射类型并公开映射所需的信息.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 17:32:00
└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace Obase.Core.Odm
{
    /// <summary>
    ///     标明类型为可映射类型并公开映射所需的信息。
    /// </summary>
    public interface IMappable
    {
        /// <summary>
        ///     获取映射目标名称。
        /// </summary>
        string TargetName { get; set; }

        /// <summary>
        ///     获取标识成员的名称序列。
        /// </summary>
        string[] KeyMemberNames { get; }

        /// <summary>
        ///     获取标识成员的映射目标序列。
        /// </summary>
        List<string> KeyFields { get; set; }
    }
}