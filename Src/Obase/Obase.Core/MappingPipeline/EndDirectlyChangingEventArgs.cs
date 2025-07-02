/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：结束就地修改事件数据类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 09:59:54
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     结束就地修改事件数据类
    /// </summary>
    public class EndDirectlyChangingEventArgs : DirectlyChangingEventArgs
    {
        /// <summary>
        ///     影响的行数
        /// </summary>
        private readonly int _affectedCount;

        /// <summary>
        ///     执行过程中发生的异常，如果执行成功则值为NULL。
        /// </summary>
        private readonly Exception _exception;


        /// <summary>
        ///     创建EndDirectlyChangingEventArgs实例，并指定过滤条件表达式和执行过程中发生的异常。
        /// </summary>
        /// <param name="expression">条件表达式。</param>
        /// <param name="changeType">修改类型</param>
        /// <param name="objectType">修改的对象类型</param>
        /// <param name="affectedCount">影响行数</param>
        /// <param name="newValues">属性新值字典。</param>
        /// <param name="exception">执行过程中发生的异常。</param>
        public EndDirectlyChangingEventArgs(Expression expression, EDirectlyChangeType changeType, Type objectType,
            int affectedCount,
            KeyValuePair<string, object>[] newValues = null, Exception exception = null) : base(expression, changeType,
            objectType,
            newValues)
        {
            _affectedCount = affectedCount;
            _exception = exception;
        }


        /// <summary>
        ///     获取执行过程中发生的异常，如果执行成功则值为NULL。
        /// </summary>
        public Exception Exception => _exception;

        /// <summary>
        ///     获取一个值，该值指示执行过程中是否发生了异常。
        /// </summary>
        public bool Failed => _exception != null;

        /// <summary>
        ///     影响的行数
        /// </summary>
        public int AffectedCount => _affectedCount;
    }
}