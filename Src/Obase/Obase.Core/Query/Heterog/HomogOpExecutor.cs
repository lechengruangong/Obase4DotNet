/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：同构运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 12:21:52
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.Heterog
{
    /// <summary>
    ///     将同构运算视为特殊的异构运算，定义特殊算法。
    ///     实施说明
    ///     将该查询本身作为基础查询，且无附加查询；基础查询的结果即为最终结果，即没有合并操作。
    /// </summary>
    public sealed class HomogOpExecutor : StandardHeterogOpExecutor
    {
        /// <summary>
        ///     初始化HeterogOpExecutor类的新实例。
        /// </summary>
        /// <param name="storageProviderCreator">创建存储提供程序的委托。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="preexecutionCallback">执行前回调委托。</param>
        /// <param name="postexecutionCallback">执行后回调委托。</param>
        /// <param name="baseQueryProvider">基础查询提供程序</param>
        public HomogOpExecutor(Func<StorageSymbol, IStorageProvider> storageProviderCreator, ObjectDataModel model,
            Action<QueryEventArgs> preexecutionCallback,
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
            //本身作为基础查询
            attachingRefs = new ReferenceElement[] { };
            return heterogQuery ?? heterogOp;
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
            //无附加查询
            attachingRefs = new ReferenceElement[] { };
            return Array.Empty<QueryOp>();
        }

        /// <summary>
        ///     由派生类实现，合并基础查询与附加查询的结果。
        /// </summary>
        /// <param name="baseResult">基础查询结果。</param>
        /// <param name="attachingResults">
        ///     各附加查询的结果，其顺序与GenerateAttachingQuery方法返回的附加查询的顺序一致。
        /// </param>
        /// <param name="attachObject"></param>
        /// <param name="attachRoot"></param>
        protected override object Combine(object baseResult, object[] attachingResults, AttachObject attachObject,
            bool attachRoot = true)
        {
            foreach (var attaching in attachingResults)
                if (Model.GetObjectType(attaching.GetType()) != null && attachRoot)
                {
                    var local = attaching;
                    attachObject.Invoke(ref local, true);
                }

            //没有合并操作
            return baseResult;
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
            //提取隐含投影视图或投影链
            var including = basicOp?.ImpliedIncluding ?? basicOp?.GetChainIncluding();
            if (including != null)
            {
                var result = includingTree?.Select(including.Node);
                if (result != null)
                    return result;
            }

            return includingTree;
        }

        /// <summary>
        ///     设置异构查询提供程序
        /// </summary>
        /// <param name="provider"></param>
        internal void SetHeterogQueryProvider(HeterogQueryProvider provider)
        {
            HeterogQueryProvider = provider;
        }
    }
}