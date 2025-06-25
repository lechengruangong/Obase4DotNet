/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举属性的合并处理策略.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-23 17:13:27
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Odm.Builder
{
    /// <summary>
    ///     枚举属性的合并处理策略。
    /// </summary>
    public enum EAttributeCombinationHandlingStrategy
    {
        /// <summary>
        ///     覆盖——强制覆盖对方版本的值。
        /// </summary>
        Overwrite = 0,

        /// <summary>
        ///     忽略——忽略当前属性，即承认冲突对方版本的值。
        /// </summary>
        Ignore = 1,

        /// <summary>
        ///     累加——将当前版本中属性值的增量累加到对方版本。
        /// </summary>
        Accumulate = 2
    }
}