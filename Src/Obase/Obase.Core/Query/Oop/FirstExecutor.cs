/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：First索引运算执行器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2025-6-26 17:06:34
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Obase.Core.Query.Oop
{
    /// <summary>
    ///     First索引运算执行器。
    /// </summary>
    /// <typeparam name="TSource">源对象的类型。</typeparam>
    public class FirstExecutor<TSource> : OopExecutor
    {
        /// <summary>
        ///     要执行的查询运算。
        /// </summary>
        private readonly FirstOp _op;

        /// <summary>
        ///     构造FirstExecutor的新实例。
        /// </summary>
        /// <param name="op">要执行的查询运算。</param>
        public FirstExecutor(FirstOp op)
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
            //条件部分已转移至管道构建时处理
            if (context.Result is IEnumerable<TSource> source)
                //此处肯定没条件
                context.Result = _op.ReturnDefault ? source.FirstOrDefault() : source.First();
            else
                throw new InvalidOperationException(
                    $"无法将{context.Source.GetType().Name}转换为IEnumerable<{typeof(TSource)}>");
            //调用管道的下一节处理
            (_next as OopExecutor)?.Execute(context);
        }
    }
}