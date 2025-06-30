/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：对象映射器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:23:14
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq;
using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     对象映射器，负责生成数据源、确定修改Sql语句的修改类型、生成筛选条件、生成字段设值器。
    /// </summary>
    public class ObjectMapper
    {
        /// <summary>
        ///     在对象映射过程中实施持久化的工作流机制。
        /// </summary>
        private readonly IMappingWorkflow _mappingWorkflow;

        /// <summary>
        ///     筛选条件建造器
        /// </summary>
        private SelectionCriteriaBuilder _criteriaBuilder;

        /// <summary>
        ///     元素映射器
        /// </summary>
        private ElementMapper _elementMapper;


        /// <summary>
        ///     创建ObjectMapper实例。
        /// </summary>
        /// <param name="mappingWorkflow">映射工作流机制。</param>
        public ObjectMapper(IMappingWorkflow mappingWorkflow)
        {
            _mappingWorkflow = mappingWorkflow;
        }

        /// <summary>
        ///     生成作为映射目标的查询源。
        /// </summary>
        /// <param name="objectType">要映射的对象的类型。</param>
        public void GenerateSource(ObjectType objectType)
        {
            _mappingWorkflow.SetSource(objectType.TargetTable);
        }

        /// <summary>
        ///     根据对象状态确定修改SQL的修改类型。
        /// </summary>
        /// <param name="objectStatus">对象状态。</param>
        /// <param name="objectType">要映射的对象的类型。</param>
        public void DetermineChangeType(EObjectStatus objectStatus, ObjectType objectType)
        {
            if (objectType is AssociationType associationType && associationType.Independent == false)
            {
                _mappingWorkflow.ForUpdating();
                return;
            }

            switch (objectStatus)
            {
                case EObjectStatus.Added:
                    _mappingWorkflow.ForInserting();
                    break;
                case EObjectStatus.Deleted:
                    _mappingWorkflow.ForDeleting();
                    break;
                case EObjectStatus.Modified:
                    _mappingWorkflow.ForUpdating();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(objectStatus), $"未知的对象状态{objectStatus}");
            }
        }

        /// <summary>
        ///     生成用于从数据源筛选指定对象的筛选条件。
        /// </summary>
        /// <param name="obj">要筛选的对象。</param>
        /// <param name="objectType">要筛选的对象的类型。</param>
        public void GenerateCriteria(object obj, ObjectType objectType)
        {
            if (_criteriaBuilder == null)
                _criteriaBuilder = new SelectionCriteriaBuilder();
            _mappingWorkflow.SetSource(objectType.TargetTable);
            _criteriaBuilder.Build(obj, objectType, _mappingWorkflow);
        }

        /// <summary>
        ///     生成用于从数据源筛选指定对象组的筛选条件。
        /// </summary>
        /// <param name="objs">要筛选的对象组。</param>
        /// <param name="objectType">对象组中对象的类型。</param>
        public void GenerateCriteria(object[] objs, ObjectType objectType)
        {
            foreach (var obj in objs) GenerateCriteria(obj, objectType);
        }

        /// <summary>
        ///     生成字段设值器，这些设值器用于将对象映射到表。
        /// </summary>
        /// <param name="obj">要映射的对象。</param>
        /// <param name="objectType">对象的类型。</param>
        /// <param name="objectStatus">对象的状态。</param>
        /// <param name="attributeHasChanged">一个委托，用于确定属性是否已修改。</param>
        public virtual void GenerateFieldSetter(object obj, ObjectType objectType,
            EObjectStatus objectStatus, Predicate<string> attributeHasChanged = null)
        {
            if (_elementMapper == null)
                _elementMapper = new ElementMapper();
            _elementMapper.ObjectType = objectType;
            //是否设置空值
            if (objectStatus.Equals(EObjectStatus.Added) || objectStatus.Equals(EObjectStatus.Modified))
                _elementMapper.SetNull = false;
            else
                _elementMapper.SetNull = true;

            var element = objectType.Elements.OrderBy(p => p.ElementType).ToList();

            foreach (var e in element)
            {
                var selected = _elementMapper.Select(e, objectType, objectStatus, attributeHasChanged);
                if (selected)
                    _elementMapper.Map(e, obj, _mappingWorkflow);
            }
        }
    }
}