/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：元素映射器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:12:34
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;
using Attribute = Obase.Core.Odm.Attribute;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     元素映射器，用于将属性或关联端映射到字段。该映射器不执行实际的映射任务，而是将任务交由具体的元素映射器完成。
    /// </summary>
    public class ElementMapper : IElementMapper
    {
        /// <summary>
        ///     关联端映射器
        /// </summary>
        private readonly AssociationEndMapper _associationEndMapper;

        /// <summary>
        ///     属性映射器
        /// </summary>
        private readonly AttributeMapper _attributeMapper;


        /// <summary>
        ///     元素所属类型
        /// </summary>
        private ObjectType _objectType;

        /// <summary>
        ///     构造函数
        /// </summary>
        public ElementMapper()
        {
            _associationEndMapper = new AssociationEndMapper();
            _attributeMapper = new AttributeMapper();
        }


        /// <summary>
        ///     获取或设置映射的元素所属的类型。
        /// </summary>
        public ObjectType ObjectType
        {
            get => _objectType;
            set
            {
                _objectType = value;
                _associationEndMapper.AssociationType = _objectType as AssociationType;
            }
        }

        /// <summary>
        ///     获取或设置一个值，该值指示是否将元素涉及的映射目标字段置空。
        ///     实施说明
        ///     该访问器是对实际执行者同名访问器的冒泡。
        /// </summary>
        public bool SetNull
        {
            get => _attributeMapper.SetNull;
            set
            {
                _attributeMapper.SetNull = value;
                _associationEndMapper.SetNull = value;
            }
        }


        /// <summary>
        ///     确定是否应当选取指定的元素参与映射。
        /// </summary>
        /// <param name="element">要确定的元素。</param>
        /// <param name="objectType">要确定的元素所属对象的类型。</param>
        /// <param name="objectStatus">要确定的元素所属对象的状态。</param>
        /// <param name="attributeHasChanged">Predicate{String}委托，用于判定属性是否已修改。</param>
        public bool Select(TypeElement element, ObjectType objectType, EObjectStatus objectStatus,
            Predicate<string> attributeHasChanged = null)
        {
            if (element is Attribute)
                return _attributeMapper.Select(element, objectType, objectStatus, attributeHasChanged);
            if (element is AssociationEnd)
                return _associationEndMapper.Select(element, objectType, objectStatus, attributeHasChanged);
            return false;
        }

        /// <summary>
        ///     将元素映射到字段，即生成字段设值器。
        /// </summary>
        /// <param name="element">要映射的元素。</param>
        /// <param name="obj">要映射的元素所属的对象。</param>
        /// <param name="mappingWorkflow">实施持久化的工作流机制。</param>
        public void Map(TypeElement element, object obj, IMappingWorkflow mappingWorkflow)
        {
            if (element is Attribute)
                _attributeMapper.Map(element, obj, mappingWorkflow);
            else
                _associationEndMapper.Map(element, obj, mappingWorkflow);
        }
    }
}