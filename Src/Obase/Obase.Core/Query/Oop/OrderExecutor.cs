/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：排序运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:25:13
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     排序运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型。</typeparam>
    /// <typeparam name="TKey">排序键的类型。</typeparam>
    public class OrderExecutor<TSource, TKey> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算
        /// </summary>
        private readonly OrderOp _op;

        /// <summary>
        ///     构造OrderExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public OrderExecutor(OrderOp op)
            : base(op)
        {
            _op = op;
        }

        /// <summary>
        ///     执行运算
        /// </summary>
        /// <param name="context">运算上下文</param>
        public override void Execute(OopContext context)
        {
            var result = (IEnumerable<TSource>)context.Result;
            var keySelector = (Func<TSource, TKey>)ExpressionDelegates.Current[_op.KeySelector];
            IComparer<TKey> comparer = null;
            if (_op.Comparer is IComparer<TKey> cm)
                comparer = cm;
            //反序
            if (_op.Descending)
            {
                //不清除
                if (context.Result is IOrderedEnumerable<TSource> orderedSource && !_op.ClearPrevious)
                    context.Result = comparer != null
                        ? orderedSource.ThenByDescending(keySelector, comparer)
                        : orderedSource.ThenByDescending(keySelector);
                else
                    context.Result = comparer != null
                        ? result.OrderByDescending(keySelector, comparer)
                        : result.OrderByDescending(keySelector);
            }
            //正序
            else
            {
                //不清除
                if (context.Result is IOrderedEnumerable<TSource> orderedSource && !_op.ClearPrevious)
                    context.Result = comparer != null
                        ? orderedSource.ThenBy(keySelector, comparer)
                        : orderedSource.ThenBy(keySelector);
                else
                    context.Result = comparer != null
                        ? result.OrderBy(keySelector, comparer)
                        : result.OrderBy(keySelector);
            }

            //调用管道下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}