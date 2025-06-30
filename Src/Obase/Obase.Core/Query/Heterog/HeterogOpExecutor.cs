/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：异构运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 12:00:28
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.Heterog
{
    /// <summary>
    ///     异构运算执行器。
    /// </summary>
    public abstract class HeterogOpExecutor
    {
        /// <summary>
        ///     对象数据模型。
        /// </summary>
        protected readonly ObjectDataModel Model;

        /// <summary>
        ///     执行后回调委托。
        /// </summary>
        protected readonly Action<QueryEventArgs> PostexecutionCallback;

        /// <summary>
        ///     执行前回调委托。
        /// </summary>
        protected readonly Action<QueryEventArgs> PreexecutionCallback;

        /// <summary>
        ///     用于构造存储提供程序的委托。
        /// </summary>
        protected readonly Func<StorageSymbol, IStorageProvider> StorageProviderCreator;

        /// <summary>
        ///     异构查询提供程序器。
        /// </summary>
        protected HeterogQueryProvider HeterogQueryProvider;

        /// <summary>
        ///     初始化HeterogOpExecutor类的新实例。
        /// </summary>
        /// <param name="storageProviderCreator">创建存储提供程序的委托。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="preexecutionCallback">执行前回调委托。</param>
        /// <param name="postexecutionCallback">执行后回调委托。</param>
        protected HeterogOpExecutor(Func<StorageSymbol, IStorageProvider> storageProviderCreator, ObjectDataModel model,
            Action<QueryEventArgs> preexecutionCallback,
            Action<QueryEventArgs> postexecutionCallback)
        {
            StorageProviderCreator = storageProviderCreator;
            Model = model;
            PreexecutionCallback = preexecutionCallback;
            PostexecutionCallback = postexecutionCallback;
        }

        /// <summary>
        ///     执行异构运算。
        /// </summary>
        /// <param name="heterogOp">要执行的异构运算。</param>
        /// <param name="heterogQuery">要执行的异构运算所在的查询链，它是该查询链的末节点。</param>
        /// <param name="including">包含树。</param>
        /// <param name="attachObject">附加委托</param>
        /// <param name="attachRoot">是否附加根</param>
        public abstract object Execute(QueryOp heterogOp, QueryOp heterogQuery, AssociationTree including,
            AttachObject attachObject, bool attachRoot = true);

        /// <summary>
        ///     为指定的异构运算创建执行器。
        /// </summary>
        /// <returns>
        ///     异构运算执行器。
        ///     实施说明
        ///     检查要执行的运算是否为异构的，如果不是则返回HomogOpExecutor；否则返回特定于运算的执行器。
        /// </returns>
        /// <param name="heterogOp">要执行的异构运算。</param>
        /// <param name="storageProviderCreator">创建存储提供程序的委托。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="preexecutionCallback">执行前回调委托。</param>
        /// <param name="postexecutionCallback">执行后回调委托。</param>
        /// <param name="heterogQueryProvider">异构运算执行器的创建者，它是一个异构查询执行器。</param>
        /// <param name="baseQueryProvider">基础查询提供程序</param>
        public static HeterogOpExecutor Create(QueryOp heterogOp,
            Func<StorageSymbol, IStorageProvider> storageProviderCreator, ObjectDataModel model,
            Action<QueryEventArgs> preexecutionCallback,
            Action<QueryEventArgs> postexecutionCallback, HeterogQueryProvider heterogQueryProvider,
            IBaseQueryProvider baseQueryProvider = null)
        {
            var tail = heterogOp.Tail;
            //是否为异构的
            if (!tail.Heterogeneous())
                //不是异构的 返回同构执行器
                return new HomogOpExecutor(storageProviderCreator, model, preexecutionCallback,
                    postexecutionCallback, baseQueryProvider)
                {
                    HeterogQueryProvider = heterogQueryProvider
                };

            //其他执行器
            switch (tail.Name)
            {
                case EQueryOpName.Select:
                {
                    if (!(tail is SelectOp selectOp)) throw new ArgumentException(@"创建异构执行器失败:操作对应的类型不符");
                    if (selectOp.IsNew)
                        return new HeterogSelectionExecutor(storageProviderCreator, model,
                            preexecutionCallback,
                            postexecutionCallback, baseQueryProvider)
                        {
                            HeterogQueryProvider = heterogQueryProvider
                        };
                    return new HeterogAtrophySelectionExecutor(storageProviderCreator, model,
                        preexecutionCallback,
                        postexecutionCallback, baseQueryProvider)
                    {
                        HeterogQueryProvider = heterogQueryProvider
                    };
                }
                case EQueryOpName.Where:
                    return new HeterogWhereExecutor(storageProviderCreator, model, preexecutionCallback,
                        postexecutionCallback, baseQueryProvider)
                    {
                        HeterogQueryProvider = heterogQueryProvider
                    };
                case EQueryOpName.Group:
                    return new HeterogGroupExecutor(storageProviderCreator, model, preexecutionCallback,
                        postexecutionCallback)
                    {
                        HeterogQueryProvider = heterogQueryProvider
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(tail), $"创建异构执行器失败:{tail.Name}无对应的异构运算执行器.");
            }
        }
    }
}