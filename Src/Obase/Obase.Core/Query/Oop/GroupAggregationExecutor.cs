/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：分组运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:10:34
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     分组（聚合）运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型。</typeparam>
    /// <typeparam name="TKey">分组键的类型。</typeparam>
    /// <typeparam name="TResult">结果元素的类型。</typeparam>
    public class GroupAggregationExecutor<TSource, TKey, TResult> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算
        /// </summary>
        private readonly GroupAggregationOp _op;

        /// <summary>
        ///     构造GroupExecutor的实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public GroupAggregationExecutor(GroupAggregationOp op)
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
            var resultSelector =
                (Func<TKey, IEnumerable<TSource>, TResult>)ExpressionDelegates.Current[_op.ResultSelector];
            //根据是否有比较器调用不同的重载
            if (_op.Comparer is IEqualityComparer<TKey> comparer)
                context.Result = result.GroupBy(keySelector, resultSelector, comparer);
            else
                context.Result = result.GroupBy(keySelector, resultSelector);
            //调用管道的下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }


    /// <summary>
    ///     分组（聚合）运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型。</typeparam>
    /// <typeparam name="TKey">分组键的类型。</typeparam>
    /// <typeparam name="TElement">组元素的类型。</typeparam>
    /// <typeparam name="TResult">结果元素的类型。</typeparam>
    public class GroupAggregationExecutor<TSource, TKey, TElement, TResult> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算
        /// </summary>
        private readonly GroupAggregationOp _op;

        /// <summary>
        ///     构造GroupExecutor的实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public GroupAggregationExecutor(GroupAggregationOp op)
            : base(op)
        {
            _op = op;
        }

        /// <summary>
        ///     执行操作
        /// </summary>
        /// <param name="context">对象管道上下文</param>
        public override void Execute(OopContext context)
        {
            var result = (IEnumerable<TSource>)context.Result;
            var keySelector = (Func<TSource, TKey>)ExpressionDelegates.Current[_op.KeySelector];
            var elementSelector = (Func<TSource, TElement>)ExpressionDelegates.Current[_op.ElementSelector];
            var resultSelector =
                (Func<TKey, IEnumerable<TElement>, TResult>)ExpressionDelegates.Current[_op.ResultSelector];
            //根据是否有比较器调用不同的重载
            if (_op.Comparer is IEqualityComparer<TKey> comparer)
                context.Result = result.GroupBy(keySelector, elementSelector, resultSelector, comparer);
            else
                context.Result = result.GroupBy(keySelector, elementSelector, resultSelector);
            //调用管道的下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}