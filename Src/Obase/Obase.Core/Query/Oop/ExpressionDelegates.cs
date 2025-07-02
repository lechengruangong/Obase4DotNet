/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：表达式委托库.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 16:26:20
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     表达式委托库，存储表达式经编译生成的委托，可以根据表达式获取委托。
    /// </summary>
    public class ExpressionDelegates
    {
        /// <summary>
        ///     表达式经编译生成的委托。
        /// </summary>
        private readonly Dictionary<Expression, Delegate> _delegates = new Dictionary<Expression, Delegate>();

        /// <summary>
        ///     锁对象
        /// </summary>
        private readonly object _lockObj = new object();

        /// <summary>
        ///     初始化ExpressionDelegates的新实例。
        /// </summary>
        private ExpressionDelegates()
        {
            //私有构造
        }

        /// <summary>
        ///     获取当前应用程序域中的ExpressionDelegates实例。
        /// </summary>
        public static ExpressionDelegates Current => new ExpressionDelegates();

        /// <summary>
        ///     获取指定表达式经编译生成的委托。
        /// </summary>
        /// <param name="expression">表达式。</param>
        public Delegate this[Expression expression]
        {
            get
            {
                lock (_lockObj)
                {
                    //在锁内操作
                    if (!_delegates.ContainsKey(expression))
                        _delegates.Add(expression, ((LambdaExpression)expression).Compile());
                    return _delegates[expression];
                }
            }
        }
    }
}