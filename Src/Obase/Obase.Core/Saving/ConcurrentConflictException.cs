/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：并发冲突异常.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 15:44:31
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;

namespace Obase.Core.Saving
{
    /// <summary>
    ///     为并发冲突异常提供基础实现。
    /// </summary>
    public abstract class ConcurrentConflictException : Exception
    {
        /// <summary>
        ///     发生并发冲突的对象。
        /// </summary>
        private readonly object _object;

        /// <summary>
        ///     发生并发冲突的对象的类型。
        /// </summary>
        private readonly ObjectType _objectType;

        /// <summary>
        ///     创建ConcurrentConflictException实例。
        /// </summary>
        /// <param name="obj">发生并发冲突的对象。</param>
        /// <param name="objType">发生并发冲突的对象的类型。</param>
        protected ConcurrentConflictException(object obj, ObjectType objType)
        {
            _object = obj;
            _objectType = objType;
        }

        /// <summary>
        ///     获取发生并发冲突的对象。
        /// </summary>
        public object Object => _object;

        /// <summary>
        ///     获取发生并发冲突的对象的类型。
        /// </summary>
        public ObjectType ObjectType => _objectType;

        /// <summary>
        ///     获取发生并发冲突的对象的标识。
        ///     实施说明：使用ObjectSystemVisitor.GetObjectKey方法获取对象标识。
        /// </summary>
        public ObjectKey ObjectKey => ObjectSystemVisitor.GetObjectKey(_object, _objectType);
    }
}