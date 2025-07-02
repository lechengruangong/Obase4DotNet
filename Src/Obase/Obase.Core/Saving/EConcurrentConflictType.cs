/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：枚举并发冲突类型.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-24 16:55:52
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Saving
{
    /// <summary>
    ///     枚举并发冲突类型。
    /// </summary>
    public enum EConcurrentConflictType : byte
    {
        /// <summary>
        ///     重复创建。
        /// </summary>
        RepeatCreation = 0,

        /// <summary>
        ///     版本冲突。
        /// </summary>
        VersionConflict = 1,

        /// <summary>
        ///     更新幻影。
        /// </summary>
        UpdatingPhantom = 2
    }
}