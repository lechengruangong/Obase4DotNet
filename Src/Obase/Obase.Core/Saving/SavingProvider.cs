/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：保存提供程序.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:34:29
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;
using IsolationLevel = System.Data.IsolationLevel;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     保存提供程序
    /// </summary>
    public class SavingProvider : ISavingPipeline, IDeletingPipeline, IDirectlyChangingPipeline
    {
        #region 构造函数

        /// <summary>
        ///     创建SavingProvider实例。
        /// </summary>
        /// <param name="model">对象数据模型</param>
        /// <param name="storageProviderCreator">创建存储提供程序的委托。</param>
        /// <param name="judge">默认的存储标记判定器</param>
        public SavingProvider(ObjectDataModel model, Func<StorageSymbol, IStorageProvider> storageProviderCreator,
            IStorageSymbolJudge judge = null)
        {
            _storageProviderCreator = storageProviderCreator;
            if (judge != null)
                _storageSymbolJudge = judge;
            _model = model;
        }

        #endregion

        #region 属性访问器

        /// <summary>
        ///     一个委托，用于构造存储提供程序。
        /// </summary>
        public Func<StorageSymbol, IStorageProvider> StorageProviderCreator => _storageProviderCreator;

        #endregion

        #region 字段

        /// <summary>
        ///     对象数据模型
        /// </summary>
        private readonly ObjectDataModel _model;

        /// <summary>
        ///     存储标记判定器
        /// </summary>
        private readonly IStorageSymbolJudge _storageSymbolJudge = new StorageSymbolJudge();

        /// <summary>
        ///     一个委托，用于构造存储提供程序。
        /// </summary>
        private readonly Func<StorageSymbol, IStorageProvider> _storageProviderCreator;

        /// <summary>
        ///     实施持久化的存储提供程序。
        /// </summary>
        private readonly Dictionary<StorageSymbol, IStorageProvider> _storageProviders =
            new Dictionary<StorageSymbol, IStorageProvider>();

        #endregion

        #region 事件

        #region 保存管道

        /// <summary>
        ///     保存管道 执行SQL前事件
        /// </summary>
        private EventHandler<PreExecuteCommandEventArgs> _savingPreExecuteCommand;

        /// <summary>
        ///     保存管道 执行SQL后事件
        /// </summary>
        private EventHandler<PostExecuteCommandEventArgs> _savingPostExecuteCommand;


        /// <summary>
        ///     结束删除事件
        /// </summary>
        public event EventHandler EndDeleting;


        /// <summary>
        ///     开始保存时触发事件
        /// </summary>
        public event EventHandler BeginSaving;

        /// <summary>
        ///     生成队列后触发事件
        /// </summary>
        public event EventHandler PostGenerateQueue;

        /// <summary>
        ///     开始保存映射单元触发事件
        /// </summary>
        public event EventHandler<BeginSavingUnitEventArgs> BeginSavingUnit;

        /// <summary>
        ///     保存管道 执行SQL前事件
        /// </summary>
        event EventHandler<PreExecuteCommandEventArgs> ISavingPipeline.PreExecuteCommand
        {
            add => _savingPreExecuteCommand += value;
            remove => _savingPreExecuteCommand -= value;
        }


        /// <summary>
        ///     保存管道 执行SQL后事件
        /// </summary>
        event EventHandler<PostExecuteCommandEventArgs> ISavingPipeline.PostExecuteCommand
        {
            add => _savingPostExecuteCommand += value;
            remove => _savingPostExecuteCommand -= value;
        }

        /// <summary>
        ///     结束保存映射单元触发事件
        /// </summary>
        public event EventHandler<EndSavingUnitEventArgs> EndSavingUnit;

        /// <summary>
        ///     结束保存触发事件
        /// </summary>
        public event EventHandler EndSaving;

        #endregion

        #region 删除管道

        /// <summary>
        ///     删除管道 执行SQL前事件
        /// </summary>
        private EventHandler<PreExecuteCommandEventArgs> _deletingPreExecuteCommand;

        /// <summary>
        ///     删除管道 执行SQL后事件
        /// </summary>
        private EventHandler<PostExecuteCommandEventArgs> _deletingPostExecuteCommand;


        /// <summary>
        ///     开始删除触发事件
        /// </summary>
        public event EventHandler BeginDeleting;

        /// <summary>
        ///     结束生成删除组触发事件
        /// </summary>
        public event EventHandler PostGenerateGroup;

        /// <summary>
        ///     开始删除组触发事件
        /// </summary>
        public event EventHandler<BeginDeletingGroupEventArgs> BeginDeletingGroup;

        /// <summary>
        ///     保存管道 执行SQL前事件
        /// </summary>
        event EventHandler<PreExecuteCommandEventArgs> IDeletingPipeline.PreExecuteCommand
        {
            add => _deletingPreExecuteCommand += value;
            remove => _deletingPreExecuteCommand -= value;
        }

        /// <summary>
        ///     保存管道 执行SQL后事件
        /// </summary>
        event EventHandler<PostExecuteCommandEventArgs> IDeletingPipeline.PostExecuteCommand
        {
            add => _deletingPostExecuteCommand += value;
            remove => _deletingPostExecuteCommand -= value;
        }


        /// <summary>
        ///     结束删除组触发事件
        /// </summary>
        public event EventHandler<EndDeletingGroupEventArgs> EndDeletingGroup;

        #endregion

        #region 直接修改管道

        /// <summary>
        ///     删除管道 执行SQL前事件
        /// </summary>
        private EventHandler<PreExecuteCommandEventArgs> _directlyPreExecuteCommand;

        /// <summary>
        ///     删除管道 执行SQL后事件
        /// </summary>
        private EventHandler<PostExecuteCommandEventArgs> _directlyPostExecuteCommand;


        /// <summary>
        ///     为BeginDirectlyChanging事件附加或移除事件处理程序。
        /// </summary>
        public event EventHandler<BeginDirectlyChangingEventArgs> BeginDirectlyChanging;


        /// <summary>
        ///     保存管道 执行SQL前事件
        /// </summary>
        event EventHandler<PreExecuteCommandEventArgs> IDirectlyChangingPipeline.PreExecuteCommand
        {
            add => _directlyPreExecuteCommand += value;
            remove => _directlyPreExecuteCommand -= value;
        }

        /// <summary>
        ///     保存管道 执行SQL后事件
        /// </summary>
        event EventHandler<PostExecuteCommandEventArgs> IDirectlyChangingPipeline.PostExecuteCommand
        {
            add => _directlyPostExecuteCommand += value;
            remove => _directlyPostExecuteCommand -= value;
        }

        /// <summary>
        ///     为EndDirectlyChanging事件附加或移除事件处理程序。
        /// </summary>
        public event EventHandler<EndDirectlyChangingEventArgs> EndDirectlyChanging;

        #endregion

        #endregion

        #region 保存方法

        /// <summary>
        ///     将对象的当前状态持久化至存储服务。
        /// </summary>
        /// <param name="added">新增的对象。</param>
        /// <param name="modified">已修改过的对象。</param>
        /// <param name="deleted">已删除的对象。</param>
        /// <param name="addedComps">新增的伴随关联。</param>
        /// <param name="deletedComps">已删除的伴随关联。</param>
        /// <param name="attrHasChanged">一个委托，用于探测对象的属性是否已更改。</param>
        /// <param name="attrOriginalValueGetter">用于获取属性原值的委托。</param>
        public void Save(List<object> added, List<object> modified, List<object> deleted, List<object> addedComps,
            List<object> deletedComps, Func<object, string, bool> attrHasChanged,
            GetAttributeValue attrOriginalValueGetter)
        {
            //准备存储提供程序
            PrepareStorageProvider(added, modified, deleted, addedComps, deletedComps);

            //是否在我开启事务前已经开启了事务
            var isOutTrBegun = _storageProviders.Values.Any(p => p.TransactionBegun);

            //当前的环境事务
            TransactionScope transactionScope = null;

            if (added.Count + modified.Count + deleted.Count > 1 || addedComps.Count > 0 || deletedComps.Count > 0)
                //开启事务
                BeginTransaction(ref transactionScope);
            else
                foreach (var providers in _storageProviders.Values)
                    providers.BeginTransaction();

            try
            {
                //处理具体的操作
                if (addedComps?.Count > 0)
                {
                    SaveNew(added);
                    SaveOld(modified, addedComps, deletedComps, attrHasChanged, attrOriginalValueGetter);
                    Delete(deleted);
                }
                else
                {
                    SaveOld(modified, addedComps, deletedComps, attrHasChanged, attrOriginalValueGetter);
                    Delete(deleted);
                    SaveNew(added);
                }

                foreach (var providers in _storageProviders.Values)
                    if (!isOutTrBegun)
                        providers.CommitTransaction();

                transactionScope?.Complete();
            }
            catch
            {
                foreach (var providers in _storageProviders.Values) providers.RollbackTransaction();
                throw;
            }
            finally
            {
                //释放资源
                foreach (var providers in _storageProviders.Values)
                    if (!isOutTrBegun)
                        providers.ReleaseResource();

                transactionScope?.Dispose();
            }
        }

        /// <summary>
        ///     开启事务
        /// </summary>
        /// <param name="transactionScope">环境事务</param>
        /// <returns></returns>
        private void BeginTransaction(ref TransactionScope transactionScope)
        {
            //已开启环境事务
            if (Transaction.Current != null)
            {
                //环境事务
                EnlistTransaction(ref transactionScope);
            }
            else
            {
                //如果只有一个存储提供程序
                if (_storageProviders.Count <= 1)
                {
                    var first = _storageProviders.Values.First();
                    if (first is ITransactionable transactionable)
                        transactionable.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                //多个 登记环境事务
                else
                {
                    EnlistTransaction(ref transactionScope);
                }
            }
        }

        /// <summary>
        ///     登记事务
        /// </summary>
        private void EnlistTransaction(ref TransactionScope transactionScope)
        {
            foreach (var provider in _storageProviders)
                if (provider.Value is IAmbientTransactionable ambientTransactionable)
                {
                    if (transactionScope == null && Transaction.Current == null)
                        transactionScope = new TransactionScope(TransactionScopeOption.Required,
                            new TransactionOptions
                                { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted },
                            TransactionScopeAsyncFlowOption.Enabled);
                    ambientTransactionable.EnlistTransaction();
                }
        }


        #region 准备存储程序

        /// <summary>
        ///     根据待保存的对象准备存储提供程序。
        /// </summary>
        /// <param name="added">新增的对象。</param>
        /// <param name="modified">已修改的对象。</param>
        /// <param name="deleted">已删除的对象。</param>
        /// <param name="addedComps">新增的连带对象</param>
        /// <param name="deletedComps">删除的连带对象</param>
        private void PrepareStorageProvider(List<object> added, List<object> modified, List<object> deleted,
            List<object> addedComps,
            List<object> deletedComps)
        {
            var total = new List<object>();
            total.AddRange(added);
            total.AddRange(modified);
            total.AddRange(deleted);
            total.AddRange(addedComps);
            total.AddRange(deletedComps);

            var enumerator = total.GetEnumerator();
            CreateStorageProvider(enumerator);
        }

        /// <summary>
        ///     通过枚举器实现挨个处理
        /// </summary>
        /// <param name="enumerator">枚举器</param>
        private void CreateStorageProvider(IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
                if (enumerator.Current != null)
                {
                    //获取对象类型
                    var objectType = _model.GetObjectType(enumerator.Current.GetType());
                    GenerateSymbolByObjectType(enumerator.Current, objectType);
                }
        }

        /// <summary>
        ///     根据ObjectType获取存储提供器
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="objectType">对象类型</param>
        private void GenerateSymbolByObjectType(object obj, ObjectType objectType)
        {
            var storgeSymbol = _storageSymbolJudge.Judge(obj, objectType);
            //不存在 添加一个新的
            if (!_storageProviders.ContainsKey(storgeSymbol))
            {
                var storageProvider = _storageProviderCreator(storgeSymbol);
                _storageProviders.Add(storgeSymbol, storageProvider);
            }
        }

        /// <summary>
        ///     根据ObjectType获取存储提供器
        /// </summary>
        /// <param name="objectType">对象类型</param>
        private void GenerateSymbolByObjectType(ObjectType objectType)
        {
            var storgeSymbols = _storageSymbolJudge.Judge(objectType);
            foreach (var storgeSymbol in storgeSymbols)
                //不存在 添加一个新的
                if (!_storageProviders.ContainsKey(storgeSymbol))
                {
                    var storageProvider = _storageProviderCreator(storgeSymbol);
                    _storageProviders.Add(storgeSymbol, storageProvider);
                }
        }

        #endregion

        #region 保存旧对象

        /// <summary>
        ///     保存旧对象。
        /// </summary>
        /// <param name="objects">要保存的对象集合</param>
        /// <param name="added">要新增的对象集合</param>
        /// <param name="deleted">要删除的对象集合</param>
        /// <param name="attributeHasChanged">
        ///     一个委托，用于检查对象的属性是否已更改。三个类型参数分别对应于要检查的对象、属性名称和是否已更改。
        /// </param>
        /// <param name="attributeOriginalValueGetter">用于获取属性原值的委托</param>
        private void SaveOld(List<object> objects, List<object> added, List<object> deleted,
            Func<object, string, bool> attributeHasChanged, GetAttributeValue attributeOriginalValueGetter)
        {
            //触发开始保存事件
            BeginSaving?.Invoke(this, EventArgs.Empty);

            var set = GenerateMappingSet(objects, added, deleted);

            //执行集合中的每个映射
            var index = 0;

            while (index < set.Count)
            {
                var unit = set[index];

                //触发开始保存事件
                BeginSavingUnit?.Invoke(this, new BeginSavingUnitEventArgs(unit, EObjectStatus.Modified));

                ObjectType objType;
                if (unit.HostObject != null)
                {
                    objType = _model.GetObjectType(unit.HostObject.GetType());
                }
                else
                {
                    var obj = (CompanionMapping)unit.MappingObjects.FirstOrDefault(p => p != null);
                    objType = _model.GetObjectType(obj?.AssociationObject.GetType());
                }

                var symbol = _storageSymbolJudge.Judge(null, objType);
                var provider = _storageProviders[symbol];
                var workflow = provider.CreateMappingWorkflow();

                try
                {
                    unit.SaveOld(workflow, true, _model, attributeHasChanged,
                        args => _savingPreExecuteCommand?.Invoke(this, args),
                        args => _savingPostExecuteCommand?.Invoke(this, args), attributeOriginalValueGetter);

                    //触发结束保存事件
                    EndSavingUnit?.Invoke(this, new EndSavingUnitEventArgs(unit, EObjectStatus.Modified));
                }
                catch (NothingUpdatedException)
                {
                    //取出策略工厂
                    var factory =
                        ConcurrentConflictHandlerFactory.ChooseFactory(objType.ConcurrentConflictHandlingStrategy);
                    //忽略策略
                    if (factory == null)
                    {
                        index++;
                        continue;
                    }

                    //给工厂设值
                    factory.Model = _model;
                    factory.StorageProvider = provider;
                    factory.AttributeHasChanged = attributeHasChanged;
                    factory.AttributeOriginalValueGetter = attributeOriginalValueGetter;

                    try
                    {
                        //处理此次冲突
                        var handler = factory.CreateVersionConflictHandler();
                        handler.ProcessConflict(unit);
                        //触发结束保存事件
                        EndSavingUnit?.Invoke(this, new EndSavingUnitEventArgs(unit, EObjectStatus.Modified));
                    }
                    catch (NothingUpdatedException)
                    {
                        var newhandler = factory.CreateUpdatingPhantomHandler();
                        newhandler.ProcessConflict(unit);
                        //触发结束保存事件
                        EndSavingUnit?.Invoke(this, new EndSavingUnitEventArgs(unit, EObjectStatus.Modified));
                    }
                }
                catch (Exception ex)
                {
                    //触发结束保存事件
                    EndSavingUnit?.Invoke(this, new EndSavingUnitEventArgs(unit, EObjectStatus.Modified, ex));
                    throw;
                }

                index++;
            }
        }

        /// <summary>
        ///     生成更新映射集。将待执行更新映射的对象划分为一组映射单元，划分依据为：实体对象和独立关联取其键值、伴随关联取其伴随端的键值，键值相等者为一个单元。
        /// </summary>
        /// <param name="objects">要保存的对象的集合。</param>
        /// <param name="added">新增的伴随关联对象集合。</param>
        /// <param name="deleted">已删除的伴随关联对象集合。</param>
        private UpdateMappingSet GenerateMappingSet(List<object> objects, List<object> added, List<object> deleted)
        {
            var set = new UpdateMappingSet();
            foreach (var obj in objects)
            {
                var mt = _model.GetObjectType(obj.GetType());
                if (mt is AssociationType associationType && !associationType.Independent)
                    set.AddCompanion(obj, associationType, EObjectStatus.Modified);
                else
                    set.AddHost(obj, mt);
            }

            foreach (var delete in deleted)
            {
                var mt = _model.GetAssociationType(delete.GetType());
                set.AddCompanion(delete, mt, EObjectStatus.Deleted);
            }

            foreach (var add in added)
            {
                var mt = _model.GetAssociationType(add.GetType());
                set.AddCompanion(add, mt, EObjectStatus.Added);
            }

            return set;
        }

        #endregion

        #region 删除对象

        /// <summary>
        ///     删除对象。
        /// </summary>
        /// <param name="objects">要删除的对象的集合</param>
        private void Delete(List<object> objects)
        {
            //剔除连带对象
            RejectJointObjects(ref objects);

            //触发开始删除事件
            BeginDeleting?.Invoke(this, EventArgs.Empty);

            var groups = objects.GroupBy(o => o.GetType());
            //触发结束分组事件
            PostGenerateGroup?.Invoke(this, EventArgs.Empty);
            //删除
            foreach (var group in groups)
            {
                var objectType = _model.GetObjectType(group.Key);
                //准备存储程序
                var symbol = _storageSymbolJudge.Judge(null, objectType);
                var provider = _storageProviders[symbol];
                var workflow = provider.CreateMappingWorkflow();
                //要删除的对象们
                var objs = group.ToArray();

                //开始删除一组对象事件
                BeginDeletingGroup?.Invoke(this, new BeginDeletingGroupEventArgs(objectType, objs));

                try
                {
                    DeleteGroup(group.ToArray(), objectType, workflow,
                        args => _deletingPreExecuteCommand?.Invoke(this, args),
                        args => _deletingPostExecuteCommand?.Invoke(this, args));

                    EndDeletingGroup?.Invoke(this, new EndDeletingGroupEventArgs(objectType, objs));
                }
                catch (Exception ex)
                {
                    EndDeletingGroup?.Invoke(this, new EndDeletingGroupEventArgs(objectType, objs, ex));
                    throw;
                }
            }

            //触发结束删除事件
            EndDeleting?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     删除一组对象
        /// </summary>
        /// <param name="objects">要删除的对象的集合</param>
        /// <param name="objectType">对象类型</param>
        /// <param name="mappingWorkflow">工作流</param>
        /// <param name="preexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）前回调的方法。</param>
        /// <param name="postexecutionCallback">一个委托，代表在执行存储指令（如SQL语句）后回调的方法。</param>
        private void DeleteGroup(object[] objects, ObjectType objectType, IMappingWorkflow mappingWorkflow,
            Action<PreExecuteCommandEventArgs> preexecutionCallback,
            Action<PostExecuteCommandEventArgs> postexecutionCallback)
        {
            mappingWorkflow.Begin();
            var objectMapper = new ObjectMapper(mappingWorkflow);
            objectMapper.GenerateCriteria(objects, objectType);
            if (objectType is EntityType ||
                (objectType is AssociationType associationType && associationType.Independent))
            {
                mappingWorkflow.ForDeleting();
            }
            else
            {
                if (objectType is AssociationType associationType1 && !associationType1.CompanionEnd.IsAggregated)
                {
                    mappingWorkflow.ForUpdating();
                    foreach (var end in associationType1.AssociationEnds)
                        if (!end.IsCompanionEnd())
                            foreach (var mapping in end.Mappings)
                                mappingWorkflow.SetField(mapping.TargetField, null);
                }
            }

            mappingWorkflow.DeleteCascade(objectType);

            mappingWorkflow.Commit(preexecutionCallback, postexecutionCallback);
        }

        /// <summary>
        ///     剔除连带对象。
        /// </summary>
        /// <param name="objs">要从中筛选出连带对象并加以剔除的对象组</param>
        private void RejectJointObjects(ref List<object> objs)
        {
            //查找
            var hashSet = new HashSet<ObjectKey>();
            var nullRefs = new List<Tuple<ReferenceElement, object>>();
            foreach (var obj in objs)
            {
                var structuralType = _model.GetStructuralType(obj.GetType());
                var objectKey = ObjectSystemVisitor.GetObjectKey(obj, structuralType);
                if (!hashSet.Contains(objectKey))
                    switch (structuralType)
                    {
                        case EntityType entityType:
                            AnalyzeObject(obj, entityType, hashSet, nullRefs);
                            break;
                        case AssociationType associationType:
                            AnalyzeAssociation(obj, associationType, "", hashSet, nullRefs);
                            break;
                    }
            }

            //剔除
            for (var i = objs.Count - 1; i >= 0; i--)
            {
                var obj = objs.ElementAt(i);
                if (hashSet.Contains(
                        ObjectSystemVisitor.GetObjectKey(obj, _model.GetStructuralType(obj.GetType()))))
                    objs.Remove(obj);
            }

            foreach (var tuple in nullRefs)
            {
                var filterObj = objs.ToArray();
                tuple.Item1.FilterTarget(ref filterObj, tuple.Item2, null, true);
                objs = filterObj.ToList();
            }
        }

        /// <summary>
        ///     分析关联。
        /// </summary>
        /// <param name="associationObj">目标关联对象</param>
        /// <param name="associationType">目标关联对象的类型</param>
        /// <param name="excludedEnd">过滤端</param>
        /// <param name="jointObjects">连带对象的容器，在分析过程中发现的连带对象将被放入此容器</param>
        /// <param name="nullRefs">空引用的容器，收集分析过程中发现的空引用，每一个空引用保存为一个元组，其中第一个元素为表示该引用的ReferenceElement实例，第二个元素该引用的宿主对象。</param>
        private void AnalyzeAssociation(object associationObj, AssociationType associationType, string excludedEnd,
            HashSet<ObjectKey> jointObjects, List<Tuple<ReferenceElement, object>> nullRefs)
        {
            foreach (var end in associationType.AssociationEnds)
            {
                var aggregated = end.IsAggregated;
                if (excludedEnd == end.Name)
                    if (aggregated)
                    {
                        var endObj = ObjectSystemVisitor.GetValue(associationObj, end);
                        if (endObj == null)
                        {
                            nullRefs.Add(new Tuple<ReferenceElement, object>(end, associationObj));
                        }
                        else
                        {
                            var entityType = end.EntityType;
                            var key = ObjectSystemVisitor.GetObjectKey(endObj, entityType);
                            if (jointObjects.Add(key)) AnalyzeObject(endObj, entityType, jointObjects, nullRefs);
                        }
                    }
            }
        }

        /// <summary>
        ///     分析对象。
        /// </summary>
        /// <param name="entityObj">目标对象</param>
        /// <param name="entityType">目标对象的类型</param>
        /// <param name="jointObjs">连带对象的容器，在分析过程中发现的连带对象将被放入此容器</param>
        /// <param name="nullRefs">空引用的容器，收集分析过程中发现的空引用，每一个空引用保存为一个元组，其中第一个元素为表示该引用的ReferenceElement实例，第二个元素该引用的宿主对象。</param>
        private void AnalyzeObject(object entityObj, EntityType entityType, HashSet<ObjectKey> jointObjs,
            List<Tuple<ReferenceElement, object>> nullRefs)
        {
            foreach (var item in entityType.AssociationReferences)
            {
                //取出（重数大于1）关联型集合
                var assObjs = ObjectSystemVisitor.AssociationNavigate(entityObj, item);

                if (assObjs == null)
                    nullRefs.Add(new Tuple<ReferenceElement, object>(item, entityObj));
                else
                    foreach (var assObj in assObjs)
                    {
                        var key = ObjectSystemVisitor.GetObjectKey(assObj, item.AssociationType);
                        if (jointObjs.Add(key))
                            AnalyzeAssociation(assObj, item.AssociationType, item.LeftEnd, jointObjs, nullRefs);
                    }
            }
        }

        #endregion

        #region 保存新对象

        /// <summary>
        ///     保存新对象。
        /// </summary>
        /// <param name="objects">要保存的对象集合</param>
        private void SaveNew(List<object> objects)
        {
            if (objects != null && objects.Count > 0)
            {
                //委托，判断对象是否在要保存的对象集合
                bool IsSaving(object o)
                {
                    return objects.Contains(o);
                }

                //生成对象参照图
                var g = GenerateObjectReferenceGraphic(objects, IsSaving);
                //生成映射队列（边缘节点先插入）
                var queue = GenerateMappingQueue(g);

                //触发结束分组
                PostGenerateQueue?.Invoke(this, EventArgs.Empty);

                while (queue.Count > 0)
                {
                    var unit = queue.Dequeue();
                    if (unit != null)
                    {
                        //如果主体对象是空
                        if (unit.HostObject == null)
                            throw new ArgumentException($"无法获取保存单元的主体对象,请参考映射单元的映射对象{string.Join(",", GenNullHostObjectExceptionMessage(unit))}检查相应的配置.");

                        //触发开始保存事件
                        BeginSavingUnit?.Invoke(this, new BeginSavingUnitEventArgs(unit, EObjectStatus.Added));

                        var objType = _model.GetObjectType(unit.HostObject.GetType());
                        var symbol = _storageSymbolJudge.Judge(unit.HostObject, objType);
                        var provider = _storageProviders[symbol];
                        var workflow = provider.CreateMappingWorkflow();

                        try
                        {
                            unit.SaveNew(workflow, _model, args => _savingPreExecuteCommand?.Invoke(this, args),
                                args => _savingPostExecuteCommand?.Invoke(this, args));

                            //触发结束保存事件
                            EndSavingUnit?.Invoke(this, new EndSavingUnitEventArgs(unit, EObjectStatus.Added));
                        }
                        catch (RepeatInsertionException ex)
                        {
                            //如果是不支持导致的 就只能处理为抛出异常
                            if (ex.IsUnSupported && objType.ConcurrentConflictHandlingStrategy !=
                                EConcurrentConflictHandlingStrategy.ThrowException)
                                throw new UnSupportedException(unit.HostObject, objType, ex);

                            //取出策略工厂
                            var factory =
                                ConcurrentConflictHandlerFactory.ChooseFactory(objType
                                    .ConcurrentConflictHandlingStrategy);

                            if (factory == null) continue;

                            //给工厂设值
                            factory.Model = _model;
                            factory.StorageProvider = provider;

                            //处理此次冲突
                            var handler = factory.CreateRepeatCreationHandler();
                            handler.ProcessConflict(unit);
                            //触发结束保存事件
                            EndSavingUnit?.Invoke(this, new EndSavingUnitEventArgs(unit, EObjectStatus.Modified));
                        }
                        catch (Exception ex)
                        {
                            //触发结束保存事件
                            EndSavingUnit?.Invoke(this, new EndSavingUnitEventArgs(unit, EObjectStatus.Added, ex));
                            throw;
                        }
                    }
                }
            }

            //触发结束保存事件
            EndSaving?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        ///     生成对象参照图。
        /// </summary>
        /// <param name="objs">要生成对象参照图的对象集合</param>
        /// <param
        ///     name="isSaving">
        ///     一个委托，用于检查传入的对象是否为正在执行保存操作的对象，如果是返回true。第一个参数为传入的对象，第二个参数为返回值。
        /// </param>
        private ObjectReferenceGraphic GenerateObjectReferenceGraphic(List<object> objs, Func<object, bool> isSaving)
        {
            var graphic = new ObjectReferenceGraphic();
            if (objs == null || objs.Count <= 0) return null;
            var graphicGenerator = new ObjectReferenceGraphicGenerator();
            //遍历对象分析
            foreach (var item in objs)
            {
                //获取对象模型
                var mt = _model.GetStructuralType(item.GetType());
                if (mt is EntityType)
                    graphicGenerator.AnalyzeObject(item, isSaving.Invoke, graphic);
                else if (mt is AssociationType associationType)
                    graphicGenerator.AnalyzeAssociation(item, associationType, isSaving.Invoke, graphic);
            }

            //返回图
            return graphic;
        }

        /// <summary>
        ///     根据对象参照图生成映射队列。
        /// </summary>
        /// <param name="graphic">对象参照图</param>
        private Queue<MappingUnit> GenerateMappingQueue(ObjectReferenceGraphic graphic)
        {
            var queue = new Queue<MappingUnit>();
            var hashSet = new HashSet<object>();
            while (graphic.Count > 0)
            {
                var enqueueNum = 0;
                for (var i = 0; i < graphic.Count; i++)
                {
                    var unit = graphic[i];
                    //{参照对象总数减(-)已放入队列的参照对象}(及为 未放入队列的参照对象数)
                    var countReference = 0;
                    if (unit.ReferredObjects != null && unit.ReferredObjects.Count > 0)
                        foreach (var item in unit.ReferredObjects)
                            if (!hashSet.Contains(item))
                            {
                                countReference++;
                                break;
                            }

                    if (countReference == 0)
                    {
                        queue.Enqueue(unit);
                        hashSet.Add(unit.HostObject);
                        graphic.Remove(i);
                        i--;
                        enqueueNum++;
                    }
                }

                if (enqueueNum == 0)
                {
                    var typeNames = graphic.Units.Select(p => p.HostObject.GetType().FullName).ToArray();
                    var typeName = string.Join(",", typeNames);
                    var message =
                        $"无法决定对象保存的优先顺序,可能是因为在数据库中存在循环参照关系(如两个实体之间定义了两个关联型,且它们的映射表分别为两个实体型的映射表),请检查以下类型及其映射表:{typeName}.";
                    throw new InvalidOperationException(message);
                }
            }

            return queue;
        }

        /// <summary>
        ///     生成映射单元的主体对象为空时的异常消息
        /// </summary>
        /// <param name="unit">映射单元</param>
        /// <returns>异常消息</returns>
        private string[] GenNullHostObjectExceptionMessage(MappingUnit unit)
        {
            var mappingObjs = unit.MappingObjects;

            return (from mappingObj in mappingObjs where mappingObj != null select mappingObj.ToString()).ToArray();
        }

        #endregion

        #endregion

        #region 直接修改方法

        /// <summary>
        ///     按条件删除对象。
        ///     实施说明
        ///     在相应位置引发DirectlyChanging管道事件，其中，运用回调机制引发PreExecuteCommand和PostExecuteCommand事件。
        /// </summary>
        /// <param name="objectType">要删除的对象的类型</param>
        /// <param name="filter">对象筛选条件</param>
        public int Delete(ObjectType objectType, Expression filter)
        {
            //获取指定类型的存储标记集，进而获取存储提供程序集，最后调用（并行执行）各提供程序的Delete方法。
            //如果获取的提供程序实例超过一个，则须启用事务，须同时考虑本地事务和分布式事务。启用事务的具体方案可参考活动图“执行保存”，主流程不变，作以下两项改动：
            //（1）略去判定对象数是否大于1的步骤，不论是否大于1都执行垂直分支；
            //（2）保存新对象、保存旧对象和删除对象三步合并为“执行就地修改”。

            //获取存储提供程序
            var storageSymbols = PrepareDirectlyChangeTransaction(objectType, out var isOutTrBegun, out var transactionScope);

            try
            {
                var affectCount = 0;
                foreach (var symbol in storageSymbols)
                {
                    BeginDirectlyChanging?.Invoke(this,
                        new BeginDirectlyChangingEventArgs(filter, EDirectlyChangeType.Delete, objectType.ClrType));

                    var storageProvider = _storageProviders[symbol];

                    affectCount += storageProvider.Delete(objectType, (LambdaExpression)filter,
                        args => _directlyPreExecuteCommand?.Invoke(this, args),
                        args => _directlyPostExecuteCommand?.Invoke(this, args));

                    EndDirectlyChanging?.Invoke(this,
                        new EndDirectlyChangingEventArgs(filter, EDirectlyChangeType.Delete, objectType.ClrType,
                            affectCount));

                    if (!isOutTrBegun)
                        storageProvider.CommitTransaction();
                }

                transactionScope?.Complete();
                return affectCount;
            }
            catch (Exception ex)
            {
                EndDirectlyChanging?.Invoke(this,
                    new EndDirectlyChangingEventArgs(filter, EDirectlyChangeType.Delete, objectType.ClrType, 0, null,
                        ex));
                foreach (var symbol in storageSymbols)
                {
                    var storageProvider = _storageProviders[symbol];
                    storageProvider.RollbackTransaction();
                }

                throw;
            }
            finally
            {
                //释放资源
                foreach (var symbol in storageSymbols)
                {
                    var storageProvider = _storageProviders[symbol];
                    if (!isOutTrBegun)
                        storageProvider.ReleaseResource();
                }

                transactionScope?.Dispose();
            }
        }

        /// <summary>
        ///     为符合条件的对象的属性设置新值。
        ///     实施说明
        ///     在相应位置引发DirectlyChanging管道事件，其中，运用回调机制引发PreExecuteCommand和PostExecuteCommand事件。
        /// </summary>
        /// <param name="objectType">要设置其属性值的对象的类型。</param>
        /// <param name="filter">对象筛选条件。</param>
        /// <param name="newValues">存储属性新值的键值对集合，其中键为属性名称，值为属性的新值。</param>
        public int SetAttributes(ObjectType objectType, Expression filter, KeyValuePair<string, object>[] newValues)
        {
            //获取指定类型的存储标记集，进而获取存储提供程序集，最后调用（并行执行）各提供程序的Delete方法。
            //如果获取的提供程序实例超过一个，则须启用事务，须同时考虑本地事务和分布式事务。启用事务的具体方案可参考活动图“执行保存”，主流程不变，作以下两项改动：
            //（1）略去判定对象数是否大于1的步骤，不论是否大于1都执行垂直分支；
            //（2）保存新对象、保存旧对象和删除对象三步合并为“执行就地修改”。
            //获取存储提供程序
            var storageSymbols = PrepareDirectlyChangeTransaction(objectType, out var isOutTrBegun, out var transactionScope);

            try
            {
                var affectCount = 0;
                foreach (var symbol in storageSymbols)
                {
                    BeginDirectlyChanging?.Invoke(this,
                        new BeginDirectlyChangingEventArgs(filter, EDirectlyChangeType.Update, objectType.ClrType,
                            newValues));

                    var storageProvider = _storageProviders[symbol];

                    affectCount += storageProvider.SetAttributes(objectType, (LambdaExpression)filter, newValues,
                        args => _directlyPreExecuteCommand?.Invoke(this, args),
                        args => _directlyPostExecuteCommand?.Invoke(this, args));

                    EndDirectlyChanging?.Invoke(this,
                        new EndDirectlyChangingEventArgs(filter, EDirectlyChangeType.Update, objectType.ClrType,
                            affectCount, newValues));

                    if (!isOutTrBegun)
                        storageProvider.CommitTransaction();
                }

                transactionScope?.Complete();
                return affectCount;
            }
            catch (Exception ex)
            {
                EndDirectlyChanging?.Invoke(this,
                    new EndDirectlyChangingEventArgs(filter, EDirectlyChangeType.Update, objectType.ClrType, 0,
                        newValues, ex));
                foreach (var symbol in storageSymbols)
                {
                    var storageProvider = _storageProviders[symbol];
                    storageProvider.RollbackTransaction();
                }

                throw;
            }
            finally
            {
                //释放资源
                foreach (var symbol in storageSymbols)
                {
                    var storageProvider = _storageProviders[symbol];
                    if (!isOutTrBegun)
                        storageProvider.ReleaseResource();
                }

                transactionScope?.Dispose();
            }
        }

        /// <summary>
        ///     为符合条件的对象的属性设置新值，其中新值为原值加上增量值。属性必须为数值类型。
        ///     实施说明
        ///     在相应位置引发DirectlyChanging管道事件，其中，运用回调机制引发PreExecuteCommand和PostExecuteCommand事件。
        /// </summary>
        /// <param name="objectType">要设置其属性值的对象的类型。</param>
        /// <param name="filter">对象筛选条件。</param>
        /// <param name="increaseValues">存储增量值的键值对集合，其中键为属性名称，值为增量值。</param>
        public int IncreaseAttributes(ObjectType objectType, Expression filter,
            KeyValuePair<string, object>[] increaseValues)
        {
            //获取指定类型的存储标记集，进而获取存储提供程序集，最后调用（并行执行）各提供程序的Delete方法。
            //如果获取的提供程序实例超过一个，则须启用事务，须同时考虑本地事务和分布式事务。启用事务的具体方案可参考活动图“执行保存”，主流程不变，作以下两项改动：
            //（1）略去判定对象数是否大于1的步骤，不论是否大于1都执行垂直分支；
            //（2）保存新对象、保存旧对象和删除对象三步合并为“执行就地修改”。
            //获取存储提供程序
            var storageSymbols = PrepareDirectlyChangeTransaction(objectType, out var isOutTrBegun, out var transactionScope);

            try
            {
                var affectCount = 0;
                foreach (var symbol in storageSymbols)
                {
                    BeginDirectlyChanging?.Invoke(this,
                        new BeginDirectlyChangingEventArgs(filter, EDirectlyChangeType.Increment, objectType.ClrType,
                            increaseValues));

                    var storageProvider = _storageProviders[symbol];

                    affectCount += storageProvider.IncreaseAttributes(objectType, (LambdaExpression)filter,
                        increaseValues,
                        args => _directlyPreExecuteCommand?.Invoke(this, args),
                        args => _directlyPostExecuteCommand?.Invoke(this, args));

                    EndDirectlyChanging?.Invoke(this,
                        new EndDirectlyChangingEventArgs(filter, EDirectlyChangeType.Increment, objectType.ClrType,
                            affectCount, increaseValues));

                    if (!isOutTrBegun)
                        storageProvider.CommitTransaction();
                }

                transactionScope?.Complete();
                return affectCount;
            }
            catch (Exception ex)
            {
                EndDirectlyChanging?.Invoke(this,
                    new EndDirectlyChangingEventArgs(filter, EDirectlyChangeType.Increment, objectType.ClrType, 0,
                        increaseValues, ex));
                foreach (var symbol in storageSymbols)
                {
                    var storageProvider = _storageProviders[symbol];
                    storageProvider.RollbackTransaction();
                }

                throw;
            }
            finally
            {
                //释放资源
                foreach (var symbol in storageSymbols)
                {
                    var storageProvider = _storageProviders[symbol];
                    if (!isOutTrBegun)
                        storageProvider.ReleaseResource();
                }

                transactionScope?.Dispose();
            }
        }

        /// <summary>
        ///     准备就地修改事务
        /// </summary>
        /// <param name="objectType">对象类型</param>
        /// <param name="isOutTrBegun">是否外部已经开启了事务</param>
        /// <param name="transactionScope">事务块</param>
        /// <returns></returns>
        private StorageSymbol[] PrepareDirectlyChangeTransaction(ObjectType objectType, out bool isOutTrBegun,
            out TransactionScope transactionScope)
        {
            GenerateSymbolByObjectType(objectType);
            var storageSymbols = _storageSymbolJudge.Judge(objectType);

            //是否在我开启事务前已经开启了事务
            isOutTrBegun = _storageProviders.Values.Any(p => p.TransactionBegun);

            //当前的环境事务
            transactionScope = null;
            //开启事务
            BeginTransaction(ref transactionScope);
            return storageSymbols;
        }

        #endregion
    }
}