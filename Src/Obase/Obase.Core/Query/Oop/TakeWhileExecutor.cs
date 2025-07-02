/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：条件提取运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:41:58
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     条件提取运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型。</typeparam>
    public class TakeWhileExecutor<TSource> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算
        /// </summary>
        private readonly TakeWhileOp _op;

        /// <summary>
        ///     构造TakeWhileExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public TakeWhileExecutor(TakeWhileOp op)
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
            var func = ExpressionDelegates.Current[_op.Predicate];
            //有两种不同的委托参数 调用不同的重载
            if (func is Func<TSource, bool> func1)
                context.Result = result.TakeWhile(func1);
            else if (func is Func<TSource, int, bool> func2)
                context.Result = result.TakeWhile(func2);
            //调用管道下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}