/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：关联端映射器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:40:37
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     关联端映射器，封装特定于关联端的映射方案。
    /// </summary>
    public class AssociationEndMapper : RealElementMapper
    {
        /// <summary>
        ///     映射的关联端所属的关联型。
        /// </summary>
        private AssociationType _associationType;


        /// <summary>
        ///     获取或设置映射的关联端所属的关联型。
        /// </summary>
        public AssociationType AssociationType
        {
            get => _associationType;
            set => _associationType = value;
        }

        /// <summary>
        ///     确定是否应当选取指定的元素参与映射。
        /// </summary>
        /// <param name="element">要确定的元素。</param>
        /// <param name="objectType">元素所属对象的类型。</param>
        /// <param name="objectStatus">元素所属对象的状态。</param>
        /// <param name="attributeHasChanged">Predicate{String}委托，用于判定属性是否已修改。</param>
        public override bool Select(TypeElement element, ObjectType objectType, EObjectStatus objectStatus,
            Predicate<string> attributeHasChanged = null)
        {
            var associationType = objectType as AssociationType;
            var end = element as AssociationEnd;
            if (end == null || associationType == null)
                throw new ArgumentException("要选取的参与映射的元素必须为关联端,且对象必须为关联型.");
            return (objectStatus.Equals(EObjectStatus.Added) || objectStatus.Equals(EObjectStatus.Deleted)) &&
                   !associationType.IsCompanionEnd(end);
        }

        /// <summary>
        ///     将元素映射到字段，即生成字段设值器。
        /// </summary>
        /// <param name="element">要映射的元素。</param>
        /// <param name="obj">要映射的元素所属的对象。</param>
        /// <param name="mappingWorkflow">实施持久化的工作流机制。</param>
        public override void Map(TypeElement element, object obj, IMappingWorkflow mappingWorkflow)
        {
            if (element is AssociationEnd end)
                //遍历映射属性
                foreach (var mapp in end.Mappings)
                {
                    var keyAttr = mapp.KeyAttribute;
                    var targetFiled = mapp.TargetField;
                    object value = null;
                    //不是置空 取关联端的键属性
                    if (!SetNull) value = end.GetKeyAttributeValue(obj, keyAttr);
                    mappingWorkflow.SetField(targetFiled, value);
                }
        }
    }
}