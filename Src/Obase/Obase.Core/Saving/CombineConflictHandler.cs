/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：发生并发冲突时执行版本合并.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:51:04
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;
using Attribute = Obase.Core.Odm.Attribute;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     发生并发冲突时执行版本合并。
    /// </summary>
    public class CombineConflictHandler : ConcurrentConflictHandler, IRepeatCreationHandler, IVersionConflictHandler
    {
        /// <summary>
        ///     用于探测属性值是否发生更改的委托。
        /// </summary>
        private readonly Func<object, string, bool> _attributeHasChanged = (obj, attr) => true;

        /// <summary>
        ///     用于获取属性原值的委托。
        /// </summary>
        private readonly GetAttributeValue _attributeOriginalValueGetter;

        /// <summary>
        ///     用于执行Sql语句的执行器。
        /// </summary>
        private readonly IStorageProvider _storageProvider;

        /// <summary>
        ///     创建Combine-ConflictHandler实例。
        /// </summary>
        /// <param name="model">对象数据模型。</param>
        /// <param name="storageProvider">在冲突处理过程中实施持久化的存储提供程序。</param>
        /// <param name="attrOriginalValueGetter">用于获取属性原值的委托。</param>
        /// <param name="attrHasChanged">用于探测属性是否已更改的委托。</param>
        public CombineConflictHandler(ObjectDataModel model, IStorageProvider storageProvider,
            GetAttributeValue attrOriginalValueGetter = null,
            Func<object, string, bool> attrHasChanged = null) : base(model)
        {
            _storageProvider = storageProvider;
            _attributeOriginalValueGetter = attrOriginalValueGetter;
            if (attrHasChanged != null) _attributeHasChanged = attrHasChanged;
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
            var workFlow = _storageProvider.CreateMappingWorkflow();
            var hostType = Model.GetObjectType(mappingUnit.HostObject.GetType());

            workFlow.Begin();
            //设置为更新模式
            workFlow.ForUpdating();
            var objectMapper = new ObjectMapper(workFlow);
            objectMapper.GenerateSource(hostType);
            objectMapper.GenerateCriteria(mappingUnit.HostObject, hostType);

            var objItems = mappingUnit.MappingObjects;
            //处理每一个映射对象
            for (var i = 0; i < (objItems?.Count ?? 0); i++)
            {
                if (objItems != null)
                {
                    var objItem = objItems[i];
                    var itemType = Model.GetObjectType(objItem.GetType());

                    if (itemType == null) continue;
                    //构造版本合并上下文
                    var context =
                        new VersionCombinationContext(objItem, itemType, conflictType, _attributeOriginalValueGetter);
                    for (var j = 0; j < (itemType.Attributes?.Count ?? 0); j++)
                        if (itemType.Attributes != null)
                        {
                            var attr = itemType.Attributes[j];
                            _attributeHasChanged(objItem, attr.Name);
                            //可能是复杂属性或者一般属性 要进行属性合并
                            CombineAttribute(attr, workFlow, context);
                        }
                }

                //提交更改
                workFlow.Commit(null, null);
            }
        }

        /// <summary>
        ///     在对象执行版本合并期间，处理指定的属性。
        /// </summary>
        /// <param name="attribute">目标属性。</param>
        /// <param name="workflow">对象修改并实施持久化的工作流机制。</param>
        /// <param name="context">版本合并上下文。</param>
        private void CombineAttribute(Attribute attribute, IMappingWorkflow workflow, VersionCombinationContext context)
        {
            if (attribute.IsComplex)
            {
                var comType = ((ComplexAttribute)attribute).ComplexType;
                var parent = context.ParentAttribute ?? new AttributePath(comType);
                parent.GoDown(attribute);
                context.ParentAttribute = parent;
                //合并复杂属性的子属性
                for (var i = 0; i < (comType?.Attributes?.Count ?? 0); i++)
                    if (comType?.Attributes != null)
                    {
                        var subAttr = comType.Attributes[i];
                        context.ComplexObject = attribute.GetValue(context.Object);
                        context.ComplexAttribute = (ComplexAttribute)attribute;
                        CombineAttribute(subAttr, workflow, context);
                        context.ComplexObject = null;
                        context.ComplexAttribute = null;
                    }

                context.ParentAttribute = null;
            }
            else
            {
                var combiner = attribute.CombinationHandler;
                combiner?.Process(attribute, workflow, context);
            }
        }
    }
}