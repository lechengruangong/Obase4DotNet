/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：为所有的冲突处理器工厂提供基础实现.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 14:54:25
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     为所有的冲突处理器工厂提供基础实现。
    /// </summary>
    public abstract class ConcurrentConflictHandlerFactory
    {
        /// <summary>
        ///     用于探测属性值是否已更改的委托。
        /// </summary>
        private Func<object, string, bool> _attributeHasChanged;

        /// <summary>
        ///     用于获取属性原值的委托。
        /// </summary>
        private GetAttributeValue _attributeOriginalValueGetter;

        /// <summary>
        ///     对象数据模型。
        /// </summary>
        private ObjectDataModel _model;

        /// <summary>
        ///     在冲突处理过程中实施持久化的存储提供程序。
        /// </summary>
        private IStorageProvider _storageProvider;

        /// <summary>
        ///     获取或设置对象数据模型。
        /// </summary>
        public ObjectDataModel Model
        {
            get => _model;
            set => _model = value;
        }

        /// <summary>
        ///     获取或设置用于探测属性值是否已更改的委托。
        /// </summary>
        public Func<object, string, bool> AttributeHasChanged
        {
            get => _attributeHasChanged;
            set => _attributeHasChanged = value;
        }

        /// <summary>
        ///     获取或设置用于获取属性原值的委托。
        /// </summary>
        public GetAttributeValue AttributeOriginalValueGetter
        {
            get => _attributeOriginalValueGetter;
            set => _attributeOriginalValueGetter = value;
        }

        /// <summary>
        ///     在冲突处理过程中实施持久化的存储提供程序。
        /// </summary>
        public IStorageProvider StorageProvider
        {
            get => _storageProvider;
            set => _storageProvider = value;
        }

        /// <summary>
        ///     创建“重复创建”冲突的处理器。
        /// </summary>
        public abstract IRepeatCreationHandler CreateRepeatCreationHandler();

        /// <summary>
        ///     创建版本冲突的处理器。
        /// </summary>
        public abstract IVersionConflictHandler CreateVersionConflictHandler();

        /// <summary>
        ///     创建“更新幻影”冲突的处理器。
        /// </summary>
        public abstract IUpdatingPhantomHandler CreateUpdatingPhantomHandler();

        /// <summary>
        ///     根据指定的并发冲突处理策略选取相应的冲突处理器工厂。
        ///     如果指定的处理策略为“忽略”，返回null。
        /// </summary>
        /// <param name="strategy">冲突处理策略。</param>
        public static ConcurrentConflictHandlerFactory ChooseFactory(EConcurrentConflictHandlingStrategy strategy)
        {
            ConcurrentConflictHandlerFactory factory = null;
            switch (strategy)
            {
                case EConcurrentConflictHandlingStrategy.Ignore:
                    break;
                case EConcurrentConflictHandlingStrategy.ThrowException:
                    factory = new ThrowExceptionConflictHandlerFactory();
                    break;
                case EConcurrentConflictHandlingStrategy.Overwrite:
                    factory = new OverwriteConflictHandlerFactory();
                    break;
                case EConcurrentConflictHandlingStrategy.Combine:
                    factory = new CombineConflictHandlerFactory();
                    break;
                case EConcurrentConflictHandlingStrategy.Reconstruct:
                    factory = new ReconstructConflictHandlerFactory();
                    break;
            }

            return factory;
        }
    }
}