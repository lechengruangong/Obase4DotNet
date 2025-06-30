/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：实体对象筛选条件建造器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:15:37
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     实体对象筛选条件建造器。
    /// </summary>
    public class EntitySelectionCriteriaBuilder : ISelectionCriteriaBuilder
    {
        /// <summary>
        ///     筛选条件构造
        /// </summary>
        /// <param name="targetObj">对象</param>
        /// <param name="objectType">对象类型</param>
        /// <param name="mappingWorkflow">实施持久化的工作流机制</param>
        /// <returns>筛选条件</returns>
        public void Build(object targetObj, ObjectType objectType, IMappingWorkflow mappingWorkflow)
        {
            //过滤器
            var filter = mappingWorkflow.Or();
            if (objectType is EntityType entity)
                foreach (var key in entity.KeyAttributes)
                {
                    //要映射的字段和值
                    var attr = entity.GetAttribute(key);
                    var value = ObjectSystemVisitor.GetValue(targetObj, attr);
                    //片段
                    var segment = filter.AddSegment();
                    segment.SetField(attr.TargetField);
                    segment.SetReferenceValue(value);
                }

            filter.End();
        }
    }
}