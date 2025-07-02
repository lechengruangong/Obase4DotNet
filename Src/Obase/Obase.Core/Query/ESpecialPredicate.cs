/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举断言是否启用特定访问逻辑返回的结果.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:37:13
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Query
{
    /// <summary>
    ///     枚举断言是否启用特定访问逻辑返回的结果。
    /// </summary>
    public enum ESpecialPredicate : byte
    {
        /// <summary>
        ///     不启用。
        /// </summary>
        False,

        /// <summary>
        ///     启用并替换通用访问逻辑。
        /// </summary>
        Substitute,

        /// <summary>
        ///     在执行通用访问逻辑前启用特定逻辑。
        /// </summary>
        PreExecute,

        /// <summary>
        ///     在执行通用访问逻辑后启用特定逻辑。
        /// </summary>
        PostExecute
    }
}