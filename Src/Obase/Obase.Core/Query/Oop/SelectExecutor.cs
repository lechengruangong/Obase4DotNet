/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：投影运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:30:05
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     投影运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型。</typeparam>
    /// <typeparam name="TResult">投影函数的返回值类型。</typeparam>
    public class SelectExecutor<TSource, TResult> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算
        /// </summary>
        private readonly SelectOp _op;

        /// <summary>
        ///     构造SelectExecutor的实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public SelectExecutor(SelectOp op)
            : base(op)
        {
            _op = op;
        }

        /// <summary>
        ///     构造SelectExecutor的实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        internal SelectExecutor(CombiningSelectOp op)
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
            //根据查询类型不同 调用不同的投影方法
            if (_op is CombiningSelectOp)
            {
                var selector = ExpressionDelegates.Current[_op.ResultSelector];
                if (selector is Func<TSource, IEnumerable<TResult>> selector1)
                    context.Result = result.SelectMany(selector1);
                else if (selector is Func<TSource, int, IEnumerable<TResult>> selector2)
                    context.Result = result.SelectMany(selector2);
            }
            else
            {
                var selector = ExpressionDelegates.Current[_op.ResultSelector];

                if (selector is Func<TSource, TResult> selector1)
                    context.Result = result.Select(selector1);
                else if (selector is Func<TSource, int, TResult> selector2) context.Result = result.Select(selector2);
            }

            //调用管道下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }


    /// <summary>
    ///     投影函数的返回值类型。
    /// </summary>
    /// <typeparam name="TSource">投影函数的返回值类型。</typeparam>
    /// <typeparam name="TCollection">投影函数的返回值类型。</typeparam>
    /// <typeparam name="TResult">投影函数的返回值类型。</typeparam>
    public class SelectExecutor<TSource, TCollection, TResult> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算。
        /// </summary>
        private readonly CollectionSelectOp _op;

        /// <summary>
        ///     构造SelectExecutor的实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public SelectExecutor(CollectionSelectOp op)
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
            var resultSelector = (Func<TSource, TCollection, TResult>)ExpressionDelegates.Current[_op.ResultSelector];
            var collectionSelector = ExpressionDelegates.Current[_op.CollectionSelector];
            //根据有没有结果选择器调用重载
            if (collectionSelector is Func<TSource, IEnumerable<TCollection>> selector1)
                context.Result = ((IEnumerable<TSource>)context.Source).SelectMany(selector1, resultSelector);
            else if (collectionSelector is Func<TSource, int, IEnumerable<TCollection>> selector2)
                context.Result = ((IEnumerable<TSource>)context.Source).SelectMany(selector2, resultSelector);
            //调用管道下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}