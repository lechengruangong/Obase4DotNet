/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：序列化对象数据模型,此模型全局应只有一个.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-3-25 15:56:25
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Obase.Core.Odm.Serialization
{
    /// <summary>
    ///     序列化对象数据模型
    /// </summary>
    public class SerializationObjectDataModel
    {
        /// <summary>
        ///     锁对象
        /// </summary>
        private static readonly ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        ///     clr类型与模型字典
        /// </summary>
        private readonly Dictionary<Type, SerializationEntity> _structuralTypes =
            new Dictionary<Type, SerializationEntity>();

        /// <summary>
        ///     获取模型类型集合
        /// </summary>
        public List<SerializationEntity> Types => _structuralTypes.Values.ToList();

        /// <summary>
        ///     向模型添加类型
        /// </summary>
        /// <param name="modelType">要添加到模型中的类型</param>
        public void AddType(SerializationEntity modelType)
        {
            ReaderWriterLock.EnterWriteLock();
            //覆盖原有的类型
            _structuralTypes[modelType.ClrType] = modelType;
            ReaderWriterLock.ExitWriteLock();
        }

        /// <summary>
        ///     获取指定CLR类型的模型类型
        /// </summary>
        /// <param name="type">CLR类型</param>
        /// <returns>模型类型 不存在则返回空</returns>
        public SerializationEntity GetTypeOrNull(Type type)
        {
            //取出clr类型对应模型
            if (_structuralTypes.TryGetValue(type, out var result))
                return result;
            return null;
        }
    }
}