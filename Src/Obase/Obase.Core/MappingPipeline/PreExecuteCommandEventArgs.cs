/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：PreExecuteCommandEventArgs事件的数据类.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 10:23:51
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;

namespace Obase.Core.MappingPipeline
{
    /// <summary>
    ///     PreExecuteCommandEventArgs事件的数据类。
    /// </summary>
    public class PreExecuteCommandEventArgs : EventArgs
    {
        /// <summary>
        ///     要执行的存储指令（如Sql语句）。
        /// </summary>
        private readonly object _command;

        /// <summary>
        ///     在查询管道中，表示查询表达式；对于其它管道，该属性为NULL。
        /// </summary>
        private Expression _expression;


        /// <summary>
        ///     创建PreExecuteCommandEventArgs实例。
        /// </summary>
        /// <param name="command">要执行的存储指令（如Sql语句）。</param>
        public PreExecuteCommandEventArgs(object command)
        {
            _command = command;
        }

        /// <summary>
        ///     在查询管道中，获取或设置查询表达式；对于其它管道，该属性为NULL。
        /// </summary>
        public Expression Expression
        {
            get => _expression;
            set => _expression = value;
        }

        /// <summary>
        ///     获取要执行的Sql语句。
        /// </summary>
        /// <summary>
        ///     获取要执行的存储指令（如Sql语句）。
        /// </summary>
        public object Command => _command;
    }
}