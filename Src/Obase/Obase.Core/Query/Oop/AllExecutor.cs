/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：All测定运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 16:29:55
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     All测定运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型</typeparam>
    public class AllExecutor<TSource> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算。
        /// </summary>
        private readonly AllOp _op;

        /// <summary>
        ///     构造AllExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public AllExecutor(AllOp op)
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
                //有条件 先从缓存里取条件
                var predicate = (Func<TSource, bool>)ExpressionDelegates.Current[_op.Predicate];
                context.Result = source.All(predicate);
            }
            else
            {
                //ALL运算 查询条数＞0 则认为失败
                context.Result = Convert.ToInt32(context.Result) <= 0;
            }

            //执行管道下一节运算
            (_next as OopExecutor)?.Execute(context);
        }
    }
}