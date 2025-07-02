/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：映射单元.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:48:46
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     映射单元。
    ///     映射单元由一个或多个对象组成，这些对象将映射到同一个表，它们的操作不可拆分，应由同一条Sql语句完成。
    /// </summary>
    public class MappingUnit
    {
        /// <summary>
        ///     伴随端队列
        /// </summary>
        private Queue<CompanionMapping> _companionMappings;

        /// <summary>
        ///     主体对象。
        /// </summary>
        private object _hostObject;

        /// <summary>
        ///     当前映射单元参照的对象集合。
        /// </summary>
        private List<object> _referredObjects;

        /// <summary>
        ///     获取伴随映射的对象及其状态。
        /// </summary>
        public List<CompanionMapping> Companions
        {
            get
            {
                if (_companionMappings == null)
                    _companionMappings = new Queue<CompanionMapping>();
                return _companionMappings.ToList();
            }
        }

        /// <summary>
        ///     获取映射单元的主体对象。
        /// </summary>
        public object HostObject => _hostObject;

        /// <summary>
        ///     获取当前映射单元参照的对象集合。
        /// </summary>
        public List<object> ReferredObjects => _referredObjects ?? (_referredObjects = new List<object>());

        /// <summary>
        ///     获取参与映射的对象，包含主体对象和伴随映射对象。
        /// </summary>
        public List<object> MappingObjects
        {
            get
            {
                var list = new List<object> { _hostObject };
                list.AddRange(Companions);
                return list;
            }
        }

        /// <summary>
        ///     向映射单元添加伴随关联对象，并指定其状态和被其参照的对象。
        /// </summary>
        /// <param name="companion">要添加的伴随关联对象</param>
        /// <param name="status">伴随关联对象的状态</param>
        /// <param name="referredObjs">关联对象参照的对象的集合</param>
        internal void AddCompanion(object companion, EObjectStatus status, object[] referredObjs)
        {
            ReferredObjects.AddRange(referredObjs);
            AddCompanion(companion, status);
        }

        /// <summary>
        ///     向映射单元添加伴随关联对象，并指定其状态。
        /// </summary>
        /// <param name="companion">要添加的伴随关联对象</param>
        /// <param name="status">伴随关联的状态</param>
        internal void AddCompanion(object companion, EObjectStatus status)
        {
            var companionMapping = new CompanionMapping(companion, status);
            if (_companionMappings == null)
                _companionMappings = new Queue<CompanionMapping>();
            _companionMappings.Enqueue(companionMapping);
        }

        /// <summary>
        ///     向映射单元添加主体对象。注：只有实体对象和独立关联对象才能作为主体对象。
        /// </summary>
        /// <param name="hostObj">要添加的对象</param>
        internal void AddHost(object hostObj)
        {
            _hostObject = hostObj;
        }

        /// <summary>
        ///     向映射单元添加关联对象，该关联对象将作为映射单元的主体对象。注：只有当关联对象为独立映射时才可作为主体对象。
        /// </summary>
        /// <param name="associationObj">要添加的关联对象</param>
        /// <param name="referredObjs">关联对象参照的对象的集合</param>
        internal void AddHost(object associationObj, object[] referredObjs)
        {
            _hostObject = associationObj;
            ReferredObjects.AddRange(referredObjs);
        }

        /// <summary>
        ///     将映射单元中的对象转换为特定的存储数据结构。
        /// </summary>
        /// <param name="mappingWorkflow">映射工作流机制。</param>
        /// <param name="status">映射单元的状态，即该单元中主对象的状态，不考虑伴随对象的状态。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="attributeHasChanged">
        ///     一个委托，用于检查对象的属性是否已修改。三个类型参数分别对应于要检查的对象、要检查的属性和检查结果。
        /// </param>
        private void MapObjects(IMappingWorkflow mappingWorkflow, EObjectStatus status, ObjectDataModel model,
            Func<object, string, bool> attributeHasChanged = null)
        {
            var objectMapper = new ObjectMapper(mappingWorkflow);

            bool Predicate(string s)
            {
                return attributeHasChanged != null && attributeHasChanged(_hostObject, s);
            }

            var flag = false;

            //处理宿主类型
            if (_hostObject != null)
            {
                var objType = model.GetObjectType(_hostObject.GetType());
                objectMapper.GenerateSource(objType);
                objectMapper.DetermineChangeType(status, objType);
                if (status != EObjectStatus.Added) objectMapper.GenerateCriteria(_hostObject, objType);
                objectMapper.GenerateFieldSetter(_hostObject, objType, status, Predicate);
                flag = true;
            }

            //处理伴随映射
            if (_companionMappings != null && _companionMappings.Count > 0)
                foreach (var cm in _companionMappings)
                {
                    var associanObj = cm.AssociationObject;
                    var associanType = model.GetAssociationType(associanObj.GetType());
                    if (!flag)
                    {
                        objectMapper.GenerateSource(associanType);
                        objectMapper.DetermineChangeType(cm.Status, associanType);
                        objectMapper.GenerateCriteria(associanObj, associanType);
                    }

                    objectMapper.GenerateFieldSetter(associanObj, associanType, cm.Status, Predicate);
                }
        }

        /// <summary>
        ///     在映射过程中校验对象版本。
        /// </summary>
        /// <param name="mappingWorkflow">映射工作流机制。</param>
        /// <param name="model">对象数据模型</param>
        /// <param name="attributeOriginalValueGetter">用于获取属性原始值的委托。</param>
        private void CheckVersion(IMappingWorkflow mappingWorkflow, ObjectDataModel model,
            GetAttributeValue attributeOriginalValueGetter = null)
        {
            var filter = mappingWorkflow.And();
            //处理每个对象
            var mappingObjs = MappingObjects;
            foreach (var mappingObj in mappingObjs)
            {
                if (mappingObj == null) continue;
                var objType = mappingObj is CompanionMapping companionMapping
                    ? model.GetObjectType(companionMapping.AssociationObject.GetType())
                    : model.GetObjectType(mappingObj.GetType());
                var attrNames = objType.VersionAttributes;
                if (attrNames != null && attrNames.Count > 0)
                    foreach (var attrName in attrNames)
                    {
                        var attr = objType.GetAttribute(attrName);
                        //取原始值
                        if (attributeOriginalValueGetter != null)
                        {
                            var value = attributeOriginalValueGetter.Invoke(mappingObj, attr);
                            var segment = filter.AddSegment();
                            segment.SetField(attrName);
                            segment.SetReferenceValue(value);
                        }
                    }
            }

            filter.End();
        }

        /// <summary>
        ///     保存新对象
        /// </summary>
        /// <param name="mappingWorkflow">映射工作流机制。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="preexecutionCallback">执行前回调委托。</param>
        /// <param name="postexecutionCallback">执行后回调委托。</param>
        public void SaveNew(IMappingWorkflow mappingWorkflow, ObjectDataModel model,
            Action<PreExecuteCommandEventArgs> preexecutionCallback,
            Action<PostExecuteCommandEventArgs> postexecutionCallback)
        {
            mappingWorkflow.Begin();
            //映射对象
            MapObjects(mappingWorkflow, EObjectStatus.Added, model);
            var mappingEntityType = model.GetEntityType(_hostObject.GetType());

            //标识自增
            if (mappingEntityType != null && mappingEntityType.KeyIsSelfIncreased)
            {
                mappingWorkflow.Commit(preexecutionCallback, postexecutionCallback, out var identity);
                ObjectSystemVisitor.SetValue(_hostObject, mappingEntityType, mappingEntityType.KeyAttributes[0],
                    identity);
            }
            else
            {
                //无自增标识
                mappingWorkflow.Commit(preexecutionCallback, postexecutionCallback);
            }
        }

        /// <summary>
        ///     保存旧对象
        /// </summary>
        /// <param name="mappingWorkflow">映射工作流机制。</param>
        /// <param name="checkVersion">指示是否进行版本校验。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="attributeHasChanged">一个委托，用于检查对象的属性是否已修改。三个类型参数分别对应于要检查的对象、要检查的属性和检查结果。</param>
        /// <param name="preexecutionCallback">执行前回调委托。</param>
        /// <param name="postexecutionCallback">执行后回调委托。</param>
        /// <param name="attributeOriginalValueGetter">用于获取属性原值的委托。</param>
        public void SaveOld(IMappingWorkflow mappingWorkflow, bool checkVersion, ObjectDataModel model,
            Func<object, string, bool> attributeHasChanged,
            Action<PreExecuteCommandEventArgs> preexecutionCallback,
            Action<PostExecuteCommandEventArgs> postexecutionCallback,
            GetAttributeValue attributeOriginalValueGetter = null)
        {
            mappingWorkflow.Begin();

            MapObjects(mappingWorkflow, EObjectStatus.Modified, model, attributeHasChanged);
            if (checkVersion)
                //如果要进行版本校验 进入版本校验流程
                CheckVersion(mappingWorkflow, model, attributeOriginalValueGetter);

            mappingWorkflow.Commit(preexecutionCallback, postexecutionCallback);
        }
    }
}