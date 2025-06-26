/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表示表达式非法的异常.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 11:46:29
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Linq.Expressions;

namespace Obase.Core.Query
{
    /// <summary>
    ///     表示表达式非法的异常
    /// </summary>
    public class ExpressionIllegalException : Exception
    {
        /// <summary>
        ///     被判定为非法的表达式
        /// </summary>
        private readonly Expression _expression;

        /// <summary>
        ///     表达式不合法的原因。
        /// </summary>
        private readonly string _reason;

        /// <summary>
        ///     构造ExpressionIllegalException的新实例。
        /// </summary>
        /// <param name="expression">被判定为非法的表达式。</param>
        /// <param name="reason">表达式不合法的原因。</param>
        public ExpressionIllegalException(Expression expression, string reason = "表达式不符合规范")
        {
            _expression = expression;
            _reason = reason;

            Message = $"{expression}{reason}";
        }

        /// <summary>
        ///     获取被判定为非法的表达式。
        /// </summary>
        public Expression Expression => _expression;

        /// <summary>
        ///     获取表达式不合法的原因。
        /// </summary>
        public string Reason => _reason;

        /// <summary>
        ///     获取异常消息。重写Exception.Message。
        /// </summary>
        public override string Message { get; }
    }
}