/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：与删除对象组相关的事件的数据类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:41:00
└──────────────────────────────────────────────────────────────┘
*/

using System;
using Obase.Core.Odm;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     与删除对象组相关的事件的数据类。
    /// </summary>
    public abstract class DeletingGroupEventArgs : EventArgs
    {
        /// <summary>
        ///     要删除对象的集合。
        /// </summary>
        private readonly object[] _objects;

        /// <summary>
        ///     要删除对象的类型。
        /// </summary>
        private readonly ObjectType _objectType;


        /// <summary>
        ///     创建DeletingGroupEventArgs实例。
        /// </summary>
        /// <param name="objectType">对象组中对象的类型。</param>
        /// <param name="objects">对象组中的对象。</param>
        protected DeletingGroupEventArgs(ObjectType objectType, object[] objects)
        {
            _objectType = objectType;
            _objects = objects;
        }

        /// <summary>
        ///     获取要删除对象的类型。
        /// </summary>
        public ObjectType ObjectType => _objectType;

        /// <summary>
        ///     获取要删除对象的集合。
        /// </summary>
        public object[] Objects => _objects;
    }
}