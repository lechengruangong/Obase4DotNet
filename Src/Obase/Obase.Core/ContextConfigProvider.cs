/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：结构化表示的连接字符串.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:48:21
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Obase.Core.Odm;
using Obase.Core.Odm.Builder;
using Obase.Core.Query;
using Obase.Core.Query.Heterog;
using Obase.Core.Saving;

namespace Obase.Core
{
    /// <summary>
    ///     特定于功能的上下文配置提供程序。
    /// </summary>
    public abstract class ContextConfigProvider
    {
        /// <summary>
        ///     提供程序字典
        /// </summary>
        private readonly Dictionary<StorageSymbol, IStorageProvider> _storageProviders =
            new Dictionary<StorageSymbol, IStorageProvider>();

        /// <summary>
        ///     查询提供程序
        /// </summary>
        private QueryProvider _queryProvider;

        /// <summary>
        ///     保存提供程序
        /// </summary>
        private SavingProvider _savingProvider;

        /// <summary>
        ///     对象数据模型
        /// </summary>
        protected internal ObjectDataModel Model;

        /// <summary>
        ///     所属于的上下文
        /// </summary>
        public ObjectContext ObjectContext { get; set; }

        /// <summary>
        ///     获取保存提供程序。
        ///     实施说明
        ///     使用GetStorageProvider方法作为目标方法构造一个委托，作为storageProviderCreator参数的实参。
        /// </summary>
        internal SavingProvider SavingProvider =>
            _savingProvider ?? (_savingProvider = new SavingProvider(CreateModel(), GetStorageProvider));

        /// <summary>
        ///     获取查询提供程序。
        ///     实施说明
        ///     实例化HeterogeneousQueryProvider。使用GetStorageProvider方法作为目标方法构造一个委托，作为storageProvid
        ///     erCreator参数的实参。
        /// </summary>
        protected internal QueryProvider QueryProvider => _queryProvider ??
                                                          (_queryProvider = new HeterogQueryProvider(GetStorageProvider,
                                                              CreateModel(),
                                                              (ref object o, bool root) =>
                                                                  ObjectContext.Attach(ref o, false, root),
                                                              ObjectContext));

        /// <summary>
        ///     获取一个值，该值指示是否自动创建对象集。
        /// </summary>
        protected internal virtual bool WhetherCreateSet => true;

        /// <summary>
        ///     提供程序字典
        /// </summary>
        internal Dictionary<StorageSymbol, IStorageProvider> StorageProviders => _storageProviders;


        /// <summary>
        ///     创建对象数据模型。
        /// </summary>
        /// <returns>
        ///     创建的对象数据模型。
        ///     实施说明
        ///     如果关联引用_model已初始化，直接返回其值。
        ///     否则，实例化一个ModelBuilder，然后调用CreateModel(ModelBuilder)，最后调用ModelBuilder.
        ///     Build方法建造模型，并以建造的模型初始化关联引用_model。
        /// </returns>
        internal ObjectDataModel CreateModel()
        {
            //如果关联引用_model已初始化，直接返回其值。
            // 否则，实例化一个ModelBuilder，然后调用CreateModel(ModelBuilder)，最后调用ModelBuilder.
            // Build方法建造模型，并以建造的模型初始化关联引用_model。

            if (ObjectContext?.Model != null) Model = ObjectContext.Model;

            if (Model != null)
                return Model;
            var modelBuilder = new ModelBuilder(ObjectContext);
            //查找指定的扩展构件
            var extList = new List<string>
                { "Obase.Odm.Annotation.dll", "Obase.LogicDeletion.dll", "Obase.MultiTenant.dll" };
            var assemblies = extList.SelectMany(p => Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, p))
                .Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x))).ToList();
            //调用IAddonRegister进行注册
            foreach (var assembly in assemblies)
            foreach (var assemblyType in assembly.GetExportedTypes())
                if (typeof(IAddonRegister).IsAssignableFrom(assemblyType))
                {
                    var register = (IAddonRegister)Activator.CreateInstance(assemblyType);
                    register.Regist(modelBuilder);
                }

            //创建模型配置
            CreateModel(modelBuilder);
            //开始创建模型
            Model = modelBuilder.Build(new StorageStructMappingExecutor(CreateStorageStructMappingProvider));

            return Model;
        }

        /// <summary>
        ///     使用指定的建模器创建对象数据模型。
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected abstract void CreateModel(ModelBuilder modelBuilder);

        /// <summary>
        ///     由派生类实现，获取指定存储标记对应的存储提供程序。
        /// </summary>
        /// <returns>存储提供程序。</returns>
        /// <param name="symbol"></param>
        private IStorageProvider GetStorageProvider(StorageSymbol symbol)
        {
            //调用CreateStorageProvider(StorageSymbol,
            // ObjectDataModel)生成提供程序实例。如果模型未生成，则调用CreateModel()方法生成模型。
            // 建立一个内部字典用于寄存已生成的提供程序实例，其键为StorageSymbol。调用CreateStorageProvider之
            // 前先查询该字典，如果已存在则直接返回。如果已开启本地事务，且所需提供程序在字典中不存在，
            // 则引发异常“你已启用本地事务，不能再创建另一个存储提供程序实例。如果需要多个存
            // 储提供程序实例，可以使用环境事务”。

            if (_storageProviders.TryGetValue(symbol, out var storageProvider))
                return storageProvider;

            //不存在
            if (_storageProviders.Values.Any(p => p.TransactionBegun))
                throw new InvalidOperationException("你已启用本地事务，不能再创建另一个存储提供程序实例。如果需要多个存储提供程序实例，可以使用环境事务");
            var model = CreateModel();
            var provider = CreateStorageProvider(symbol, model);
            _storageProviders[symbol] = provider;
            return provider;
        }

        /// <summary>
        ///     创建面向指定存储服务的存储结构映射提供程序。
        ///     实施说明
        ///     默认不执行任何操作，派生类可以重写此方法创建所需的提供程序。
        /// </summary>
        /// <param name="storageSymbol">存储标记。</param>
        protected virtual IStorageStructMappingProvider CreateStorageStructMappingProvider(StorageSymbol storageSymbol)
        {
            return null;
        }

        /// <summary>
        ///     由派生类实现，创建指定存储标记对应的存储提供程序。
        /// </summary>
        /// <returns>存储提供程序。</returns>
        /// <param name="symbol">存储标记。</param>
        /// <param name="model">对象数据模型。</param>
        protected abstract IStorageProvider CreateStorageProvider(StorageSymbol symbol, ObjectDataModel model);
    }
}