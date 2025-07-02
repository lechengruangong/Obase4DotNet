/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：全局模型缓存.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:51:20
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Threading;
using Obase.Core.Odm;

namespace Obase.Core
{
    /// <summary>
    ///     全局模型缓存,用于缓存对象上下文创建的模型，避免重复创建。
    ///     每个具体的上下文类对应一个对象数据模型，不论应用程序域中该类型有多少实例，只有在第一个实例初始化时才会生成模型，后续所有实例将使用该模型。
    /// </summary>
    public class GlobalModelCache
    {
        /// <summary>
        ///     锁对象
        /// </summary>
        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        ///     单例对象
        /// </summary>
        private static volatile GlobalModelCache _current;

        /// <summary>
        ///     对象上下文类对象数据模型缓存
        /// </summary>
        private readonly Dictionary<Type, ObjectDataModel> _models = new Dictionary<Type, ObjectDataModel>();


        /// <summary>
        ///     创建全局模型缓存实例。
        /// </summary>
        private GlobalModelCache()
        {
        }

        /// <summary>
        ///     获取全局模型缓存实例。本属性确保应用程序域中有且仅有一个全局模型缓存实例。
        /// </summary>
        public static GlobalModelCache Current
        {
            get
            {
                if (_current == null)
                    lock (typeof(GlobalModelCache))
                    {
                        if (_current == null) _current = new GlobalModelCache();
                    }

                return _current;
            }
        }

        /// <summary>
        ///     从缓存中取出指定上下文类的模型。
        /// </summary>
        /// <param name="contextType">具体的对象上下文类型。</param>
        public ObjectDataModel GetModel(Type contextType)
        {
            ReaderWriterLock.EnterReadLock();
            var result = _models.TryGetValue(contextType, out var model) ? model : null;
            ReaderWriterLock.ExitReadLock();
            return result;
        }

        /// <summary>
        ///     将对象数据模型放入全局缓存。
        /// </summary>
        /// <param name="contextType">具体的对象上下文类型。</param>
        /// <param name="provider">要放入缓存的对象数据模型提供器。</param>
        public void SetModel(Type contextType, ContextConfigProvider provider)
        {
            if (!_models.ContainsKey(contextType))
            {
                ReaderWriterLock.EnterWriteLock();
                _models[contextType] = provider.CreateModel();
                ReaderWriterLock.ExitWriteLock();
            }
        }
    }
}