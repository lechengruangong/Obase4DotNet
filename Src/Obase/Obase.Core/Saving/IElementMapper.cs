/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：元素映射器接口.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:38:48
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     元素映射器接口，定义抽象的元素映射方案。
    /// </summary>
    public interface IElementMapper
    {
        /// <summary>
        ///     确定是否应当选取指定的元素参与映射。
        /// </summary>
        /// <param name="element">要确定的元素。</param>
        /// <param name="objectType">元素所属对象的类型。</param>
        /// <param name="objectStatus">元素所属对象的状态。</param>
        /// <param name="attributeHasChanged">Predicate{String}委托，用于判定属性是否已修改。</param>
        bool Select(TypeElement element, ObjectType objectType, EObjectStatus objectStatus,
            Predicate<string> attributeHasChanged = null);

        /// <summary>
        ///     将元素映射到字段，即生成字段设值器。
        /// </summary>
        /// <param name="element">要映射的元素。</param>
        /// <param name="obj">要映射的元素所属的对象。</param>
        /// <param name="mappingWorkflow">实施持久化的工作流机制。</param>
        void Map(TypeElement element, object obj, IMappingWorkflow mappingWorkflow);
    }
}