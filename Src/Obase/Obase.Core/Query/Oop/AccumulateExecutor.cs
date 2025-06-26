/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：聚合运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 16:25:47
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     聚合运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型.</typeparam>
    /// <typeparam name="TAccumulate">累加器返回值的类型，也是种子（或有）的类型。</typeparam>
    /// <typeparam name="TResult">结果值的类型。</typeparam>
    public class AccumulateExecutor<TSource, TAccumulate, TResult> : OopExecutor
    {
        /// <summary>
        ///     聚合运算
        /// </summary>
        private readonly AccumulateOp _op;

        /// <summary>
        ///     构造AggregateExecutor的实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public AccumulateExecutor(AccumulateOp op)
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
            //先从缓存中取
            var accumulator = ExpressionDelegates.Current[_op.Accumulator];
            //根据不同的参数个数来执行Aggregate操作
            if (accumulator is Func<TSource, TSource, TSource> func && _op.Seed == null)
            {
                context.Result = ((IEnumerable<TSource>)context.Result).Aggregate(func);
            }
            else if (accumulator is Func<TAccumulate, TSource, TAccumulate> func1 && _op.Seed != null)
            {
                if (_op.ResultSelector != null)
                    context.Result = ((IEnumerable<TSource>)context.Result).Aggregate((TAccumulate)_op.Seed, func1,
                        (Func<TAccumulate, TResult>)ExpressionDelegates.Current[_op.ResultSelector]);
                else
                    context.Result = ((IEnumerable<TSource>)context.Result).Aggregate((TAccumulate)_op.Seed, func1);
            }

            //执行管道的下一节
            (_next as OopExecutor)?.Execute(context);
        }
    }
}