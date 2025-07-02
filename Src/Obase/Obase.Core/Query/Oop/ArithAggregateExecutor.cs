/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：聚合运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 16:53:10
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
    /// <typeparam name="TSource">源对象的类型。</typeparam>
    /// <typeparam name="TResult">聚合结果的类型，也是投影函数（或有）的返回类型。</typeparam>
    public class ArithAggregateExecutor<TSource, TResult> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算
        /// </summary>
        private readonly ArithAggregateOp _op;

        /// <summary>
        ///     构造AggregateExecutor的实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public ArithAggregateExecutor(ArithAggregateOp op)
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
            switch (_op.Operator)
            {
                // 求和。
                case EAggregationOperator.Sum:
                    if (_op.Selector == null)
                    {
                        //非可空
                        if (context.Result is IEnumerable<float> fSource)
                            context.Result = fSource.Sum();
                        else if (context.Result is IEnumerable<long> lSource)
                            context.Result = lSource.Sum();
                        else if (context.Result is IEnumerable<int> iSource)
                            context.Result = iSource.Sum();
                        else if (context.Result is IEnumerable<double> dSource)
                            context.Result = dSource.Sum();
                        else if (context.Result is IEnumerable<decimal> decSource)
                            context.Result = decSource.Sum();

                        //可空
                        else if (context.Result is IEnumerable<float?> fnSource)
                            context.Result = fnSource.Sum();
                        else if (context.Result is IEnumerable<long?> lnSource)
                            context.Result = lnSource.Sum();
                        else if (context.Result is IEnumerable<int?> inSource)
                            context.Result = inSource.Sum();
                        else if (context.Result is IEnumerable<double?> dnSource)
                            context.Result = dnSource.Sum();
                        else if (context.Result is IEnumerable<decimal?> decnSource)
                            context.Result = decnSource.Sum();
                    }
                    else
                    {
                        var selector = ExpressionDelegates.Current[_op.Predicate];
                        //非可空
                        if (selector is Func<TSource, float> fSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Sum(fSelector);
                        else if (selector is Func<TSource, long> lSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Sum(lSelector);
                        else if (selector is Func<TSource, int> iSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Sum(iSelector);
                        else if (selector is Func<TSource, double> dSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Sum(dSelector);
                        else if (selector is Func<TSource, decimal> decSSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Sum(decSSelector);

                        //可空
                        else if (selector is Func<TSource, float?> fnSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Sum(fnSelector);
                        else if (selector is Func<TSource, long?> lnSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Sum(lnSelector);
                        else if (selector is Func<TSource, int?> inSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Sum(inSelector);
                        else if (selector is Func<TSource, double?> dnSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Sum(dnSelector);
                        else if (selector is Func<TSource, decimal?> decnSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Sum(decnSelector);
                    }

                    break;
                // 求平均数。
                case EAggregationOperator.Average:
                    if (_op.Selector == null)
                    {
                        //非可空
                        if (context.Result is IEnumerable<float> fSource)
                            context.Result = fSource.Average();
                        else if (context.Result is IEnumerable<long> lSource)
                            context.Result = lSource.Average();
                        else if (context.Result is IEnumerable<int> iSource)
                            context.Result = iSource.Average();
                        else if (context.Result is IEnumerable<double> dSource)
                            context.Result = dSource.Average();
                        else if (context.Result is IEnumerable<decimal> decSource)
                            context.Result = decSource.Average();

                        //可空
                        else if (context.Result is IEnumerable<float?> fnSource)
                            context.Result = fnSource.Average();
                        else if (context.Result is IEnumerable<long?> lnSource)
                            context.Result = lnSource.Average();
                        else if (context.Result is IEnumerable<int?> inSource)
                            context.Result = inSource.Average();
                        else if (context.Result is IEnumerable<double?> dnSource)
                            context.Result = dnSource.Average();
                        else if (context.Result is IEnumerable<decimal?> decnSource)
                            context.Result = decnSource.Average();
                    }
                    else
                    {
                        var selector = ExpressionDelegates.Current[_op.Predicate];
                        //非可空
                        if (selector is Func<TSource, float> fSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Average(fSelector);
                        else if (selector is Func<TSource, long> lSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Average(lSelector);
                        else if (selector is Func<TSource, int> iSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Average(iSelector);
                        else if (selector is Func<TSource, double> dSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Average(dSelector);
                        else if (selector is Func<TSource, decimal> decSSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Average(decSSelector);

                        //可空
                        else if (selector is Func<TSource, float?> fnSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Average(fnSelector);
                        else if (selector is Func<TSource, long?> lnSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Average(lnSelector);
                        else if (selector is Func<TSource, int?> inSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Average(inSelector);
                        else if (selector is Func<TSource, double?> dnSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Average(dnSelector);
                        else if (selector is Func<TSource, decimal?> decnSelector)
                            context.Result = ((IEnumerable<TSource>)context.Result).Average(decnSelector);
                    }

                    break;
                // 取最大值。
                case EAggregationOperator.Max:
                    if (_op.Selector != null)
                        context.Result =
                            ((IEnumerable<TSource>)context.Result).Max(
                                (Func<TSource, TResult>)ExpressionDelegates.Current[_op.Selector]);
                    else
                        context.Result = ((IEnumerable<TSource>)context.Result).Max();
                    break;

                // 取最小值。
                case EAggregationOperator.Min:
                    if (_op.Selector != null)
                        context.Result =
                            ((IEnumerable<TSource>)context.Result).Min(
                                (Func<TSource, TResult>)ExpressionDelegates.Current[_op.Selector]);
                    else
                        context.Result = ((IEnumerable<TSource>)context.Result).Min();
                    break;
            }

            //执行管道下一节的运算
            (_next as OopExecutor)?.Execute(context);
        }
    }
}