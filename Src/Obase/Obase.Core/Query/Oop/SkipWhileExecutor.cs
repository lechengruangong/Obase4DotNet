/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：条件略过运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:39:13
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     条件略过运算执行器。
    ///     类型参数：
    ///     TSource		源对象的类型
    ///     实施建议：
    ///     首先从上下文获取查询源，强制转换为IEnumerable{TSource}，然后调用相应的扩展方法（注意分情况选择不同的重载），最后将结果存储于上下文。
    /// </summary>
    public class SkipWhileExecutor<TSource> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算
        /// </summary>
        private readonly SkipWhileOp _op;

        /// <summary>
        ///     构造SkipWhileExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public SkipWhileExecutor(SkipWhileOp op)
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
            //从缓存中取条件
            var argDelegate = ExpressionDelegates.Current[_op.Predicate];
            if (argDelegate is Func<TSource, bool> func)
                context.Result = result.SkipWhile(func);
            //调用管道下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}