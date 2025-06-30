/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：默认的基础查询提供程序.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 12:00:28
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using Obase.Core.MappingPipeline;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;
using Obase.Core.Query.Oop;

namespace Obase.Core.Query.Heterog
{
    /// <summary>
    ///     默认的基础查询提供程序
    /// </summary>
    public class BaseQueryProvider : IBaseQueryProvider
    {
        /// <summary>
        ///     对象数据模型。
        /// </summary>
        private readonly ObjectDataModel _model;

        /// <summary>
        ///     用于构造存储提供程序的委托。
        /// </summary>
        private readonly Func<StorageSymbol, IStorageProvider> _storageProviderCreator;

        /// <summary>
        ///     默认的基础查询提供程序
        /// </summary>
        /// <param name="storageProviderCreator">存储提供程序构建委托。</param>
        /// <param name="model">对象数据模型。</param>
        public BaseQueryProvider(Func<StorageSymbol, IStorageProvider> storageProviderCreator, ObjectDataModel model)
        {
            _model = model;
            _storageProviderCreator = storageProviderCreator;
        }

        /// <summary>
        ///     调用存储服务。
        /// </summary>
        /// <param name="executionState">一个状态对象，携带查询执行流程中生成的数据。</param>
        /// <param name="including">指定由运算管道加载的对象须包含的引用，必须是同构的。</param>
        /// <param name="postexecutionCallback">执行命令后委托</param>
        /// <param name="attachObject">用于将对象附加到对象上下文的委托。</param>
        /// <param name="preexecutionCallback">执行命令前委托</param>
        public object CallService(object executionState, AssociationTree including,
            Action<QueryEventArgs> preexecutionCallback,
            Action<QueryEventArgs> postexecutionCallback, AttachObject attachObject)
        {
            if (executionState is QueryExecutionState queryExecutionState)
            {
                var provider = queryExecutionState.StorageProvider;
                var pipeline = queryExecutionState.Pipeline;
                //调用存储服务的 ExecutePipeline 方法执行运算管道
                return provider.ExecutePipeline(pipeline, including, preexecutionCallback, postexecutionCallback,
                    attachObject, false);
            }

            return null;
        }

        /// <summary>
        ///     执行补充运算。
        /// </summary>
        /// <param name="complement">要执行的补充查询。</param>
        /// <param name="serviceResult">存储服务输出的结果。</param>
        /// <param name="executionState">一个状态对象，携带查询执行流程中生成的数据。</param>
        public object ExecuteComplement(QueryOp complement, object serviceResult, object executionState)
        {
            if (executionState is QueryExecutionState queryExecutionState)
            {
                var builder = queryExecutionState.ComplementBuilder;
                var oopPipeline = complement.GeneratePipeline(builder);

                //执行补充运算
                if (serviceResult is IEnumerable instances) return oopPipeline.Execute(instances);

                return oopPipeline.Execute(serviceResult);
            }

            return null;
        }

        /// <summary>
        ///     从基础查询中分离出补充查询。
        ///     补充运算是特定的存储服务无法执行，须以对象运算方式补充执行的片段。
        /// </summary>
        /// <param name="baseQuery">要执行的基础查询。</param>
        /// <param name="executionState">一个状态对象，携带查询执行流程中生成的数据。</param>
        public QueryOp SeperateOutComplement(QueryOp baseQuery, out object executionState)
        {
            //获取基点存储标记
            var objectType = _model.GetObjectType(baseQuery.SourceType);
            StorageSymbol storageSymbol;
            if (objectType != null)
            {
                var typeExtension = objectType.GetExtension(typeof(HeterogStorageExtension));
                storageSymbol = (typeExtension == null
                    ? _model.StorageSymbol
                    : ((HeterogStorageExtension)typeExtension).StorageSymbol) ?? StorageSymbols.Current.Default;
            }
            else
            {
                storageSymbol = _model.StorageSymbol ?? StorageSymbols.Current.Default;
            }

            //创建存储提供程序
            var provider = _storageProviderCreator(storageSymbol);
            //创建对应的运算管道
            var pipeline = provider.GeneratePipeline(baseQuery, out var complement, out var complementBuilder);
            //存储在状态对象中
            executionState = new QueryExecutionState
            {
                ComplementBuilder = complementBuilder,
                Pipeline = pipeline,
                StorageProvider = provider
            };
            return complement;
        }

        /// <summary>
        ///     一个数据结构，用于存储查询执行流程中生成的数据
        /// </summary>
        private struct QueryExecutionState
        {
            /// <summary>
            ///     用于生成补充对象运算管道的生成器。如果不指定，将使用默认生成器（OopPipelineBuilder）。
            /// </summary>
            public OopPipelineBuilder ComplementBuilder;

            /// <summary>
            ///     基础查询链可由存储服务执行部分生成的运算管道。
            /// </summary>
            public OpExecutor Pipeline;

            /// <summary>
            ///     存储提供程序。
            /// </summary>
            public IStorageProvider StorageProvider;
        }
    }
}