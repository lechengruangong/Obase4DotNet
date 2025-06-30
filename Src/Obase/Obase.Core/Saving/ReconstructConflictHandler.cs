/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：发生并发冲突时重建对象.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:04:39
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     发生并发冲突时重建对象。
    /// </summary>
    public class ReconstructConflictHandler : ConcurrentConflictHandler, IUpdatingPhantomHandler
    {
        /// <summary>
        ///     用于执行Sql语句的执行器。
        /// </summary>
        private readonly IStorageProvider _storageProvider;

        /// <summary>
        ///     创建Reconstruct-ConflictHandler实例。
        /// </summary>
        /// <param name="model">对象数据模型。</param>
        /// <param name="storageProvider">在冲突处理过程中实施持久化的存储提供程序。</param>
        public ReconstructConflictHandler(ObjectDataModel model, IStorageProvider storageProvider) : base(model)
        {
            _storageProvider = storageProvider;
        }

        /// <summary>
        ///     处理更新幻影冲突。
        /// </summary>
        /// <param name="mappingUnit">映射执行器。</param>
        public void ProcessConflict(MappingUnit mappingUnit)
        {
            ProcessConflict(mappingUnit, EConcurrentConflictType.UpdatingPhantom);
        }

        /// <summary>
        ///     处理更新幻影冲突
        /// </summary>
        /// <param name="mappingUnit">映射执行器</param>
        /// <param name="conflictType">并发冲突类型</param>
        public override void ProcessConflict(MappingUnit mappingUnit, EConcurrentConflictType conflictType)
        {
            var mappingWorkflow = _storageProvider.CreateMappingWorkflow();
            mappingUnit.SaveNew(mappingWorkflow, Model, null, null);
        }
    }
}