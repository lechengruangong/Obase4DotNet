/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：全局模型缓存.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-30 15:51:20
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Obase.Core.Odm;

namespace Obase.Core
{
    /// <summary>
    ///     全局模型缓存,用于缓存对象上下文创建的模型，避免重复创建。
    ///     每个具体的上下文类对应一个对象数据模型，不论应用程序域中该类型有多少实例，只有在第一个实例初始化时才会生成模型，后续所有实例将使用该模型。
    ///     实施说明
    ///     （1）模型的建造过程不持有任何锁。模型建造会执行用户注册的中间件（类型解析器、代理类型生成器、补充配置器）、隐含类型发射以及存储结构映射，
    ///     如果在锁内建造，一旦这些过程引发异常，锁将无法释放，导致同一线程后续取锁抛出LockRecursionException、其它线程永久阻塞。
    ///     （2）每个上下文类型对应一个Lazy对象，由运行库保证模型只被建造一次；并发调用时未中选的Lazy不会被执行，因此不会出现重复建造。
    /// </summary>
    public class GlobalModelCache
    {
        /// <summary>
        ///     单例对象
        /// </summary>
        private static readonly Lazy<GlobalModelCache> LazyCurrent =
            new Lazy<GlobalModelCache>(() => new GlobalModelCache(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        ///     对象上下文类对象数据模型缓存
        /// </summary>
        private readonly ConcurrentDictionary<Type, Lazy<ObjectDataModel>> _models =
            new ConcurrentDictionary<Type, Lazy<ObjectDataModel>>();

        /// <summary>
        ///     创建全局模型缓存实例。
        /// </summary>
        private GlobalModelCache()
        {
        }

        /// <summary>
        ///     获取全局模型缓存实例。本属性确保应用程序域中有且仅有一个全局模型缓存实例。
        /// </summary>
        public static GlobalModelCache Current => LazyCurrent.Value;

        /// <summary>
        ///     从缓存中取出指定上下文类的模型。如果模型中不存在该上下文类的模型（或该模型尚在建造中）则返回null。
        /// </summary>
        /// <param name="contextType">具体的对象上下文类型。</param>
        public ObjectDataModel GetModel(Type contextType)
        {
            if (!_models.TryGetValue(contextType, out var lazy)) return null;
            //建造中或建造失败时不返回任何模型，与原有语义一致
            return lazy.IsValueCreated ? lazy.Value : null;
        }

        /// <summary>
        ///     将对象数据模型放入全局缓存。
        ///     说明
        ///     模型由第一个调用者建造，建造过程不持有本缓存的任何锁，同一上下文类型的并发调用者会等待该次建造完成后取得同一个模型实例。
        ///     如果建造过程引发异常，异常会抛给所有调用者，同时移除本次建造，保证修正问题后可以重新建造，也不会造成锁泄漏。
        /// </summary>
        /// <param name="contextType">具体的对象上下文类型。</param>
        /// <param name="provider">要放入缓存的对象数据模型提供器。</param>
        public void SetModel(Type contextType, ContextConfigProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            //此处的GetOrAdd在并发时可能创建多个Lazy，但只有被放入字典的那一个会被执行，因此模型只会被建造一次
            var lazy = _models.GetOrAdd(contextType,
                type => new Lazy<ObjectDataModel>(provider.CreateModel,
                    LazyThreadSafetyMode.ExecutionAndPublication));

            try
            {
                //触发或等待本次建造完成，此处不持有任何锁
                var model = lazy.Value;
                if (model == null)
                    throw new InvalidOperationException($"上下文{contextType.FullName}的对象数据模型建造失败");
            }
            catch
            {
                //建造失败：移除本次失败的建造（只移除自己这一个），使后续调用可以重新尝试建造
                ((ICollection<KeyValuePair<Type, Lazy<ObjectDataModel>>>)_models)
                    .Remove(new KeyValuePair<Type, Lazy<ObjectDataModel>>(contextType, lazy));
                throw;
            }
        }
    }
}