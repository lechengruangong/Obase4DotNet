/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：发生并发冲突时执行强制覆盖.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:01:57
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     发生并发冲突时执行强制覆盖。
    /// </summary>
    public class OverwriteConflictHandler : ConcurrentConflictHandler, IRepeatCreationHandler, IVersionConflictHandler
    {
        /// <summary>
        ///     用于探测属性值是否发生更改的委托。
        /// </summary>
        private readonly Func<object, string, bool> _attributeHasChanged = (obj, attr) => true;

        /// <summary>
        ///     用于执行Sql语句的执行器。
        /// </summary>
        private readonly IStorageProvider _storageProvider;

        /// <summary>
        ///     创建Overwrite-ConflictHandler实例。
        /// </summary>
        /// <param name="model">对象数据模型。</param>
        /// <param name="storageProvider">在冲突处理过程中实施持久化的存储提供程序。</param>
        /// <param name="attributeHasChanged">用于探测属性是否发生更改的委托。</param>
        public OverwriteConflictHandler(ObjectDataModel model, IStorageProvider storageProvider,
            Func<object, string, bool> attributeHasChanged = null) : base(model)
        {
            _storageProvider = storageProvider;
            if (attributeHasChanged != null) _attributeHasChanged = attributeHasChanged;
        }

        /// <summary>
        ///     处理重复创建冲突。
        /// </summary>
        /// <param name="mappingUnit">映射执行器。</param>
        void IRepeatCreationHandler.ProcessConflict(MappingUnit mappingUnit)
        {
            ProcessConflict(mappingUnit, EConcurrentConflictType.RepeatCreation);
        }

        /// <summary>
        ///     处理版本冲突。
        /// </summary>
        /// <param name="mappingUnit">映射执行器。</param>
        void IVersionConflictHandler.ProcessConflict(MappingUnit mappingUnit)
        {
            ProcessConflict(mappingUnit, EConcurrentConflictType.VersionConflict);
        }

        /// <summary>
        ///     处理并发冲突。
        /// </summary>
        /// <param name="mappingUnit">映射执行器。</param>
        /// <param name="conflictType">并发冲突类型。</param>
        public override void ProcessConflict(MappingUnit mappingUnit, EConcurrentConflictType conflictType)
        {
            var mappingWorkflow = _storageProvider.CreateMappingWorkflow();
            mappingUnit.SaveOld(mappingWorkflow, false, Model, _attributeHasChanged, null, null);
        }
    }
}