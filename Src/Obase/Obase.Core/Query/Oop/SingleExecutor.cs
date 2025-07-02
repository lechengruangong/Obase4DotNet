/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：Single索引运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:36:52
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     Single索引运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型.</typeparam>
    public class SingleExecutor<TSource> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算。
        /// </summary>
        private readonly SingleOp _op;


        /// <summary>
        ///     构造SingleExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public SingleExecutor(SingleOp op)
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
            Func<TSource, bool> predicate = null;
            if (_op.Predicate != null) predicate = (Func<TSource, bool>)ExpressionDelegates.Current[_op.Predicate];
            //根据是否返回默认值调用重载
            if (_op.ReturnDefault)
                context.Result = predicate != null ? result.SingleOrDefault(predicate) : result.SingleOrDefault();
            else
                context.Result = predicate != null ? result.Single(predicate) : result.Single();
            //调用管道下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}