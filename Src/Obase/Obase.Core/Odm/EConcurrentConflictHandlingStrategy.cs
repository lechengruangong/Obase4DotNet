/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举并发冲突处理策略.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 17:11:34
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm
{
    /// <summary>
    ///     枚举并发冲突处理策略。
    /// </summary>
    public enum EConcurrentConflictHandlingStrategy : byte
    {
        /// <summary>
        ///     忽略。
        /// </summary>
        Ignore = 0,

        /// <summary>
        ///     引发异常。
        /// </summary>
        ThrowException = 1,

        /// <summary>
        ///     强制覆盖。
        /// </summary>
        Overwrite = 2,

        /// <summary>
        ///     版本合并。
        /// </summary>
        Combine = 3,

        /// <summary>
        ///     重建对象。
        /// </summary>
        Reconstruct = 4
    }
}
