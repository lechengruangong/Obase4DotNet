/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：查询上下文.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:24:35
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     查询上下文
    /// </summary>
    public class QueryContext
    {
        /// <summary>
        ///     表示查询表达式。
        /// </summary>
        private readonly Expression _expression;

        /// <summary>
        ///     受影响的行数。
        /// </summary>
        private int _affectedCount;

        /// <summary>
        ///     要执行的存储指令（如Sql语句）。
        /// </summary>
        private object _command;

        /// <summary>
        ///     执行Sql语句的过程中发生的异常，未发生异常则为Null。
        /// </summary>
        private Exception _exception;

        /// <summary>
        ///     指示查询操作是否已被取消。
        /// </summary>
        private bool _hasCanceled;

        /// <summary>
        ///     要执行的查询。
        /// </summary>
        private QueryOp _query;

        /// <summary>
        ///     查询结果。
        /// </summary>
        private object _result;

        /// <summary>
        ///     执行Sql语句所消耗的时间，以毫秒为单位。
        /// </summary>
        private int _timeConsumed;

        /// <summary>
        ///     在查询过程中由用户自定义的状态信息。
        /// </summary>
        private string _userState;

        /// <summary>
        ///     实例化QueryContext的新实例。
        /// </summary>
        /// <param name="query">要执行的查询。</param>
        /// <param name="expression">查询表达式。</param>
        public QueryContext(QueryOp query, Expression expression = null)
        {
            _query = query;
            _expression = expression;
        }

        /// <summary>
        ///     受影响的行数。
        /// </summary>
        public int AffectedCount
        {
            get => _affectedCount;
            set => _affectedCount = value;
        }

        /// <summary>
        ///     要执行的存储指令（如Sql语句）。
        /// </summary>
        public object Command
        {
            get => _command;
            set => _command = value;
        }

        /// <summary>
        ///     执行Sql语句的过程中发生的异常，未发生异常则为Null。
        /// </summary>
        public Exception Exception
        {
            get => _exception;
            set => _exception = value;
        }

        /// <summary>
        ///     表示查询表达式。
        /// </summary>
        public Expression Expression => _expression;

        /// <summary>
        ///     指示查询操作是否已被取消。
        /// </summary>
        public bool HasCanceled
        {
            get => _hasCanceled;
            set => _hasCanceled = value;
        }

        /// <summary>
        ///     查询结果。
        /// </summary>
        public object Result
        {
            get => _result;
            set => _result = value;
        }

        /// <summary>
        ///     执行Sql语句所消耗的时间，以毫秒为单位。
        /// </summary>
        public int TimeConsumed
        {
            get => _timeConsumed;
            set => _timeConsumed = value;
        }

        /// <summary>
        ///     在查询过程中由用户自定义的状态信息。
        /// </summary>
        public string UserState
        {
            get => _userState;
            set => _userState = value;
        }

        /// <summary>
        ///     要执行的查询。
        /// </summary>
        public QueryOp Query
        {
            get => _query;
            set => _query = value;
        }
    }
}