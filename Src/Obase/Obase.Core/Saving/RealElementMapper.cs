/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：元素的映射方案基类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:39:44
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     为不同元素的映射方案提供基础实现。
    /// </summary>
    public abstract class RealElementMapper : IElementMapper
    {
        /// <summary>
        ///     获取或设置一个值，该值指示是否将元素涉及的映射目标字段置空。
        /// </summary>
        public bool SetNull { get; set; }

        /// <summary>
        ///     确定是否应当选取指定的元素参与映射。
        /// </summary>
        /// <param name="element">要确定的元素。</param>
        /// <param name="objectType">元素所属对象的类型。</param>
        /// <param name="objectStatus">元素所属对象的状态。</param>
        /// <param name="attributeHasChanged">Predicate{String}委托，用于判定属性是否已修改。</param>
        public abstract bool Select(TypeElement element, ObjectType objectType, EObjectStatus objectStatus,
            Predicate<string> attributeHasChanged = null);

        /// <summary>
        ///     将元素映射到字段，即生成字段设值器。
        /// </summary>
        /// <param name="element">要映射的元素。</param>
        /// <param name="obj">要映射的元素所属的对象。</param>
        /// <param name="mappingWorkflow">实施持久化的工作流机制。</param>
        public abstract void Map(TypeElement element, object obj, IMappingWorkflow mappingWorkflow);
    }
}