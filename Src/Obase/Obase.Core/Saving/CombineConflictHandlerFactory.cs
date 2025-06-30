/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：冲突处理器工厂.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:53:34
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     冲突处理器工厂，该工厂创建的处理器用于执行“版本合并”策略。
    /// </summary>
    public class CombineConflictHandlerFactory : ConcurrentConflictHandlerFactory
    {
        /// <summary>
        ///     创建“重复创建”冲突的处理器。
        /// </summary>
        public override IRepeatCreationHandler CreateRepeatCreationHandler()
        {
            var combineConflictHandler =
                new CombineConflictHandler(Model, StorageProvider, AttributeOriginalValueGetter, AttributeHasChanged);
            return combineConflictHandler;
        }

        /// <summary>
        ///     创建版本冲突的处理器。
        /// </summary>
        public override IVersionConflictHandler CreateVersionConflictHandler()
        {
            var combineConflictHandler =
                new CombineConflictHandler(Model, StorageProvider, AttributeOriginalValueGetter, AttributeHasChanged);
            return combineConflictHandler;
        }

        /// <summary>
        ///     引发异常：发生“更新幻影”冲突时不能适用“版本合并”处理策略。
        /// </summary>
        public override IUpdatingPhantomHandler CreateUpdatingPhantomHandler()
        {
            throw new ArgumentException("发生“更新幻影”冲突时不能适用“版本合并”处理策略");
        }
    }
}