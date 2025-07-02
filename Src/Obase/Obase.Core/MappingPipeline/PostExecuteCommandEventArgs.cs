/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：PostExecuteCommand事件数据类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:21:58
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     PostExecuteCommand事件数据类。
    /// </summary>
    public class PostExecuteCommandEventArgs : EventArgs
    {
        /// <summary>
        ///     受影响的行数。
        /// </summary>
        private readonly int _affectedCount;

        /// <summary>
        ///     要执行的存储指令（如Sql语句）。
        /// </summary>
        private readonly object _command;

        /// <summary>
        ///     执行Sql语句的过程中发生的异常，未发生异常则为Null。
        /// </summary>
        private readonly Exception _exception;

        /// <summary>
        ///     执行Sql语句所消耗的时间，以毫秒为单位。
        /// </summary>
        private readonly int _timeConsumed;

        /// <summary>
        ///     在查询管道中，表示查询表达式；对于其它管道，该属性为NULL。
        /// </summary>
        private Expression _expression;

        /// <summary>
        ///     创建PostExecuteSqlEventArgs实例，并指定要执行的存储指令（如Sql语句）和执行消耗的时间。
        /// </summary>
        /// <param name="command">要执行的存储指令（如Sql语句）。</param>
        /// <param name="timeConsumed">执行指令所消耗的时间，以毫秒为单位。</param>
        /// <param name="affectedCount">受影响的行数。</param>
        public PostExecuteCommandEventArgs(object command, int timeConsumed, int affectedCount)
        {
            _command = command;
            _timeConsumed = timeConsumed;
            _affectedCount = affectedCount;
        }

        /// <summary>
        ///     创建PostExecuteSqlEventArgs实例，并指定要执行的存储指令（如Sql语句）、执行消耗的时间、以及执行过程中发生的异常。
        /// </summary>
        /// <param name="command">要执行的存储指令（如Sql语句）。</param>
        /// <param name="timeConsumed">执行指令所消耗的时间，以毫秒为单位。</param>
        /// <param name="exception">执行指令过程中发生的异常</param>
        public PostExecuteCommandEventArgs(object command, int timeConsumed, Exception exception)
        {
            _command = command;
            _timeConsumed = timeConsumed;
            _exception = exception;
        }

        /// <summary>
        ///     获取执行Sql语句所消耗的时间，以毫秒为单位。
        /// </summary>
        public int TimeConsumed => _timeConsumed;

        /// <summary>
        ///     获取执行Sql语句的过程中发生的异常，未发生异常则为Null。
        /// </summary>
        public Exception Exception => _exception;

        /// <summary>
        ///     在查询管道中，获取或设置查询表达式；对于其它管道，该属性为NULL。
        /// </summary>
        public Expression Expression
        {
            get => _expression;
            set => _expression = value;
        }

        /// <summary>
        ///     获取受影响的行数。
        /// </summary>
        public int AffectedCount => _affectedCount;

        /// <summary>
        ///     获取要执行的存储指令（如Sql语句）。
        /// </summary>
        public object Command => _command;
    }
}