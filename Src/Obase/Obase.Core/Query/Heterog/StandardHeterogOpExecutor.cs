/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：为一些异构运算执行器定义标准化模板.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 12:18:02
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.Heterog
{
    /// <summary>
    ///     为一些异构运算执行器定义标准化模板。
    /// </summary>
    public abstract class StandardHeterogOpExecutor : HeterogOpExecutor
    {
        /// <summary>
        ///     基础查询提供程序
        /// </summary>
        private readonly IBaseQueryProvider _baseQueryProvider;

        /// <summary>
        ///     初始化HeterogOpExecutor类的新实例。
        /// </summary>
        /// <param name="storageProviderCreator">创建存储提供程序的委托。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="preexecutionCallback">执行前回调委托。</param>
        /// <param name="postexecutionCallback">执行后回调委托。</param>
        /// <param name="baseQueryProvider">基础查询提供程序</param>
        protected StandardHeterogOpExecutor(Func<StorageSymbol, IStorageProvider> storageProviderCreator,
            ObjectDataModel model, Action<QueryEventArgs> preexecutionCallback,
            Action<QueryEventArgs> postexecutionCallback, IBaseQueryProvider baseQueryProvider = null) : base(
            storageProviderCreator, model,
            preexecutionCallback, postexecutionCallback)
        {
            _baseQueryProvider = baseQueryProvider ?? new BaseQueryProvider(storageProviderCreator, model);
        }

        /// <summary>
        ///     执行异构运算。
        /// </summary>
        /// <param name="heterogOp">要执行的异构运算。</param>
        /// <param name="heterogQuery">要执行的异构运算所在的查询链，它是该查询链的末节点。</param>
        /// <param name="including">包含树。</param>
        /// <param name="attachObject">用于在对象上下文中附加对象的委托</param>
        /// <param name="attachRoot">指示是否附加根对象</param>
        public override object Execute(QueryOp heterogOp, QueryOp heterogQuery, AssociationTree including,
            AttachObject attachObject, bool attachRoot = true)
        {
            //生成基础查询
            var baseOp = GenerateBaseOp(heterogOp, heterogQuery, out var baseOpAttachingRefs);
            var complement = _baseQueryProvider.SeperateOutComplement(baseOp, out var executionState);
            //依据基础运算裁剪包含树
            var callerIncluding = CutIncluding(including, baseOp.Tail);
            //合并包含树并实施极限分解
            var baseIncluding = DecomposeIncluding(complement, callerIncluding, out var attachingItems);
            var baseInstances = _baseQueryProvider.CallService(executionState, baseIncluding, PreexecutionCallback,
                PostexecutionCallback, attachObject);
            var filteredAttachingRefList = new List<AssociationTreeAttachingItem>();
            //过滤attachingItems
            foreach (var item in attachingItems ?? Array.Empty<AssociationTreeAttachingItem>())
            {
                //每个都不一样 则为不包含在baseOpAttachingRefs里
                var isInBaseOpAttachingRefs = baseOpAttachingRefs?.All(p => p != item.AttachingReference);
                if (isInBaseOpAttachingRefs.HasValue && isInBaseOpAttachingRefs.Value)
                    filteredAttachingRefList.Add(item);
            }

            var filteredAttachingRefs = filteredAttachingRefList.ToArray();
            //执行非重叠附加包含运算
            ExecuteAttachingIncluding(new[] { baseInstances }, filteredAttachingRefs);

            //执行补充运算
            if (complement != null)
                baseInstances = _baseQueryProvider.ExecuteComplement(complement, baseInstances, executionState);

            //不是同构查询 此处需要将Base查询查出
            if (GetType() != typeof(HomogOpExecutor)) baseInstances = ProcessInstances(baseInstances);

            //生成附加查询
            var attachingQueries = GenerateAttachingQuery(baseInstances, heterogOp, out var attachingRefs);
            HeterogQueryProvider.AttachRoot = false;

            var attachingResult = new List<object>();
            if (attachingItems != null)
                //处理附加查询
                foreach (var attachingQuery in attachingQueries)
                foreach (var attachingRef in attachingRefs)
                {
                    //获取重叠包含
                    var include = attachingItems.FirstOrDefault(p => p.AttachingReference == attachingRef)
                        ?.AttachingTree;
                    if (include != null)
                        //执行附加查询
                        attachingResult.AddRange(
                            ProcessInstances(HeterogQueryProvider.Execute(attachingQuery, include)));
                }
            else
                //处理附加查询
                foreach (var attachingQuery in attachingQueries)
                    attachingResult.AddRange(ProcessInstances(HeterogQueryProvider.Execute(attachingQuery)));


            //合并
            var attachResult = Combine(baseInstances, attachingResult.ToArray(), attachObject, attachRoot);
            return attachResult;
        }

        /// <summary>
        ///     处理某个查询结果
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        private object[] ProcessInstances(object instances)
        {
            //将对象查出
            if (instances is IEnumerable iEnumerable)
            {
                var tempResult = new List<object>();
                var enumerator = iEnumerable.GetEnumerator();
                while (enumerator.MoveNext()) tempResult.Add(enumerator.Current);
                if (enumerator is IDisposable disposable) disposable.Dispose();

                instances = tempResult.Count == 0 ? null : tempResult.ToArray();
            }

            if (instances == null)
                return Array.Empty<object>();

            return instances as object[] ?? new[] { instances };
        }

        /// <summary>
        ///     由派生类实现，为指定的异构运算生成基础查询。
        /// </summary>
        /// <param name="heterogOp">异构运算。</param>
        /// <param name="heterogQuery">以异构运算作为末节点的异构查询。</param>
        /// <param name="attachingRefs">返回对异构运算执行极限分解形成的附加引用</param>
        protected abstract QueryOp GenerateBaseOp(QueryOp heterogOp, QueryOp heterogQuery,
            out ReferenceElement[] attachingRefs);

        /// <summary>
        ///     由派生类实现，为指定的异构运算生成附加查询。
        /// </summary>
        /// <param name="baseResult">基础查询的结果。</param>
        /// <param name="heterogOp">异构运算。</param>
        /// <param name="attachingRefs">返回生成的附加查询对应的附加引用，与方法返回值集合中的元素一一对应</param>
        protected abstract QueryOp[] GenerateAttachingQuery(object baseResult, QueryOp heterogOp,
            out ReferenceElement[] attachingRefs);

        /// <summary>
        ///     由派生类实现，合并基础查询与附加查询的结果。
        /// </summary>
        /// <param name="baseResult">基础查询结果。</param>
        /// <param
        ///     name="attachingResults">
        ///     各附加查询的结果，其顺序与GenerateAttachingQuery方法返回的附加查询的顺序一致。
        /// </param>
        /// <param name="attachObject"></param>
        /// <param name="attachRoot"></param>
        protected abstract object Combine(object baseResult, object[] attachingResults, AttachObject attachObject,
            bool attachRoot = true);

        /// <summary>
        ///     根据基础运算对包含树T进行裁剪。
        ///     给实施者的说明
        ///     实施者作为特定运算的异构执行器，应当根据具体的运算编写相应的裁剪逻辑：
        ///     （1）如果是投影运算，根据投影运算的不同状态选择AssociationTree.Select方法的相应版本实施裁剪；
        ///     （2）如果是其它运算，提取隐含投影视图或投影链，参照投影运算进行裁剪，具体参见HeterogQueryExecutor类的注释。
        /// </summary>
        /// <returns>裁剪后的包含树。</returns>
        /// <param name="includingTree">待裁剪的包含树。</param>
        /// <param name="basicOp">作为裁剪依据的基础运算。</param>
        protected abstract AssociationTree CutIncluding(AssociationTree includingTree, QueryOp basicOp);

        /// <summary>
        ///     将补充查询中的包含运算（包括隐含包含）与调用运算执行器时传入的包含树合并，然后执行极限分解。
        /// </summary>
        /// <returns>返回分解后的基础树。</returns>
        /// <param name="complementQuery">补充查询。</param>
        /// <param name="callerIncluding">调用方传入的包含树。</param>
        /// <param name="attachingItems">返回分解出的代表附加包含树的项。</param>
        private AssociationTree DecomposeIncluding(QueryOp complementQuery, AssociationTree callerIncluding,
            out AssociationTreeAttachingItem[] attachingItems)
        {
            //补充查询的包含树
            AssociationTree complementIncluding = null;
            if (complementQuery != null)
            {
                complementIncluding = complementQuery.GetChainIncluding();
                if (complementIncluding == null && callerIncluding != null)
                    complementIncluding = new AssociationTree(Model.GetReferringType(complementQuery.SourceType));
            }

            //增加补充链
            if (complementIncluding != null)
            {
                var collector = new AtrophyCollector();
                var paths = complementQuery.Accept(collector);
                //合并包含树
                complementIncluding.Grow(callerIncluding, paths);
            }

            //包含树
            var including = complementIncluding;

            if (including != null)
            {
                //执行强制包含
                var enforcer = new IncludingEnforcer();
                including.Accept(enforcer);

                //进行分解
                var decomposer = new AssociationTreeDecomposer(new StorageHeterogeneityPredicationProvider());
                decomposer.SetArgument(false);
                including.Accept(decomposer);

                if (decomposer.Result != null && decomposer.Result.SubCount > 0)
                {
                    attachingItems = decomposer.OutArgument;
                    return decomposer.Result;
                }
            }

            attachingItems = null;
            return callerIncluding;
        }

        /// <summary>
        ///     执行附加包含。
        /// </summary>
        /// <param name="sourceObjs">作为查询源的对象集。</param>
        /// <param name="attachingItems">表示要执行的附加包含的附加项。</param>
        private void ExecuteAttachingIncluding(object[] sourceObjs, AssociationTreeAttachingItem[] attachingItems)
        {
            //构造获取器
            var targetGetter = new IncludingTargetGetter(sourceObjs);
            if (attachingItems != null && attachingItems.Length > 0)
                foreach (var attachingItem in attachingItems)
                {
                    var includingTargets = attachingItem.AttachingNode.AsTree().Accept(targetGetter);
                    var includeOp = new IncludeOp(attachingItem.AttachingTree, Model);
                    var attchingQuery =
                        attachingItem.AttachingReference.GenerateLoadingQuery(includingTargets, includeOp);
                    var obj = HeterogQueryProvider.Execute(attchingQuery);
                    var attchingObjs = obj as object[] ?? new[] { obj };

                    foreach (var target in includingTargets)
                    {
                        var refValue = attachingItem.AttachingReference.FilterTarget(ref attchingObjs, target);
                        attachingItem.AttachingReference.SetValue(target, refValue);
                    }
                }
        }

        /// <summary>
        ///     作为关联树向上访问者，获取包含目标对象。
        /// </summary>
        private class IncludingTargetGetter : IAssociationTreeUpwardVisitor<object[]>
        {
            /// <summary>
            ///     源对象。
            /// </summary>
            private readonly object[] _sourceObjects;

            /// <summary>
            ///     结果
            /// </summary>
            private object[] _result = Array.Empty<object>();

            /// <summary>
            ///     初始化IncludingTargetGetter类的新实例。
            /// </summary>
            /// <param name="sourceObjs">源对象。</param>
            public IncludingTargetGetter(object[] sourceObjs)
            {
                _sourceObjects = sourceObjs;
            }

            /// <summary>
            ///     获取遍历关联树的结果。
            /// </summary>
            public object[] Result => _result;

            /// <summary>
            ///     前置访问，即在访问父级前执行操作。
            /// </summary>
            /// <param name="subTree">被访问的子树。</param>
            /// <param name="childState">访问子级时产生的状态数据。</param>
            /// <param name="outChildState">返回一个状态数据，在遍历到父级时该数据将被视为子级状态。</param>
            /// <param name="outPrevisitState">返回一个状态数据，在执行后置访问时该数据将被视为前置访问状态。</param>
            public bool Previsit(AssociationTree subTree, object childState, out object outChildState,
                out object outPrevisitState)
            {
                //Nothing to do
                outChildState = null;
                outPrevisitState = null;
                return false;
            }

            /// <summary>
            ///     后置访问，即在访问父级后执行操作。
            /// </summary>
            /// <param name="subTree">被访问的子树。</param>
            /// <param name="childState">访问子级时产生的状态数据。</param>
            /// <param name="previsitState">前置访问产生的状态数据。</param>
            public void Postvisit(AssociationTree subTree, object childState, object previsitState)
            {
                var parent = subTree.Parent;
                if (parent == null)
                {
                    _result = _sourceObjects;
                }
                else
                {
                    var referenceElement = parent.RepresentedType.GetReferenceElement(subTree.ElementName);
                    //取每个值
                    var result = _result.Select(obj => referenceElement.GetValue(obj)).ToList();
                    _result = result.ToArray();
                }
            }

            /// <summary>
            ///     重置访问者。
            /// </summary>
            public void Reset()
            {
                //Nothing to do
            }
        }
    }
}