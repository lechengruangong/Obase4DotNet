/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：异构查询提供程序.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 12:07:31
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;
using Obase.Core.Odm.ObjectSys;

namespace Obase.Core.Query.Heterog
{
    /// <summary>
    ///     异构查询提供程序。
    /// </summary>
    public class HeterogQueryProvider : QueryProvider
    {
        /// <summary>
        ///     基础查询提供程序。
        /// </summary>
        private readonly IBaseQueryProvider _baseQueryProvider;

        /// <summary>
        ///     异构查询分解器。
        /// </summary>
        private readonly HeterogQueryDecomposer _decomposer;

        /// <summary>
        ///     片段执行器
        /// </summary>
        private readonly IHeterogQuerySegmentallyExecutor _segmentallyExecutor;

        /// <summary>
        ///     一个委托，用于构造存储提供程序。
        /// </summary>
        private readonly Func<StorageSymbol, IStorageProvider> _storageProviderCreator;

        /// <summary>
        ///     获取或设置一个值，该值指示是否附加根对象
        /// </summary>
        private bool _attachRoot = true;

        /// <summary>
        ///     初始化HeterogQueryProvider类的新实例。
        /// </summary>
        /// <param name="storageProviderCreator">一个委托，用于构造存储提供程序。</param>
        /// <param name="model">对象数据模型。</param>
        /// <param name="attachObject"></param>
        /// <param name="context"></param>
        /// <param name="heterogeneityPredicationProvider"></param>
        /// <param name="baseQueryProvider"></param>
        /// <param name="segmentallyExecutor"></param>
        public HeterogQueryProvider(Func<StorageSymbol, IStorageProvider> storageProviderCreator, ObjectDataModel model,
            AttachObject attachObject,
            ObjectContext context,
            HeterogeneityPredicationProvider heterogeneityPredicationProvider = null,
            IBaseQueryProvider baseQueryProvider = null,
            IHeterogQuerySegmentallyExecutor segmentallyExecutor = null)
            : base(model, attachObject, context)
        {
            var provider = heterogeneityPredicationProvider ?? new StorageHeterogeneityPredicationProvider();
            _segmentallyExecutor = segmentallyExecutor ?? new HeterogQuerySegmentallyExecutor();
            _decomposer = new HeterogQueryDecomposer(provider);
            _storageProviderCreator = storageProviderCreator;
            _baseQueryProvider = baseQueryProvider ?? new BaseQueryProvider(storageProviderCreator, model);
        }

        /// <summary>
        ///     获取或设置一个值，该值指示是否附加根对象
        /// </summary>
        public bool AttachRoot
        {
            get => _attachRoot;
            set => _attachRoot = value;
        }

        /// <summary>
        ///     一个委托，用于构造存储提供程序。
        /// </summary>
        public Func<StorageSymbol, IStorageProvider> StorageProviderCreator => _storageProviderCreator;

        /// <summary>
        ///     基础查询提供程序。
        /// </summary>
        public IBaseQueryProvider BaseProvider => _baseQueryProvider;

        /// <summary>
        ///     执行查询。
        /// </summary>
        /// <returns>执行查询的结果。</returns>
        /// <param name="query">要执行的查询。值为null表示取出查询源中的所有对象。</param>
        /// <param name="including"></param>
        /// <param name="context">查询上下文。</param>
        protected override void Execute(AssociationTree including, QueryContext context, QueryOp query = null)
        {
            if (query != null)
            {
                //分解查询
                var decomposeResult = query.Accept(_decomposer, including);
                //使用片段执行器执行分解后的查询
                var result = _segmentallyExecutor.Execute(decomposeResult, this, AttachObject, _attachRoot);
                context.Result = result;
            }
        }
    }
}