/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：与就地修改相关的事件的数据类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-25 17:45:26
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     与就地修改相关的事件的数据类。
    /// </summary>
    public abstract class DirectlyChangingEventArgs : EventArgs
    {
        /// <summary>
        ///     修改类型。
        /// </summary>
        private readonly EDirectlyChangeType _changeType;

        /// <summary>
        ///     条件表达式。
        /// </summary>
        private readonly Expression _expression;

        /// <summary>
        ///     存储属性新值的字典，键为属性名称，值为属性的新值。
        /// </summary>
        private readonly KeyValuePair<string, object>[] _newValues;

        /// <summary>
        ///     修改的对象类型
        /// </summary>
        private readonly Type _type;


        /// <summary>
        ///     创建DirectlyChangingEventArgs实例，并指定条件表达式和属性新值字典。
        /// </summary>
        /// <param name="expression">条件表达式。</param>
        /// <param name="changeType">修改类型</param>
        /// <param name="objectType">修改的对象类型</param>
        /// <param name="newValues">属性新值字典。</param>
        protected DirectlyChangingEventArgs(Expression expression, EDirectlyChangeType changeType, Type objectType,
            KeyValuePair<string, object>[] newValues = null)
        {
            _expression = expression;
            _changeType = changeType;
            _type = objectType;
            _newValues = newValues;
        }

        /// <summary>
        ///     获取条件表达式。
        /// </summary>
        public Expression Expression => _expression;

        /// <summary>
        ///     获取修改类型。
        /// </summary>
        public EDirectlyChangeType ChangeType => _changeType;

        /// <summary>
        ///     获取存储属性新值的字典，键为属性名称，值为属性的新值。
        /// </summary>
        public KeyValuePair<string, object>[] NewValues => _newValues;

        /// <summary>
        ///     修改的对象类型
        /// </summary>
        public Type Type => _type;
    }
}