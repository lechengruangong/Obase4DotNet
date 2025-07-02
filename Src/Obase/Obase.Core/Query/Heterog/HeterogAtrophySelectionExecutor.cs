/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：异构投影（退化）运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 12:24:55
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.Heterog
{
    /// <summary>
    ///     异构投影（退化）运算执行器。
    ///     实施说明
    ///     执行算法参见活动图“执行异构退化投影运算”,（该图从整体上说明了算法的逻辑，实施时应按照模板模式的要求在基类和派生类之间分配行为）。
    /// </summary>
    public class HeterogAtrophySelectionExecutor : StandardHeterogOpExecutor
    {
        /// <summary>
        ///     附加路径
        /// </summary>
        private AtrophyPath _attachingPath;

        /// <summary>
        ///     附加引用
        /// </summary>
        private ReferenceElement _attachiungReferenceElement;

        /// <summary>
        ///     初始化HeterogOpExecutor类的新实例。
        /// </summary>
        /// <param name="storageProviderCreator">创建存储提供程序的委托。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="preexecutionCallback">执行前回调委托。</param>
        /// <param name="postexecutionCallback">执行后回调委托。</param>
        /// <param name="baseQueryProvider">基础查询提供器</param>
        public HeterogAtrophySelectionExecutor(Func<StorageSymbol, IStorageProvider> storageProviderCreator,
            ObjectDataModel model, Action<QueryEventArgs> preexecutionCallback,
            Action<QueryEventArgs> postexecutionCallback, IBaseQueryProvider baseQueryProvider = null) : base(
            storageProviderCreator, model,
            preexecutionCallback, postexecutionCallback, baseQueryProvider)
        {
        }

        /// <summary>
        ///     由派生类实现，为指定的异构运算生成基础查询。
        /// </summary>
        /// <param name="heterogOp">异构运算。</param>
        /// <param name="heterogQuery">以异构运算作为末节点的异构查询。</param>
        /// <param name="attachingRefs">返回对异构运算执行极限分解形成的附加引用</param>
        protected override QueryOp GenerateBaseOp(QueryOp heterogOp, QueryOp heterogQuery,
            out ReferenceElement[] attachingRefs)
        {
            if (heterogOp is SelectOp selectOp)
            {
                //极限分解退化路径
                var basePath =
                    selectOp.AtrophyPath.DecomposeExtremely(out _attachingPath, out var attachingNode,
                        out _attachiungReferenceElement);
                QueryOp newTail = null;
                if (attachingNode.Parent != null) newTail = QueryOp.Select(basePath, false, Model);

                attachingRefs = new[] { _attachiungReferenceElement };
                return heterogQuery.Replace(heterogOp, newTail);
            }

            throw new ArgumentException("异构投影（退化）运算执行器只能处理SelectOp", nameof(heterogOp));
        }

        /// <summary>
        ///     由派生类实现，为指定的异构运算生成附加查询。
        /// </summary>
        /// <param name="baseResult">基础查询的结果。</param>
        /// <param name="heterogOp">异构运算。</param>
        /// <param name="attachingRefs">返回生成的附加查询对应的附加引用，与方法返回值集合中的元素一一对应</param>
        protected override QueryOp[] GenerateAttachingQuery(object baseResult, QueryOp heterogOp,
            out ReferenceElement[] attachingRefs)
        {
            var attachingOp = QueryOp.Select(_attachingPath, false, Model, heterogOp.Next);
            var sourceObjs = baseResult as object[] ?? new[] { baseResult };
            var attachingQuery = _attachiungReferenceElement.GenerateLoadingQuery(sourceObjs, attachingOp);

            attachingRefs = new[] { _attachiungReferenceElement };
            return new[] { attachingQuery };
        }

        /// <summary>
        ///     由派生类实现，合并基础查询与附加查询的结果。
        /// </summary>
        /// <param name="baseResult">基础查询结果。</param>
        /// <param name="attachingResults">
        ///     各附加查询的结果，其顺序与GenerateAttachingQuery方法返回的附加查询的顺序一致。
        /// </param>
        /// <param name="attachObject">附加委托</param>
        /// <param name="attachRoot">是否附加根对象</param>
        protected override object Combine(object baseResult, object[] attachingResults, AttachObject attachObject,
            bool attachRoot = true)
        {
            foreach (var attaching in attachingResults)
                if (Model.GetObjectType(attaching.GetType()) != null && attachRoot)
                {
                    var local = attaching;
                    attachObject.Invoke(ref local, true);
                }

            return ProcessResult(attachingResults, _attachingPath.AssociationPath.RepresentedType.ClrType);
        }

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
        protected override AssociationTree CutIncluding(AssociationTree includingTree, QueryOp basicOp)
        {
            if (basicOp is SelectOp selectOp) return includingTree?.Select(selectOp.AtrophyPath.AssociationPath);

            throw new ArgumentException("异构投影（退化）运算执行器只能处理SelectOp", nameof(basicOp));
        }

        /// <summary>
        ///     处理最终结果
        /// </summary>
        /// <param name="result">结果集</param>
        /// <param name="clrType">结果的运行时类型</param>
        /// <returns></returns>
        private object ProcessResult(object[] result, Type clrType)
        {
            if (result == null)
                return null;
            //实际上是个List
            var realResult = Activator.CreateInstance(typeof(List<>).MakeGenericType(clrType));
            foreach (var r in result)
            {
                var addMethod = realResult.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public);
                addMethod?.Invoke(realResult, new[] { r });
            }

            return realResult;
        }
    }
}