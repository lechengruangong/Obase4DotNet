/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：为并发冲突处理器提供基础实现.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:50:18
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     为并发冲突处理器提供基础实现。
    /// </summary>
    public abstract class ConcurrentConflictHandler
    {
        /// <summary>
        ///     对象数据模型。
        /// </summary>
        private readonly ObjectDataModel _model;

        /// <summary>
        ///     创建ConcurrentConflictHandler实例。
        /// </summary>
        /// <param name="model">对象数据模型。</param>
        protected ConcurrentConflictHandler(ObjectDataModel model)
        {
            _model = model;
        }

        /// <summary>
        ///     获取对象数据模型。
        /// </summary>
        public ObjectDataModel Model => _model;

        /// <summary>
        ///     处理并发冲突。
        /// </summary>
        /// <param name="mappingUnit">映射执行器。</param>
        /// <param name="conflictType">并发冲突类型。</param>
        public abstract void ProcessConflict(MappingUnit mappingUnit, EConcurrentConflictType conflictType);
    }
}