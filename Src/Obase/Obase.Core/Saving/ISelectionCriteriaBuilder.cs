/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：筛选条件建造器接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:07:31
└──────────────────────────────────────────────────────────────┘
*/

using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     筛选条件建造器接口，定义抽象的建造器。
    /// </summary>
    public interface ISelectionCriteriaBuilder
    {
        /// <summary>
        ///     筛选条件构造
        /// </summary>
        /// <param name="targetObj">对象</param>
        /// <param name="objectType">对象类型</param>
        /// <param name="mappingWorkflow">实施持久化的工作流机制</param>
        /// <returns>筛选条件</returns>
        void Build(object targetObj, ObjectType objectType, IMappingWorkflow mappingWorkflow);
    }
}