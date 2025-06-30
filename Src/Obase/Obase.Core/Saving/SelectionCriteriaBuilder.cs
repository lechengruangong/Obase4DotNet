/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：筛选条件建造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:45:40
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     用于建造筛选条件。
    /// </summary>
    public class SelectionCriteriaBuilder : ISelectionCriteriaBuilder
    {
        /// <summary>
        ///     伴随关联对象条件建造器
        /// </summary>
        private CompanionAssociationSelectionCriteriaBuilder _companionBuilder;

        /// <summary>
        ///     对象筛选条件建造器
        /// </summary>
        private EntitySelectionCriteriaBuilder _entityBuilder;

        /// <summary>
        ///     独立关联对象条件建造器
        /// </summary>
        private IndependentAssociationSelectionCriteriaBuilder _independentBuilder;

        /// <summary>
        ///     建造筛选条件。
        /// </summary>
        /// <param name="targetObj">筛选条件要筛选的对象。</param>
        /// <param name="objectType">要筛选的对象的类型。</param>
        /// <param name="mappingWorkflow">实施持久化的工作流机制</param>
        public void Build(object targetObj, ObjectType objectType, IMappingWorkflow mappingWorkflow)
        {
            //实体型
            if (objectType is EntityType entity)
            {
                if (_entityBuilder == null)
                    _entityBuilder = new EntitySelectionCriteriaBuilder();
                _entityBuilder.Build(targetObj, entity, mappingWorkflow);
            }

            else if (objectType is AssociationType assoc)
            {
                //独立映射
                if (assoc.Independent)
                {
                    if (_independentBuilder == null)
                        _independentBuilder = new IndependentAssociationSelectionCriteriaBuilder();
                    _independentBuilder.Build(targetObj, assoc, mappingWorkflow);
                }
                //伴随映射
                else
                {
                    if (_companionBuilder == null)
                        _companionBuilder = new CompanionAssociationSelectionCriteriaBuilder();
                    _companionBuilder.Build(targetObj, objectType, mappingWorkflow);
                }
            }
        }
    }
}