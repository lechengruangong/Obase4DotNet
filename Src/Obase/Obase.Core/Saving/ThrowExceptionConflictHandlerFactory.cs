/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：引发异常冲突处理器工厂.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:56:15
└──────────────────────────────────────────────────────────────┘
*/

namespace Obase.Core.Saving
{
    /// <summary>
    ///     冲突处理器工厂，该工厂创建的处理器用于执行“引发异常”策略。
    /// </summary>
    public class ThrowExceptionConflictHandlerFactory : ConcurrentConflictHandlerFactory
    {
        /// <summary>
        ///     创建“重复创建”冲突的处理器。
        /// </summary>
        public override IRepeatCreationHandler CreateRepeatCreationHandler()
        {
            var throwExceptionConflictHandler = new ThrowExceptionConflictHandler(Model, AttributeOriginalValueGetter);
            return throwExceptionConflictHandler;
        }

        /// <summary>
        ///     创建版本冲突的处理器。
        /// </summary>
        public override IVersionConflictHandler CreateVersionConflictHandler()
        {
            var throwExceptionConflictHandler = new ThrowExceptionConflictHandler(Model, AttributeOriginalValueGetter);
            return throwExceptionConflictHandler;
        }

        /// <summary>
        ///     创建“更新幻影”冲突的处理器。
        /// </summary>
        public override IUpdatingPhantomHandler CreateUpdatingPhantomHandler()
        {
            var throwExceptionConflictHandler = new ThrowExceptionConflictHandler(Model, AttributeOriginalValueGetter);
            return throwExceptionConflictHandler;
        }
    }
}