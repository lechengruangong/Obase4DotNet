/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：重建对象冲突处理器工厂.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:02:50
└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     冲突处理器工厂，该工厂创建的处理器用于执行“重建对象”策略。
    /// </summary>
    public class ReconstructConflictHandlerFactory : ConcurrentConflictHandlerFactory
    {
        /// <summary>
        ///     引发异常：发生“重复创建”冲突时不能适用“重建对象”处理策略。
        /// </summary>
        public override IRepeatCreationHandler CreateRepeatCreationHandler()
        {
            throw new InvalidOperationException("发生“重复创建”冲突时不能适用“重建对象”处理策略");
        }

        /// <summary>
        ///     引发异常：发生版本冲突时不能适用“重建对象”处理策略。
        /// </summary>
        public override IVersionConflictHandler CreateVersionConflictHandler()
        {
            //发生版本冲突时不能适用“重建对象”处理策略
            throw new NothingUpdatedException();
        }

        /// <summary>
        ///     创建“更新幻影”冲突的处理器。
        /// </summary>
        public override IUpdatingPhantomHandler CreateUpdatingPhantomHandler()
        {
            var reconstructConflictHandler = new ReconstructConflictHandler(Model, StorageProvider);
            return reconstructConflictHandler;
        }
    }
}