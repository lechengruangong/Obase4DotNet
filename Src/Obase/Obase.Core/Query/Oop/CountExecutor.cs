/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Count运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 16:59:34
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     Count运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型。</typeparam>
    public class CountExecutor<TSource> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算。
        /// </summary>
        private readonly CountOp _op;

        /// <summary>
        ///     构造CountExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public CountExecutor(CountOp op)
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
            if (context.Result is IEnumerable<TSource> source)
            {
                Func<TSource, bool> predicate = null;
                //先从缓存取
                if (_op.Predicate != null)
                    predicate = (Func<TSource, bool>)ExpressionDelegates.Current[_op.Predicate];
                //根据是否有条件调用不同重载
                context.Result = predicate != null ? source.Count(predicate) : source.Count();
            }
            else
            {
                context.Result = Convert.ToInt32(context.Result);
            }

            //调用管道的下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}